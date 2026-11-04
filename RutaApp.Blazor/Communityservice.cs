using Dapper;
using MySqlConnector;

namespace RutaApp.Blazor;
// Representa una publicación en la comunidad
public class PostDb
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Autor { get; set; } = "";
    public string Tipo { get; set; } = "aviso";
    public string? Titulo { get; set; }
    public string Contenido { get; set; } = "";
    public DateTime Fecha { get; set; }
    public int? IdRuta { get; set; }
    public int Comentarios { get; set; }
    public int Likes { get; set; }
    public int MeGusta { get; set; }
    public List<string> Fotos { get; set; } = new();
}
// Representa un comentario en una publicación de la comunidad
public class ComentarioDb
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Autor { get; set; } = "";
    public string Contenido { get; set; } = "";
    public DateTime Fecha { get; set; }
}
// Servicio para manejar la comunidad de usuarios
public class ComunidadService
{
    // Obtiene las publicaciones de la comunidad
    public async Task<List<PostDb>> ObtenerPublicacionesAsync(int idUser)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        //obtiene publicaciones de la comunidad  (autor, fecha,etc)

        var posts = (await db.QueryAsync<PostDb>(
            @"SELECT p.id_post AS Id, p.id_user AS IdUsuario, u.name AS Autor,
                     p.type AS Tipo, p.title AS Titulo, p.content AS Contenido,
                     p.created_at AS Fecha, p.id_routes AS IdRuta,
                     (SELECT COUNT(*) FROM comment c WHERE c.id_post = p.id_post) AS Comentarios,
                     (SELECT COUNT(*) FROM post_like l WHERE l.id_post = p.id_post) AS Likes,
                     (SELECT COUNT(*) FROM post_like l
                       WHERE l.id_post = p.id_post AND l.id_user = @idUser) AS MeGusta
              FROM post p
              JOIN `user` u ON u.id_user = p.id_user
              ORDER BY p.created_at DESC
              LIMIT 100", new { idUser })).ToList();

        if (posts.Count == 0) return posts;

        var fotos = await db.QueryAsync<(int IdPost, string Url)>(
            "SELECT id_post, url FROM photo WHERE id_post IN @ids ORDER BY id_photo",
            new { ids = posts.Select(p => p.Id).ToArray() });
        foreach (var f in fotos)
            posts.First(p => p.Id == f.IdPost).Fotos.Add(f.Url);

        return posts;
    }
    // Crea una nueva publicación en la comunidad

    public async Task<int> CrearAsync(int idUser, string tipo, string? titulo, string contenido,
                                      int? idRuta, List<string>? fotos)
    {
        if (string.IsNullOrWhiteSpace(contenido)) return 0;

        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            // Inserta la publicación y obtiene su ID
            var id = await db.ExecuteScalarAsync<int>(
                @"INSERT INTO post (id_user, type, title, content, id_routes, created_at)
                  VALUES (@idUser, @tipo, @titulo, @contenido, @idRuta, NOW());
                  SELECT LAST_INSERT_ID();",
                new { idUser, tipo, titulo, contenido = contenido.Trim(), idRuta }, tx);

            foreach (var url in fotos ?? new List<string>())
                await db.ExecuteAsync(
                    "INSERT INTO photo (id_post, url, created_at) VALUES (@id, @url, NOW())",
                    new { id, url }, tx);

            await tx.CommitAsync();
            return id;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    // Obtiene los comentarios de una publicación
    public async Task<List<ComentarioDb>> ObtenerComentariosAsync(int idPost)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        return (await db.QueryAsync<ComentarioDb>(
            @"SELECT c.id_comment AS Id, c.id_user AS IdUsuario, u.name AS Autor,
                     c.content AS Contenido, c.created_at AS Fecha
              FROM comment c JOIN `user` u ON u.id_user = c.id_user
              WHERE c.id_post = @idPost ORDER BY c.created_at", new { idPost })).ToList();
    }
    // Agrega un comentario a una publicación
    public async Task<bool> ComentarAsync(int idUser, int idPost, string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido)) return false;
        await using var db = new MySqlConnection(Connection.ConnectionString);
        var filas = await db.ExecuteAsync(
            @"INSERT INTO comment (id_post, id_user, content, created_at)
              VALUES (@idPost, @idUser, @contenido, NOW())",
            new { idPost, idUser, contenido = contenido.Trim() });
        return filas > 0;
    }

    // Pone o quita el "me gusta"
    public async Task<bool> AlternarLikeAsync(int idUser, int idPost)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        var quitados = await db.ExecuteAsync(
            "DELETE FROM post_like WHERE id_post = @idPost AND id_user = @idUser", new { idPost, idUser });
        if (quitados > 0) return false;
        await db.ExecuteAsync(
            "INSERT IGNORE INTO post_like (id_post, id_user) VALUES (@idPost, @idUser)", new { idPost, idUser });
        return true;
    }
    // Elimina una publicación de la comunidad, solo si es del usuario
    public async Task<bool> EliminarAsync(int idUser, int idPost)
    {
        await using var db = new MySqlConnection(Connection.ConnectionString);
        await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            var p = new { idPost, idUser };
            // solo si es el autor puede borrar su publicación
            var esSuya = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM post WHERE id_post = @idPost AND id_user = @idUser", p, tx);
            if (esSuya == 0) return false;

            await db.ExecuteAsync("DELETE FROM post_like WHERE id_post = @idPost", p, tx);
            await db.ExecuteAsync("DELETE FROM `comment` WHERE id_post = @idPost", p, tx);
            await db.ExecuteAsync("DELETE FROM photo WHERE id_post = @idPost", p, tx);
            await db.ExecuteAsync("DELETE FROM post WHERE id_post = @idPost", p, tx);
            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}