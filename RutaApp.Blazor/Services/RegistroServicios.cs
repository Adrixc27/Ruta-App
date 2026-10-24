using Microsoft.Extensions.DependencyInjection;

namespace RutaApp.Blazor.Services;

public static class RegistroServicios
{
    /// <summary>
    /// Registra los servicios de la app. urlApi es la dirección de la API (registro, login, camiones y rutas).
    /// La app anfitriona (RutaApp.Maui) debe registrar además <see cref="IServicioUbicacion"/> si quiere usar el GPS.
    /// </summary>
    public static IServiceCollection AgregarServiciosRuta(this IServiceCollection servicios, string urlApi)
    {
        servicios.AddHttpClient<ServicioApi>(cliente =>
        {
            cliente.BaseAddress = new Uri(urlApi);
            cliente.Timeout = TimeSpan.FromSeconds(20);
        });

        // Servicios de OpenStreetMap (OSRM, Nominatim, Photon, Overpass): piden un User-Agent que identifique la app
        servicios.AddHttpClient("externo", cliente =>
        {
            cliente.Timeout = TimeSpan.FromSeconds(8);
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
        });

        servicios.AddSingleton<ServicioTrazos>();                  // caché de recorridos
        servicios.AddScoped<RutaRepositorio>();
        servicios.AddScoped<EstadoApp>();                          // estado de la sesión (usuario, búsqueda, rutas guardadas...)
        servicios.AddSingleton<ServicioUbicacionIp>();
        servicios.AddSingleton<IServicioUbicacion>(p => p.GetRequiredService<ServicioUbicacionIp>());   // por defecto, solo IP
        return servicios;
    }
}
