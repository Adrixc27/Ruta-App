using Dapper;
using MySqlConnector;

namespace RutaApp.Datos;

/// <summary>Comentarios que la gente manda para mejorar la app (Configuración → Soporte).</summary>
public class OpinionService
{
    private const string CrearTabla = @"
        CREATE TABLE IF NOT EXISTS app_feedback (
          id_feedback INT AUTO_INCREMENT PRIMARY KEY,
          id_user     INT NULL,
          category    VARCHAR(30) NOT NULL,
          rating      TINYINT NULL,
          message     TEXT NOT NULL,
          language    VARCHAR(5) NOT NULL DEFAULT 'es',
          created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        )";

    private bool _tablaLista;

    /// <summary>Guarda el comentario. Sin cuenta, idUsuario es null. La tabla se crea sola si todavía no existe.</summary>
    public async Task EnviarAsync(int? idUsuario, string categoria, int? calificacion, string mensaje, string idioma)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        if (!_tablaLista)
        {
            await db.ExecuteAsync(CrearTabla);
            _tablaLista = true;
        }
        await db.ExecuteAsync(
            @"INSERT INTO app_feedback (id_user, category, rating, message, language)
              VALUES (@idUsuario, @categoria, @calificacion, @mensaje, @idioma)",
            new { idUsuario, categoria, calificacion, mensaje, idioma });
    }
}
