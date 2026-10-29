using Microsoft.Extensions.DependencyInjection;

namespace RutaApp.Blazor.Services;

public static class RegistroServicios
{
    /// <summary>
    /// Registra los servicios de la app. Las rutas y paradas se leen de MySQL (RutaService).
    /// El parámetro urlApi se conserva por compatibilidad con la app anfitriona.
    /// </summary>
    public static IServiceCollection AgregarServiciosRuta(this IServiceCollection servicios, string urlApi)
    {
        // API desactivada: se usa la conexión directa a MySQL.
        // Si alguna clase todavía inyecta ServicioApi, descomenta este bloque hasta quitar ese uso.
        // servicios.AddHttpClient<ServicioApi>(cliente =>
        // {
        //     cliente.BaseAddress = new Uri(urlApi);
        //     cliente.Timeout = TimeSpan.FromSeconds(20);
        // });

        // Servicios de OpenStreetMap (OSRM, Nominatim, Photon, Overpass): piden un User-Agent que identifique la app
        servicios.AddHttpClient("externo", cliente =>
        {
            cliente.Timeout = TimeSpan.FromSeconds(8);
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
        });

        servicios.AddSingleton<ServicioTrazos>();                  // caché de recorridos
        servicios.AddSingleton<RutaService>();                     // rutas y paradas desde MySQL
        servicios.AddScoped<RutaRepositorio>();
        servicios.AddScoped<EstadoApp>();                          // estado de la sesión (usuario, búsqueda, rutas guardadas...)
        servicios.AddSingleton<ServicioUbicacionIp>();
        servicios.AddSingleton<IServicioUbicacion>(p => p.GetRequiredService<ServicioUbicacionIp>());
        servicios.AddSingleton<UsuarioService>();
        return servicios;
    }
}