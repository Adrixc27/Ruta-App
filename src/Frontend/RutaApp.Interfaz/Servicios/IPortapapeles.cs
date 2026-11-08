namespace RutaApp.Interfaz.Servicios;

// Copia texto al portapapeles del equipo. Lo implementa cada host (hoy, la ventana WPF).
public interface IPortapapeles
{
    // true si el texto quedó copiado.
    Task<bool> CopiarAsync(string texto);
}
