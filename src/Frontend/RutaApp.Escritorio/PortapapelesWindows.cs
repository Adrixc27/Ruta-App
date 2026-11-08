using System.Runtime.InteropServices;
using System.Windows;
using RutaApp.Interfaz.Servicios;

namespace RutaApp.Escritorio;

public class PortapapelesWindows : IPortapapeles
{
    public Task<bool> CopiarAsync(string texto) =>
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Clipboard.SetText(texto);
                return true;
            }
            catch (COMException)
            {
                // Otro programa tiene el portapapeles abierto
                return false;
            }
        }).Task;
}
