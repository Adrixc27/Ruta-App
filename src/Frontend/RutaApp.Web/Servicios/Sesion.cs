namespace RutaApp.Web.Servicios;

/// <summary>Usuario que inició sesión mientras la app está abierta.</summary>
public static class Sesion
{
    public static Usuario Actual { get; set; }
}
