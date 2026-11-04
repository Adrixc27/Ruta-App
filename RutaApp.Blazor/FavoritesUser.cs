using Dapper;
using MySqlConnector;

namespace RutaApp.Blazor;

public class FavoritoService
{
    //selecciona todas las rutas favoritas de un usuario
    public async Task<List<int>> ObtenerRutasAsync(int idUser)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        var ids = await db.QueryAsync<int>(
            "SELECT id_routes FROM favorite_route WHERE id_user = @idUser", new { idUser });
        return ids.ToList();
    }
    //agrega una ruta a los favoritos de un usuario

    public async Task AgregarAsync(int idUser, int idRoutes)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.ExecuteAsync(
            "INSERT IGNORE INTO favorite_route (id_user, id_routes) VALUES (@idUser, @idRoutes)",
            new { idUser, idRoutes });
    }
    //elimina una ruta de los favoritos de un usuario
    public async Task QuitarAsync(int idUser, int idRoutes)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.ExecuteAsync(
            "DELETE FROM favorite_route WHERE id_user = @idUser AND id_routes = @idRoutes",
            new { idUser, idRoutes });
    }
}