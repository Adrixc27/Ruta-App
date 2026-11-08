namespace RutaApp.Interfaz.Servicios;

/// <summary>Copia texto al portapapeles del equipo. Lo implementa cada host (hoy, la ventana WPF).</summary>
public interface IPortapapeles
{
    /// <summary>true si el texto quedó copiado.</summary>
    Task<bool> CopiarAsync(string texto);
}
