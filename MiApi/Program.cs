using MySqlConnector;
using Dapper;

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("Default");

var app = builder.Build();
//Testear que si conecta a la base de datos 

app.MapGet("/", () => "API funcionando");

app.MapGet("/probar-bd", async () =>
{
    try
    {
        await using var conexion = new MySqlConnection(cs);
        await conexion.OpenAsync();
        return Results.Ok("Conexión a MySQL exitosa");
    }
    catch (Exception ex)
    {
        return Results.Problem("No se pudo conectar: " + ex.Message);
    }
});
//Endpoints
// Registrar usuario
app.MapPost("/registro", async (RegistroRequest request) =>
{
    // Validación de datos
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        request.Password.Length < 6)
    {
        return Results.BadRequest("Datos inválidos, mínimo 6 caracteres en la contraseña");
    }

    await using var conexion = new MySqlConnection(cs);

    // Revisar si el correo ya existe (tabla `user`, con acentos graves)
    var usuarioExiste = await conexion.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM `user` WHERE email = @Email",
        new { request.Email });

    if (usuarioExiste > 0)
    {
        return Results.Conflict("El correo ya está registrado");
    }

    // Hash de la contraseña
    var hashpassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

    // Guardar usuario y recuperar el id que le asignó MySQL
    var iduser = await conexion.ExecuteScalarAsync<int>(
        @"INSERT INTO `user` (name, email, password, phone_number, photo_user)
          VALUES (@Name, @Email, @Password, @PhoneNumber, @PhotoUser);
          SELECT LAST_INSERT_ID();",
        new
        {
            request.Name,
            request.Email,
            Password = hashpassword,   
            request.PhoneNumber,
            request.PhotoUser
        });

    return Results.Created($"/usuario/{iduser}",
        new { id = iduser, request.Name, request.Email, request.PhoneNumber, request.PhotoUser });
});
//Login usuario
app.MapPost("/login", async (LoginRequest request) =>
{
    // Validación de datos
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Ingresa email y password correctos");
    }
    await using var conexion = new MySqlConnection(cs);
    var usuario = await conexion.QuerySingleOrDefaultAsync<UserDB>(
        @"SELECT id_user AS IDUser, name AS Name,email AS Email, password AS Password, phone_number AS PhoneNumber, photo_user AS PhotoUser FROM `user` WHERE email = @Email",
        new { request.Email });

    if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
    {
        return Results.Json(new { message = "Correo o contraseña incorrectos" }, statusCode: 401);
    }

    return Results.Ok(new
    {
        id = usuario.IDUser,
        name = usuario.Name,
        email = usuario.Email,
        phoneNumber = usuario.PhoneNumber,
        photoUser = usuario.PhotoUser

    });
});

// ── Camiones ─────────────────────────────────────────────────────────────
// /buses: solo el nombre del camión. /routes: nombre, precio, tiempo estimado (minutos) y descripción de las rutas activas.
app.MapGet("/buses", async () =>
{
    try
    {
        await using var conexion = new MySqlConnection(cs);
        var filas = await conexion.QueryAsync<BusDto>(
            "SELECT id_bus AS Id, name AS Name FROM `bus` ORDER BY id_bus");
        return Results.Ok(filas);
    }
    catch (Exception ex)
    {
        return Results.Problem("No se pudieron leer los buses: " + ex.Message);
    }
});

app.MapGet("/routes", async () =>
{
    try
    {
        await using var conexion = new MySqlConnection(cs);
        var filas = await conexion.QueryAsync<RutaDto>(
            @"SELECT id_routes AS Id, id_bus AS IdBus, name AS Name, price AS Price,
                     TIME_TO_SEC(travel_time) / 60 AS TravelMinutes, description AS Description
              FROM `routes`
              WHERE active_inactive = 1
              ORDER BY id_routes");
        return Results.Ok(filas);
    }
    catch (Exception ex)
    {
        return Results.Problem("No se pudieron leer las rutas: " + ex.Message);
    }
});}

//Paradas (de camiones)
app.MapGet("/stops", async () =>
{
    try
    {
        await using var conexion = new MySqlConnection(cs);
        var filas = await conexion.QueryAsync(
            @"SELECT id_stop AS Id, id_routes AS IdRoutes, name AS Name, latitude AS Latitude, longitude AS Longitude
              FROM `stop`
              ORDER BY name");
        return Results.Ok(filas);
    }
    catch (Exception ex)
    {
        return Results.Problem("No se pudieron leer las paradas: " + ex.Message);

    }

});
//Paradas por rutas
app.MapGet("/routes/{id:int}", async (int id) =>
{
    try
    {
        await using var conexion = new MySqlConnection(cs);
        var route = await conexion.QuerySingleOrDefaultAsync<RutaDto>(
            @"SELECT id_routes AS Id, id_bus AS IdBus, name AS Name, price AS Price,
                     TIME_TO_SEC(travel_time) / 60 AS TravelMinutes, description AS Description
              FROM `routes`
              WHERE id_routes = @Id AND active_inactive = 1",
            new { Id });
        if (route is null)
            return Results.NotFound("Ruta no encontrada");
        var stop = await conexion.QueryAsync<StopOrdenDta>(
            @"SELECT s.id_stop AS Id, s.id_routes AS IdRoutes, s.name AS Name, s.stop_order AS StopOrder
              FROM `route_stops` rs
                JOIN `stops` s ON rs.id_stop = s.id_stop
              WHERE rs.id_routes = @Id
              ORDER BY rs.stop_order",
            new { Id });
        return Results.Ok(new { route, stops });

    }
    catch (Exception ex)
    {
        return Results.Problem("No se pudieron leer las rutas y paradas: " + ex.Message);
    }

});

app.Run();
//clases para los endpoints

public class BusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
public class RutaDto
{
    public int Id { get; set; }
    public int IdBus { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public double TravelMinutes { get; set; }
    public string? Description { get; set; }
}
public class RegistroRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string PhotoUser { get; set; } = "";
}
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
public class UserDB
{
    public int IDUser { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string PhotoUser { get; set; } = "";
}
public class StopDto
{
    public int Id { get; set; }
    public int IdRoutes { get; set; }
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }   
}
public class StopOrdenDta : StopDto
{
    public int StopOrder { get; set; }

}
