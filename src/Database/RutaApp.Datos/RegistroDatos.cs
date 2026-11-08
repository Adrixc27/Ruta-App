using Microsoft.Extensions.DependencyInjection;

namespace RutaApp.Datos;

public static class RegistroDatos
{
    /// <summary>Registra los servicios que leen y escriben en MySQL.</summary>
    public static IServiceCollection AgregarDatos(this IServiceCollection servicios)
    {
        servicios.AddSingleton<RutaService>();
        servicios.AddSingleton<UsuarioService>();
        servicios.AddSingleton<FavoritoService>();
        servicios.AddSingleton<PreferenciaService>();
        servicios.AddSingleton<ComunidadService>();
        servicios.AddSingleton<OpinionService>();
        return servicios;
    }
}
