using Dapper;
using MySqlConnector;

namespace RutaApp.Datos;

public class Preferencias
{
    public int Costo { get; set; }
    public int Tiempo { get; set; }
    public int Transbordos { get; set; }
}
public class PreferenciaService
{
    public async Task<Preferencias?> ObtenerAsync(int idUser)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        return await db.QueryFirstOrDefaultAsync<Preferencias>(
            @"SELECT importance_cost AS Costo, importance_time AS Tiempo,
                     importance_transfers AS Transbordos
              FROM preference WHERE id_user = @idUser", new { idUser });
    }
    public async Task GuardarAsync(int idUser, Preferencias p)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.ExecuteAsync(
            @"INSERT INTO preference (id_user, importance_cost, importance_time, importance_transfers)
              VALUES (@idUser, @Costo, @Tiempo, @Transbordos)
              ON DUPLICATE KEY UPDATE importance_cost = @Costo,
                                      importance_time = @Tiempo,
                                      importance_transfers = @Transbordos",
            new { idUser, p.Costo, p.Tiempo, p.Transbordos });
    }
}