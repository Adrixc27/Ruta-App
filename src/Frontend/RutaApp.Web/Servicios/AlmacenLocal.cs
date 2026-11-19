using System.Text.Json;
using Microsoft.JSInterop;

namespace RutaApp.Web.Servicios;

// Lo que la persona guarda en la muestra (historial, lugares, rutas guardadas) vive en el localStorage de su navegador:
// no sale de su equipo ni se comparte con nadie. Si el navegador lo bloquea, la muestra sigue funcionando sin guardar.
public class AlmacenLocal
{
    private readonly IJSRuntime _js;

    public AlmacenLocal(IJSRuntime js) => _js = js;

    public async Task<T?> LeerAsync<T>(string clave)
    {
        try
        {
            string? texto = await _js.InvokeAsync<string?>("localStorage.getItem", clave);
            return string.IsNullOrEmpty(texto) ? default : JsonSerializer.Deserialize<T>(texto);
        }
        catch (Exception)
        {
            return default;   // sin almacenamiento o datos dañados: se empieza de cero
        }
    }

    // true si se pudo guardar (el navegador limita el espacio y puede negarse).
    public async Task<bool> EscribirAsync<T>(string clave, T valor)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", clave, JsonSerializer.Serialize(valor));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task BorrarAsync(string clave)
    {
        try { await _js.InvokeVoidAsync("localStorage.removeItem", clave); }
        catch (Exception) { }
    }
}
