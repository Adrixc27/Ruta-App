using Microsoft.AspNetCore.Components.Web;

namespace RutaApp.Interfaz.Servicios;

public static class Teclado
{
    /// <summary>Enter y Espacio activan lo que se puede tocar con el mouse pero no es un botón (filas, tarjetas, preguntas).</summary>
    public static bool EsActivacion(KeyboardEventArgs e) => e.Key is "Enter" or " ";
}
