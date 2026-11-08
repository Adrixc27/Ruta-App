using Dapper;
using MySqlConnector;

namespace RutaApp.Datos;

public class FavoritoService
{
    public async Task<List<int>> ObtenerRutasAsync(int idUser)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        var ids = await db.QueryAsync<int>(
            "SELECT id_routes FROM favorite_route WHERE id_user = @idUser", new { idUser });
        return ids.ToList();
    }

    public async Task AgregarAsync(int idUser, int idRoutes)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.ExecuteAsync(
            "INSERT IGNORE INTO favorite_route (id_user, id_routes) VALUES (@idUser, @idRoutes)",
            new { idUser, idRoutes });
    }
    public async Task QuitarAsync(int idUser, int idRoutes)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.ExecuteAsync(
            "DELETE FROM favorite_route WHERE id_user = @idUser AND id_routes = @idRoutes",
            new { idUser, idRoutes });
    }
}