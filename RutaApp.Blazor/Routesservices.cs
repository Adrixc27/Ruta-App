using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dapper;
using MySqlConnector;

namespace RutaApp.Blazor;

public class RutaService
{
    private List<RutaOficial>? _cache;

    public async Task<List<RutaOficial>> ObtenerRutasAsync()
    {
        if (_cache != null) return _cache;

        await using var db = new MySqlConnection(Connection.ConnectionString);

        var rutas = (await db.QueryAsync<RutaRow>(
            @"SELECT r.id_routes                   AS Id,
                     r.name                        AS Nombre,
                     COALESCE(r.grupo, r.name)     AS Grupo,
                     COALESCE(r.variante, 'Único') AS Variante,
                     r.description                 AS Descripcion,
                     r.price                       AS Precio,
                     r.travel_time                 AS TiempoRecorrido,
                     r.estimated_time              AS TiempoEstimado,
                     r.trazo                       AS Trazo,
                     b.name                        AS BusNombre
              FROM routes r
              LEFT JOIN bus b ON b.id_bus = r.id_bus
              WHERE r.active_inactive = 1
              ORDER BY r.id_routes")).ToList();

        var paradas = (await db.QueryAsync<ParadaRow>(
            @"SELECT rs.id_route   AS RutaId,
                     s.id_stop     AS Id,
                     s.name        AS Nombre,
                     s.latitude    AS Latitud,
                     s.longitude   AS Longitud,
                     rs.stop_order AS Orden
              FROM route_stop rs
              INNER JOIN stop s ON s.id_stop = rs.id_stop
              ORDER BY rs.id_route, rs.stop_order")).ToList();

        var paradasPorRuta = paradas.ToLookup(p => p.RutaId);

        _cache = rutas.Select(r =>
        {
            var lista = paradasPorRuta[r.Id]
                .Select(p => new Parada(p.Id, p.Nombre ?? "", p.Latitud, p.Longitud, p.Orden ?? 0))
                .ToList();

            return new RutaOficial
            {
                Id = r.Id,
                Nombre = r.Nombre ?? "",
                Grupo = r.Grupo ?? r.Nombre ?? "",
                Variante = r.Variante ?? "Único",
                BusNombre = r.BusNombre,
                Descripcion = r.Descripcion,
                Precio = r.Precio,
                TiempoRecorrido = r.TiempoRecorrido,
                TiempoEstimado = r.TiempoEstimado,
                Paradas = lista,
                Trazo = string.IsNullOrEmpty(r.Trazo)
                    ? lista.Select(p => new[] { p.Latitud, p.Longitud }).ToList()
                    : Decodificar(r.Trazo)
            };
        }).ToList();

        return _cache;
    }

    /// <summary>Calcula el recorrido por calles con OSRM a partir de las paradas y lo guarda en routes.trazo.
    /// Útil al agregar rutas nuevas.</summary>
    public async Task<int> GenerarTrazoAsync(int idRuta)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);

        var paradas = (await db.QueryAsync<ParadaRow>(
            @"SELECT s.latitude AS Latitud, s.longitude AS Longitud
              FROM route_stop rs
              INNER JOIN stop s ON s.id_stop = rs.id_stop
              WHERE rs.id_route = @idRuta
              ORDER BY rs.stop_order", new { idRuta })).ToList();

        if (paradas.Count < 2) return 0;

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");

        var linea = new List<double[]>();
        const int tamano = 25;

        for (int inicio = 0; inicio < paradas.Count - 1; inicio += tamano - 1)
        {
            var grupo = paradas.Skip(inicio).Take(tamano).ToList();
            if (grupo.Count < 2) break;

            string coords = string.Join(";", grupo.Select(p =>
                p.Longitud.ToString(CultureInfo.InvariantCulture) + "," +
                p.Latitud.ToString(CultureInfo.InvariantCulture)));

            string url = "https://router.project-osrm.org/route/v1/driving/" + coords +
                         "?overview=full&geometries=polyline6";

            var json = await http.GetFromJsonAsync<JsonElement>(url);
            string geometria = json.GetProperty("routes")[0].GetProperty("geometry").GetString()!;

            var puntos = Decodificar(geometria);
            if (linea.Count > 0 && puntos.Count > 0) puntos.RemoveAt(0);
            linea.AddRange(puntos);

            await Task.Delay(1000);
        }

        await db.ExecuteAsync("UPDATE routes SET trazo = @trazo WHERE id_routes = @idRuta",
            new { trazo = Codificar(linea), idRuta });

        LimpiarCache();
        return linea.Count;
    }

    public void LimpiarCache() => _cache = null;

    // ── Polyline con precisión 6 ──
    private static List<double[]> Decodificar(string texto)
    {
        var puntos = new List<double[]>();
        int i = 0, lat = 0, lng = 0;
        while (i < texto.Length)
        {
            lat += SiguienteValor(texto, ref i);
            lng += SiguienteValor(texto, ref i);
            puntos.Add(new[] { lat / 1e6, lng / 1e6 });
        }
        return puntos;
    }

    private static int SiguienteValor(string texto, ref int i)
    {
        int resultado = 0, corrimiento = 0, b;
        do
        {
            b = texto[i++] - 63;
            resultado |= (b & 0x1f) << corrimiento;
            corrimiento += 5;
        } while (b >= 0x20);
        return (resultado & 1) != 0 ? ~(resultado >> 1) : resultado >> 1;
    }

    private static string Codificar(List<double[]> puntos)
    {
        var sb = new StringBuilder();
        long anteriorLat = 0, anteriorLng = 0;
        foreach (var p in puntos)
        {
            long lat = (long)Math.Round(p[0] * 1e6);
            long lng = (long)Math.Round(p[1] * 1e6);
            Escribir(sb, lat - anteriorLat);
            Escribir(sb, lng - anteriorLng);
            anteriorLat = lat;
            anteriorLng = lng;
        }
        return sb.ToString();
    }

    private static void Escribir(StringBuilder sb, long valor)
    {
        valor = valor < 0 ? ~(valor << 1) : valor << 1;
        while (valor >= 0x20)
        {
            sb.Append((char)((0x20 | (valor & 0x1f)) + 63));
            valor >>= 5;
        }
        sb.Append((char)(valor + 63));
    }

    private class RutaRow
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Grupo { get; set; }
        public string? Variante { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public TimeSpan? TiempoRecorrido { get; set; }
        public TimeSpan? TiempoEstimado { get; set; }
        public string? Trazo { get; set; }
        public string? BusNombre { get; set; }
    }

    private class ParadaRow
    {
        public int RutaId { get; set; }
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int? Orden { get; set; }
    }
}