namespace RutaApp.Blazor.Services;

/// <summary>
/// Arma las Ruta de la app a partir de la base de datos (routes, bus, stop y route_stop) vía RutaService.
/// </summary>
public class RutaRepositorio
{
    private readonly RutaService _rutaService;
    private readonly ILogger<RutaRepositorio> _log;

    public RutaRepositorio(RutaService rutaService, ILogger<RutaRepositorio> log)
    {
        _rutaService = rutaService;
        _log = log;
    }

    /// <summary>Devuelve las rutas reales, o null si la base de datos no responde o no hay datos.</summary>
    public async Task<List<Ruta>> CargarAsync()
    {
        try
        {
            var todas = await _rutaService.ObtenerRutasAsync();

            // Las variantes de un mismo grupo (Komatsu Inverso / Directo) son una sola ruta en la lista
            var resultado = todas
                .GroupBy(o => o.Grupo)
                .Select(g => Armar(g.ToList()))
                .ToList();

            return resultado.Count > 0 ? resultado : null;
        }
        catch (Exception ex)
        {
            _log.LogWarning("RutaRepositorio: {Mensaje}", ex.Message);
            return null;
        }
    }

    private static Ruta Armar(List<RutaOficial> variantes)
    {
        var o = variantes[0];
        string nombre = variantes.Count > 1 ? o.Grupo : o.Nombre;

        var r = new Ruta
        {
            Numero = o.Id,
            Nombre = nombre,
            Lineas = string.IsNullOrWhiteSpace(o.BusNombre) ? nombre : o.BusNombre,
            Tarifa = o.Precio ?? 0m,
            Minutos = (int)Math.Round((o.TiempoRecorrido ?? TimeSpan.Zero).TotalMinutes),
            Descripcion = o.Descripcion ?? "",
            OrigenNombre = nombre,
            DestinoNombre = "Centro",
            DatosReales = true
        };

        // Con paradas en la base de datos se usa su recorrido; si hay variantes, arranca en la primera.
        if (o.Trazo.Count >= 2)
        {
            r.Variantes = variantes;
            r.UsarVariante(0);
            return r;
        }

        // Sin paradas en la base de datos: el recorrido en el mapa es un trazo DE PRUEBA.
        r.Waypoints = TrazoDePrueba(o.Id);
        r.LatO = r.Waypoints[0][0]; r.LngO = r.Waypoints[0][1];
        var ultimo = r.Waypoints[r.Waypoints.Count - 1];
        r.LatD = ultimo[0]; r.LngD = ultimo[1];
        return r;
    }

    // Recorridos de prueba que cruzan la ciudad pasando por el Centro; se asignan por id de ruta para que no se encimen.
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