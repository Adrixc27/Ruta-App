using System.Net;
using System.Net.Http.Json;

namespace RutaApp.Blazor.Services;

// Lo que responde la API cuando el login o el registro son correctos
public class UsuarioApi
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; }
    public string PhotoUser { get; set; }
}

// Resultado de intentar registrarse: trae el usuario, o el mensaje de error
public class ResultadoRegistro
{
    public UsuarioApi Usuario { get; set; }
    public string Error { get; set; }
}

// Camiones y rutas que manda la API (tablas bus y routes)
public class BusApi
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class RutaApi
{
    public int Id { get; set; }
    public int IdBus { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public double TravelMinutes { get; set; }
    public string Description { get; set; }
}

/// <summary>Cliente de la API en la web (MiApi). La dirección se configura en appsettings.json → Api:BaseUrl.</summary>
public class ServicioApi
{
    private readonly HttpClient _http;

    public ServicioApi(HttpClient http)
    {
        _http = http;
    }

    // Devuelve el usuario si el login es correcto, o null si las credenciales fallan
    public async Task<UsuarioApi> LoginAsync(string email, string password)
    {
        var respuesta = await _http.PostAsJsonAsync("login", new { email, password });

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<UsuarioApi>();
    }

    public Task<List<BusApi>> ObtenerBusesAsync()
    {
        return _http.GetFromJsonAsync<List<BusApi>>("buses");
    }

    public Task<List<RutaApi>> ObtenerRutasAsync()
    {
        return _http.GetFromJsonAsync<List<RutaApi>>("routes");
    }

    public async Task<ResultadoRegistro> RegistrarAsync(string nombre, string correo, string password)
    {
        var respuesta = await _http.PostAsJsonAsync("registro",
            new { name = nombre, email = correo, password });

        if (respuesta.IsSuccessStatusCode)
        {
            return new ResultadoRegistro
            {
                Usuario = await respuesta.Content.ReadFromJsonAsync<UsuarioApi>()
            };
        }

        // 409 (correo repetido) o 400 (datos inválidos): la API manda el motivo como texto
        string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim('"');
        return new ResultadoRegistro { Error = mensaje };
    }
}
