using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Ruta_App
{
    // Lo que responde la API cuando el login o el registro son correctos
    public class userLoginResponse
    {
        public int id { get; set; }
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public string phoneNumber { get; set; }
        public string photoUser { get; set; }
    }

    // Guarda al usuario mientras la app está abierta
    public static class sesion
    {
        public static userLoginResponse userLogin { get; set; }
    }

    // Resultado de intentar registrarse: trae el usuario, o el mensaje de error
    public class ResultadoRegistro            // <-- ya no es "static"
    {
        public userLoginResponse user { get; set; }
        public string Error { get; set; }
    }

    public class ApiService
    {
        private static readonly HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:7284/")
        };

        // Devuelve el usuario si el login es correcto, o null si las credenciales fallan
        public async Task<userLoginResponse> LoginAsync(string email, string password)
        {
            var loginrequest = await client.PostAsJsonAsync("login", new { email, password });

            if (loginrequest.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            loginrequest.EnsureSuccessStatusCode();
            return await loginrequest.Content.ReadFromJsonAsync<userLoginResponse>();
        }

        public async Task<ResultadoRegistro> RegistrarAsync(string nombre, string correo, string password)
        {
            var respuesta = await client.PostAsJsonAsync("registro",
                new { name = nombre, email = correo, password });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoRegistro
                {
                    user = await respuesta.Content.ReadFromJsonAsync<userLoginResponse>()
                };
            }

            // 409 (correo repetido) o 400 (datos inválidos): la API manda el motivo como texto
            string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim('"');
            return new ResultadoRegistro { Error = mensaje };
        }
    }
}