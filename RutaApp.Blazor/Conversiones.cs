using RutaApp.Blazor.Services;

namespace RutaApp.Blazor;

public static class Conversiones
{
    // Convierte el Usuario de MySQL al tipo que todavía espera EstadoApp.
    // Cuando EstadoApp use Usuario directamente, este archivo y UsuarioApi se pueden borrar.
    public static UsuarioApi ParaEstado(this Usuario u)
    {
        return new UsuarioApi
        {
            Id = u.Id,
            Name = u.Nombre,
            Email = u.Correo,
            PhotoUser = u.Foto
        };
    }
}