using Microsoft.AspNetCore.Components.Web;

namespace RutaApp.Interfaz.Servicios;

public static class Teclado
{
    // Enter y Espacio activan lo que se puede tocar con el mouse pero no es un botón (filas, tarjetas, preguntas).
    public static bool EsActivacion(KeyboardEventArgs e) => e.Key is "Enter" or " ";
}
