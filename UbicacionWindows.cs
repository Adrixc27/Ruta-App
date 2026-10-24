using RutaApp.Blazor.Services;
using Windows.Devices.Geolocation;

namespace Ruta_App;

/// Ubicación del equipo con el servicio de ubicación de Windows. Si está desactivado o no da permiso,
/// se usa la ubicación aproximada por IP
public class UbicacionWindows : IServicioUbicacion
{
    private readonly ServicioUbicacionIp _porIp;

    public UbicacionWindows(ServicioUbicacionIp porIp)
    {
        _porIp = porIp;
    }

    public async Task<double[]?> ObtenerAsync()
    {
        try
        {
            var acceso = await Geolocator.RequestAccessAsync();
            if (acceso == GeolocationAccessStatus.Allowed)
            {
                var geolocalizador = new Geolocator { DesiredAccuracy = PositionAccuracy.Default };
                var posicion = await geolocalizador.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(8));
                var punto = posicion.Coordinate.Point.Position;
                return new[] { punto.Latitude, punto.Longitude };
            }
        }
        catch (Exception)
        {
            // Sin servicio de ubicación o sin permiso: se usa el respaldo
        }
        return await _porIp.ObtenerAsync();
    }
}
