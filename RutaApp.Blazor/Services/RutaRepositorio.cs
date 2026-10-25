namespace RutaApp.Blazor.Services;

/// <summary>
/// Trae las rutas de la API (tabla routes) y los nombres de los camiones (tabla bus) y arma las Ruta de la app.
/// </summary>
public class RutaRepositorio
{
    private readonly ServicioApi _api;
    private readonly ILogger<RutaRepositorio> _log;

    public RutaRepositorio(ServicioApi api, ILogger<RutaRepositorio> log)
    {
        _api = api;
        _log = log;
    }

    /// <summary>Devuelve las rutas reales, o null si la API no responde o no hay datos.</summary>
    public async Task<List<Ruta>> CargarAsync()
    {
        try
        {
            var buses = await _api.ObtenerBusesAsync() ?? new List<BusApi>();
            var rutas = await _api.ObtenerRutasAsync() ?? new List<RutaApi>();

            var resultado = new List<Ruta>();
            foreach (var fila in rutas)
            {
                var bus = buses.FirstOrDefault(b => b.Id == fila.IdBus);
                resultado.Add(Armar(fila, bus));
            }
            return resultado.Count > 0 ? resultado : null;
        }
        catch (Exception ex)
        {
            _log.LogWarning("RutaRepositorio: {Mensaje}", ex.Message);
            return null;
        }
    }

    private static Ruta Armar(RutaApi fila, BusApi bus)
    {
        string nombre = string.IsNullOrWhiteSpace(fila.Name) ? (bus != null ? bus.Name : "Ruta " + fila.Id) : fila.Name.Trim();

        var r = new Ruta
        {
            Numero = fila.Id,
            Nombre = nombre,
            Lineas = bus != null ? bus.Name : nombre,
            Tarifa = fila.Price,
            Minutos = (int)Math.Round(fila.TravelMinutes),
            Descripcion = fila.Description ?? "",
            OrigenNombre = nombre,
            DestinoNombre = "Centro"
        };

        // Minutos, tarifa, nombre y camión vienen de la base de datos.
        r.DatosReales = true;

        // Si la ruta tiene plano oficial (Rutas.pdf, BOWI.pdf o DALE UP) se usa su recorrido real.
        // Algunas tienen variantes (Komatsu Inverso / Directo, Panamericana San Felipe / Mirador): arranca en la primera.
        var variantes = TrazosPlanos.BuscarVariantes(nombre) ?? TrazosPlanos.BuscarVariantes(bus?.Name);
        if (variantes != null)
        {
            r.Variantes = variantes;
            r.UsarVariante(0);
            return r;
        }

        // Las demás rutas todavía no tienen paradas ni coordenadas en la base de datos:
        // el recorrido en el mapa es un trazo DE PRUEBA (no representa el camino real de la ruta).
        r.Waypoints = TrazoDePrueba(fila.Id);
        r.LatO = r.Waypoints[0][0]; r.LngO = r.Waypoints[0][1];
        var ultimo = r.Waypoints[r.Waypoints.Count - 1];
        r.LatD = ultimo[0]; r.LngD = ultimo[1];
        return r;
    }

    // Recorridos de prueba que cruzan la ciudad pasando por el Centro; se asignan por id de ruta para que no se encimen.
    // Cuando la BD tenga paradas con coordenadas, este método se reemplaza por los puntos reales.
    private static readonly double[][][] TrazosDePrueba =
    {
        new[] { new[] { 28.7000, -106.1100 }, new[] { Geo.CentroLat, Geo.CentroLng }, new[] { 28.6200, -106.0250 } },
        new[] { new[] { 28.5900, -106.1000 }, new[] { Geo.CentroLat, Geo.CentroLng }, new[] { 28.6900, -106.0450 } },
        new[] { new[] { 28.6100, -106.1450 }, new[] { Geo.CentroLat, Geo.CentroLng }, new[] { 28.6300, -106.0400 } },
        new[] { new[] { 28.6800, -106.0950 }, new[] { Geo.CentroLat, Geo.CentroLng }, new[] { 28.6000, -106.0850 } },
        new[] { new[] { 28.6350, -106.1500 }, new[] { Geo.CentroLat, Geo.CentroLng }, new[] { 28.6500, -106.0250 } }
    };

    private static List<double[]> TrazoDePrueba(int idRuta)
    {
        var plantilla = TrazosDePrueba[Math.Abs(idRuta) % TrazosDePrueba.Length];
        return plantilla.Select(p => new[] { p[0], p[1] }).ToList();
    }
}
