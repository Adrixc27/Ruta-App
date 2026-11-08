namespace RutaApp.Nucleo.Servicios;

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

    /// <summary>Olvida lo leído de la base de datos (por ejemplo al cambiar la conexión).</summary>
    public void LimpiarCache() => _rutaService.LimpiarCache();

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
                .Where(r => r != null)
                .ToList();

            return resultado.Count > 0 ? resultado : null;
        }
        catch (Exception ex)
        {
            _log.LogWarning("RutaRepositorio: {Mensaje}", ex.Message);
            return null;
        }
    }

    // Solo se muestran rutas con recorrido real (paradas o trazo en la base de datos); no se inventan recorridos de prueba
    private static Ruta Armar(List<RutaOficial> todas)
    {
        var variantes = todas.Where(v => v.Trazo.Count >= 2).ToList();
        if (variantes.Count == 0) return null;

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
            DatosReales = true,
            Variantes = variantes
        };
        r.UsarVariante(0);
        return r;
    }
}
