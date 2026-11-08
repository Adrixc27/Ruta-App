using Microsoft.Extensions.DependencyInjection;

namespace RutaApp.Blazor.Services;

public static class RegistroServicios
{
    /// <summary>
    /// Registra los servicios de la app. Todo se lee y se guarda directo en MySQL local (ver Connection.cs): no hay API.
    /// </summary>
    public static IServiceCollection AgregarServiciosRuta(this IServiceCollection servicios)
    {
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
        servicios.AddSingleton<FavoritoService>();
        servicios.AddSingleton<PreferenciaService>();
        servicios.AddSingleton<ComunidadService>();
        servicios.AddSingleton<AjustesService>();                  // unidades, accesibilidad y sesión recordada
        servicios.AddSingleton<DatosUsuarioService>();             // historial, viajes y lugares guardados de cada usuario
        return servicios;
    }
}