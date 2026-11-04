using System.Collections.Concurrent;
using System.Globalization;

namespace RutaApp.Blazor.Services;

/// <summary>
/// Recorridos por calles (OSRM), nombre de la calle de unas coordenadas (Nominatim), paradas cercanas (Overpass)
/// y búsqueda de lugares. Es un servicio único para toda la app: los recorridos que ya se pidieron quedan en caché.
/// </summary>
public class ServicioTrazos
{
    private readonly IHttpClientFactory _fabrica;
    private readonly ILogger<ServicioTrazos> _log;
    private readonly ConcurrentDictionary<string, List<double[]>> _cacheOsrm = new();

    public ServicioTrazos(IHttpClientFactory fabrica, ILogger<ServicioTrazos> log)
    {
        _fabrica = fabrica;
        _log = log;
    }

    private HttpClient Http => _fabrica.CreateClient("externo");

    // Texto "lng,lat;lng,lat;..." que usa OSRM
    public static string ClaveOsrm(IEnumerable<double[]> puntosLatLng)
    {
        return string.Join(";", puntosLatLng.Select(w =>
            w[1].ToString("R", CultureInfo.InvariantCulture) + "," + w[0].ToString("R", CultureInfo.InvariantCulture)));
    }

    /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM). null si no hay internet. aPie = por banquetas y andadores.</summary>
    public async Task<List<double[]>> TrazoOsrmAsync(string coordenadas, bool aPie = false)
    {
        string clave = (aPie ? "pie|" : "") + coordenadas;
        if (_cacheOsrm.TryGetValue(clave, out var guardado)) return guardado;
        try
        {
            string url = aPie
                ? "https://routing.openstreetmap.de/routed-foot/route/v1/foot/"
                : "https://router.project-osrm.org/route/v1/driving/";
            string json = await Http.GetStringAsync(url + coordenadas + "?overview=full&geometries=geojson");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var raiz = doc.RootElement;
            if (raiz.GetProperty("code").GetString() != "Ok") return null;
            var lista = new List<double[]>();
            foreach (var c in raiz.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates").EnumerateArray())
                lista.Add(new[] { c[1].GetDouble(), c[0].GetDouble() });   // Leaflet usa [lat, lng]
            if (lista.Count < 2) return null;
            _cacheOsrm[clave] = lista;
            return lista;
        }
        catch (Exception ex)
        {
            _log.LogDebug("OSRM: {Mensaje}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Ajusta una línea que une puntos en línea recta (por ejemplo las paradas de una ruta) para que siga las calles.
    /// Se toman puntos cada ~350 m y se piden a OSRM en grupos de 25. Devuelve null si no hay internet: se queda la línea original.
    /// </summary>
    public async Task<List<double[]>> AjustarACallesAsync(List<double[]> puntos, bool aPie = false)
    {
        if (puntos == null || puntos.Count < 2) return null;
        var muestra = Muestrear(puntos, 350, 200);

        const int tamano = 25;
        var grupos = new List<List<double[]>>();
        for (int inicio = 0; inicio < muestra.Count - 1; inicio += tamano - 1)
        {
            var grupo = muestra.Skip(inicio).Take(tamano).ToList();
            if (grupo.Count >= 2) grupos.Add(grupo);
        }

        // Hasta 3 peticiones a la vez; si alguna falla, ese trozo se queda con sus puntos originales
        var limite = new SemaphoreSlim(3);
        var partes = await Task.WhenAll(grupos.Select(async g =>
        {
            await limite.WaitAsync();
            try { return await TrazoOsrmAsync(ClaveOsrm(g), aPie) ?? g; }
            finally { limite.Release(); }
        }));

        var resultado = new List<double[]>();
        foreach (var parte in partes)
        {
            var copia = parte.ToList();
            if (resultado.Count > 0) copia.RemoveAt(0);
            resultado.AddRange(copia);
        }
        return resultado.Count >= 2 ? resultado : null;
    }

    /// <summary>
    /// Corrige un recorrido guardado: casi todo ya sigue las calles, pero algunos tramos largos cortan esquinas
    /// (la línea salta por encima de las manzanas). Cada tramo recto largo se compara con la ruta real por calles (OSRM)
    /// y solo se reemplaza si la calle da una vuelta notable. Devuelve (nuevo recorrido o null si nada cambió, completo).
    /// completo = false si algún tramo no se pudo consultar (sin internet): conviene reintentar luego.
    /// </summary>
    public async Task<(List<double[]> Linea, bool Completo)> ReparaTramosRectosAsync(List<double[]> pts, double minimoMetros = 400)
    {
        if (pts == null || pts.Count < 2) return (null, true);
        double k = Math.Cos(pts[0][0] * Math.PI / 180);
        double Largo(List<double[]> l) => Enumerable.Range(1, l.Count - 1).Sum(i => Dist(l[i - 1], l[i]));
        double Dist(double[] a, double[] b)
        {
            double dx = (a[1] - b[1]) * 111320 * k, dy = (a[0] - b[0]) * 110574;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        var largos = Enumerable.Range(1, pts.Count - 1).Where(i => Dist(pts[i - 1], pts[i]) >= minimoMetros).ToList();
        if (largos.Count == 0) return (null, true);

        var limite = new SemaphoreSlim(3);
        var resultados = await Task.WhenAll(largos.Select(async i =>
        {
            await limite.WaitAsync();
            try
            {
                var calle = await TrazoOsrmAsync(ClaveOsrm(new[] { pts[i - 1], pts[i] }));
                if (calle == null) return (i, (List<double[]>)null, false);
                bool daVuelta = Largo(calle) > Dist(pts[i - 1], pts[i]) * 1.15;
                return (i, daVuelta ? calle : null, true);
            }
            finally { limite.Release(); }
        }));

        bool completo = resultados.All(r => r.Item3);
        var cambios = resultados.Where(r => r.Item2 != null).ToDictionary(r => r.i, r => r.Item2);
        if (cambios.Count == 0) return (null, completo);

        var nuevo = new List<double[]> { pts[0] };
        for (int i = 1; i < pts.Count; i++)
        {
            if (cambios.TryGetValue(i, out var calle)) nuevo.AddRange(calle.Skip(1));
            nuevo.Add(pts[i]);
        }
        return (nuevo, completo);
    }

    // Se queda con un punto cada "separacion" metros (siempre el primero y el último), con un máximo de puntos
    private static List<double[]> Muestrear(List<double[]> pts, double separacion, int maximo)
    {
        var res = new List<double[]> { pts[0] };
        double k = Math.Cos(pts[0][0] * Math.PI / 180);
        double acumulado = 0;
        for (int i = 1; i < pts.Count - 1; i++)
        {
            double dx = (pts[i][1] - pts[i - 1][1]) * 111320 * k, dy = (pts[i][0] - pts[i - 1][0]) * 110574;
            acumulado += Math.Sqrt(dx * dx + dy * dy);
            if (acumulado >= separacion) { res.Add(pts[i]); acumulado = 0; }
        }
        res.Add(pts[pts.Count - 1]);
        if (res.Count <= maximo) return res;

        var corto = new List<double[]>();
        double paso = (res.Count - 1) / (double)(maximo - 1);
        for (int i = 0; i < maximo; i++) corto.Add(res[(int)Math.Round(i * paso)]);
        return corto;
    }

    /// <summary>Nombre de la calle en esas coordenadas (OpenStreetMap / Nominatim), o null si no hay internet o no se encontró.</summary>
    public async Task<string> CalleDeAsync(double lat, double lng)
    {
        try
        {
            string url = "https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=18&accept-language=es&lat=" +
                         lat.ToString("R", CultureInfo.InvariantCulture) + "&lon=" + lng.ToString("R", CultureInfo.InvariantCulture);
            string json = await Http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("address", out var dir)) return null;
            foreach (var clave in new[] { "road", "pedestrian", "residential", "footway", "path", "neighbourhood", "suburb" })
            {
                if (dir.TryGetProperty(clave, out var valor) && !string.IsNullOrWhiteSpace(valor.GetString()))
                    return valor.GetString();
            }
            return null;
        }
        catch { return null; }
    }

    // ── Buscador de lugares (Photon + Nominatim) ──────────────────────────
    public Task<List<Sugerencia>> BuscarLugaresAsync(string texto) => BuscadorLugares.BuscarAsync(Http, texto);

    // ── Paradas y puntos de referencia (Overpass / OpenStreetMap) ─────────
    public class PuntoMapa
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
    }

    public async Task<List<PuntoMapa>> ParadasEntreAsync(double latO, double lngO, double latD, double lngD)
    {
        try
        {
            double s = Math.Min(latO, latD) - 0.005;
            double o = Math.Min(lngO, lngD) - 0.005;
            double n = Math.Max(latO, latD) + 0.005;
            double e = Math.Max(lngO, lngD) + 0.005;

            string query = string.Format(CultureInfo.InvariantCulture,
                "[out:json][timeout:25];(" +
                "node[highway=bus_stop]({0},{1},{2},{3});" +
                "node[amenity=bus_station]({0},{1},{2},{3});" +
                "node[amenity=pharmacy]({0},{1},{2},{3});" +
                "node[amenity=hospital]({0},{1},{2},{3});" +
                "node[amenity=school]({0},{1},{2},{3});" +
                "node[shop=supermarket]({0},{1},{2},{3});" +
                ");out body;",
                s, o, n, e);

            var contenido = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
            var respuesta = await Http.PostAsync("https://overpass-api.de/api/interpreter", contenido);
            string json = await respuesta.Content.ReadAsStringAsync();

            return ParsearOverpass(json).Take(80).ToList();   // pocos puntos: mapa limpio
        }
        catch (Exception ex)
        {
            _log.LogDebug("Overpass: {Mensaje}", ex.Message);
            return new List<PuntoMapa>();
        }
    }

    // Parser JSON de Overpass sin dependencias externas
    private static List<PuntoMapa> ParsearOverpass(string json)
    {
        var lista = new List<PuntoMapa>();
        var bloques = json.Split(new[] { "{\"type\":\"node\"" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var bloque in bloques)
        {
            var mLat = System.Text.RegularExpressions.Regex.Match(bloque, "\"lat\":([0-9.\\-]+)");
            var mLng = System.Text.RegularExpressions.Regex.Match(bloque, "\"lon\":([0-9.\\-]+)");
            if (!mLat.Success || !mLng.Success) continue;

            double lat = double.Parse(mLat.Groups[1].Value, CultureInfo.InvariantCulture);
            double lng = double.Parse(mLng.Groups[1].Value, CultureInfo.InvariantCulture);
            var mName = System.Text.RegularExpressions.Regex.Match(bloque, "\"name\":\"([^\"]+)\"");
            string nombre = mName.Success ? mName.Groups[1].Value : "";

            string tipo;
            if (bloque.Contains("\"highway\":\"bus_stop\"") || bloque.Contains("\"amenity\":\"bus_station\"")) tipo = "parada";
            else if (bloque.Contains("\"amenity\":\"pharmacy\"")) tipo = "farmacia";
            else if (bloque.Contains("\"amenity\":\"hospital\"")) tipo = "hospital";
            else if (bloque.Contains("\"amenity\":\"school\"")) tipo = "escuela";
            else if (bloque.Contains("\"shop\":\"supermarket\"")) tipo = "supermercado";
            else continue;

            lista.Add(new PuntoMapa { Lat = lat, Lng = lng, Nombre = nombre, Tipo = tipo });
        }
        return lista;
    }
}
