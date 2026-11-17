using Microsoft.Extensions.DependencyInjection;
using RutaApp.Interfaz.Idiomas;
using RutaApp.Nucleo;

namespace RutaApp.Interfaz.Servicios;

public static class RegistroServicios
{
    // Registra todo lo que necesita la interfaz: el estado de la sesión y los servicios del núcleo.
    public static IServiceCollection AgregarServiciosRuta(this IServiceCollection servicios)
    {
        servicios.AgregarNucleo();
        servicios.AddSingleton<Traductor>();
        servicios.AddScoped<EstadoApp>();
        return servicios;
    }
}
