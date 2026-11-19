using Microsoft.JSInterop;
using RutaApp.Interfaz.Servicios;

namespace RutaApp.Web.Servicios;

// Copia al portapapeles del navegador (código para compartir una ruta o un viaje).
public class PortapapelesNavegador : IPortapapeles
{
    private readonly IJSRuntime _js;

    public PortapapelesNavegador(IJSRuntime js) => _js = js;

    public async Task<bool> CopiarAsync(string texto)
    {
        try { return await _js.InvokeAsync<bool>("rutaWeb.copiar", texto); }
        catch (Exception) { return false; }
    }
}
