using Microsoft.AspNetCore.Components;

namespace RutaApp.Interfaz.Servicios;

public static class UrlApp
{
    // Pantalla actual relativa a la base de la app y sin parámetros: "planificar", "configuracion/ajuste"… ("" en la landing).
    // No usa AbsolutePath porque, publicada en una subcarpeta (GitHub Pages: /Ruta-App/), la ruta absoluta lleva esa carpeta delante.
    public static string RutaActual(this NavigationManager nav)
    {
        string ruta = nav.ToBaseRelativePath(nav.Uri);
        int corte = ruta.IndexOfAny(new[] { '?', '#' });
        return (corte >= 0 ? ruta[..corte] : ruta).Trim('/');
    }
}
