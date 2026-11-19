using Microsoft.JSInterop;
using RutaApp.Nucleo.Servicios;

namespace RutaApp.Web.Servicios;

// Ubicación con el permiso de ubicación del navegador (navigator.geolocation). Si no la da, se usa la aproximada por IP.
// Las rutas son de Chihuahua: una ubicación lejos de ahí no sirve para planificar, así que se descarta y la persona escribe su origen.
public class UbicacionNavegador : IServicioUbicacion
{
    private const double CentroLat = 28.6353, CentroLng = -106.0889;
    private const double RadioKm = 80;

    private readonly IJSRuntime _js;
    private readonly ServicioUbicacionIp _porIp;

    public UbicacionNavegador(IJSRuntime js, ServicioUbicacionIp porIp)
    {
        _js = js;
        _porIp = porIp;
    }

    public async Task<double[]?> ObtenerAsync()
    {
        double[]? pos = null;
        try { pos = await _js.InvokeAsync<double[]?>("rutaWeb.ubicacion"); }
        catch (Exception) { }

        pos ??= await _porIp.ObtenerAsync();
        return pos != null && EnZonaDeServicio(pos[0], pos[1]) ? pos : null;
    }

    private static bool EnZonaDeServicio(double lat, double lng)
    {
        double dx = (lng - CentroLng) * 111.32 * Math.Cos(CentroLat * Math.PI / 180), dy = (lat - CentroLat) * 110.57;
        return Math.Sqrt(dx * dx + dy * dy) <= RadioKm;
    }
}
