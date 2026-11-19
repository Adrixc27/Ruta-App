using System.Net.Http.Json;
using RutaApp.Datos;

namespace RutaApp.Web.Servicios;

// Las rutas y paradas que la app de escritorio lee de MySQL, aquí salen de datos/rutas.json (copia de la misma base de datos).
public class RutaServiceWeb : RutaService
{
    private readonly HttpClient _http;
    private List<RutaOficial>? _rutas;

    public RutaServiceWeb(HttpClient http) => _http = http;

    public override async Task<List<RutaOficial>> ObtenerRutasAsync()
    {
        if (_rutas != null) return _rutas;
        var lista = await _http.GetFromJsonAsync<List<RutaOficial>>("datos/rutas.json") ?? new();
        _rutas = lista;
        return lista;
    }
}
