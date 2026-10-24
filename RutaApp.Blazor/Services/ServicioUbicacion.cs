using System.Globalization;

namespace RutaApp.Blazor.Services;

/// <summary>Da la ubicación del equipo como [lat, lng], o null si no se pudo obtener.</summary>
public interface IServicioUbicacion
{
    Task<double[]> ObtenerAsync();
}

/// <summary>
/// Ubicación aproximada por IP (ipwho.is). Como la app corre en el equipo del usuario, la IP es la suya.
/// Sirve de respaldo cuando el sistema no da la ubicación exacta.
/// </summary>
public class ServicioUbicacionIp : IServicioUbicacion
{
    private readonly IHttpClientFactory _fabrica;

    public ServicioUbicacionIp(IHttpClientFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<double[]> ObtenerAsync()
    {
        try
        {
            string json = await _fabrica.CreateClient("externo").GetStringAsync("https://ipwho.is/");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var r = doc.RootElement;
            if (!r.GetProperty("success").GetBoolean()) return null;
            return new[] { r.GetProperty("latitude").GetDouble(), r.GetProperty("longitude").GetDouble() };
        }
        catch (Exception)
        {
            return null;
        }
    }
}
