using BCrypt.Net;
using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;



namespace RutaApp.Blazor
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? Foto { get; set; }
    }

    // Guarda quién inició sesión mientras la app está abierta
    public static class Sesion
    {
        public static Usuario? Actual { get; set; }
    }

    public class UsuarioService
    {
        // La llegada del user desde la database
        private class UsuarioDb
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public string Correo { get; set; } = "";
            public string Hash { get; set; } = "";
            public string? Foto { get; set; }
        }

        // CRUD 
        // CREATE/ registrar el ususario
        public async Task<(Usuario? Usuario, string? Error)> RegistrarAsync(
            string nombre, string correo, string password)
        {
            nombre = nombre.Trim();
            correo = correo.Trim();

            if (nombre.Length < 2) return (null, "Escribe tu nombre completo.");
            if (!correo.Contains('@')) return (null, "Escribe un correo válido.");
            if (password.Length < 6) return (null, "La contraseña debe tener al menos 6 caracteres.");

            await using var db = new MySqlConnection(Connection.ConnectionString);

            // Verificar existencia del correo
            var existe = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM `user` WHERE email = @correo", new { correo });
            if (existe > 0) return (null, "Ese correo ya está registrado.");

            // Uso del hash para encriptar contraseña
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            var id = await db.ExecuteScalarAsync<int>(
                @"INSERT INTO `user` (name, email, password)
              VALUES (@nombre, @correo, @hash);
              SELECT LAST_INSERT_ID();",
                new { nombre, correo, hash });

            return (new Usuario { Id = id, Nombre = nombre, Correo = correo }, null);
        }

        // READ/ Iniciar sesión
        // Confirma si ñas creedenciales son correctas 
        public async Task<Usuario?> IniciarSesionAsync(string correo, string password)
        {
            await using var db = new MySqlConnection(Connection.ConnectionString);

            var fila = await db.QueryFirstOrDefaultAsync<UsuarioDb>(
                @"SELECT id_user AS Id, name AS Nombre, email AS Correo,
                     password AS Hash, photo_user AS Foto
              FROM `user` WHERE email = @correo",
                new { correo = correo.Trim() });

            // Si no son correctos devuelve null, si son correctos devuelve el usuario
            if (fila is null || !BCrypt.Net.BCrypt.Verify(password, fila.Hash))
                return null;

            return new Usuario
            {
                Id = fila.Id,
                Nombre = fila.Nombre,
                Correo = fila.Correo,
                Foto = fila.Foto
            };
        }
        public async Task<Usuario?> ObtenerAsync(int id)
        {
            await using var db = new MySqlConnection(Connection.ConnectionString);

            return await db.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT id_user AS Id, name AS Nombre, email AS Correo, photo_user AS Foto
              FROM `user` WHERE id_user = @id",
                new { id });
        }

        // Cambio de nombre del usuario 
        public async Task<bool> ActualizarNombreAsync(int id, string nombre)
        {
            nombre = nombre.Trim();
            if (nombre.Length < 2) return false;

            await using var db = new MySqlConnection(Connection.ConnectionString);

            var filas = await db.ExecuteAsync(
                "UPDATE `user` SET name = @nombre WHERE id_user = @id",
                new { nombre, id });

            return filas > 0;
        }

        // Cambiar la foto
        public async Task<bool> ActualizarFotoAsync(int id, string? foto)
        {
            await using var db = new MySqlConnection(Connection.ConnectionString);

            var filas = await db.ExecuteAsync(
                "UPDATE `user` SET photo_user = @foto WHERE id_user = @id",
                new { foto, id });

            return filas > 0;
        }

        // Eliminar la cuenta
        public async Task<bool> EliminarAsync(int id)
        {
            await using var db = new MySqlConnection(Connection.ConnectionString);
            await db.OpenAsync();
            await using var tx = await db.BeginTransactionAsync();
            try
            {
             //Se borra lo que depende del usuario, porque si no, no deja 
                string[] dependientes =
                {
                "DELETE FROM trip_result WHERE id_trip IN (SELECT id_trip FROM trip WHERE id_user = @id)",
                "DELETE FROM trip WHERE id_user = @id",
                "DELETE FROM favorite WHERE id_user = @id",
                "DELETE FROM preference WHERE id_user = @id",
                "DELETE FROM report WHERE id_user = @id",
                "DELETE FROM photo WHERE id_post IN (SELECT id_post FROM post WHERE id_user = @id)",
                "DELETE FROM `comment` WHERE id_user = @id OR id_post IN (SELECT id_post FROM post WHERE id_user = @id)",
                "DELETE FROM post WHERE id_user = @id"
            };

                foreach (var sql in dependientes)
                    await db.ExecuteAsync(sql, new { id }, tx);

                var filas = await db.ExecuteAsync(
                    "DELETE FROM `user` WHERE id_user = @id", new { id }, tx);

                await tx.CommitAsync();
                return filas > 0;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }

}
