using Microsoft.Extensions.DependencyInjection;

namespace RutaApp.Nucleo;

public static class RegistroNucleo
{
    /// <summary>Registra la lógica de la app (rutas, recorridos, ubicación, ajustes) y, con ella, la capa de datos.</summary>
    public static IServiceCollection AgregarNucleo(this IServiceCollection servicios)
    {
        servicios.AgregarDatos();

        // OSRM, Nominatim, Photon y Overpass piden un User-Agent que identifique la app
        servicios.AddHttpClient("externo", cliente =>
        {
            cliente.Timeout = TimeSpan.FromSeconds(8);
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
        });

        servicios.AddSingleton<ServicioTrazos>();
        servicios.AddScoped<RutaRepositorio>();
        servicios.AddSingleton<ServicioUbicacionIp>();
        servicios.AddSingleton<IServicioUbicacion>(p => p.GetRequiredService<ServicioUbicacionIp>());
        servicios.AddSingleton<AjustesService>();
        servicios.AddSingleton<DatosUsuarioService>();
        return servicios;
    }
}
