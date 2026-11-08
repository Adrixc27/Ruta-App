using System.Net.Sockets;
using System.Text.Json;
using MySqlConnector;

namespace RutaApp.Blazor
{
    /// <summary>Datos para conectarse a MySQL. Se guardan en este equipo (conexion.json), no en el código.</summary>
    public class ConfigConexion
    {
        public string Servidor { get; set; } = "localhost";
        public int Puerto { get; set; } = 3306;
        public string BaseDeDatos { get; set; } = "rutadb";
        public string Usuario { get; set; } = "root";
        public string Contrasena { get; set; } = "";

        public ConfigConexion Copiar() => (ConfigConexion)MemberwiseClone();
    }

    /// <summary>
    /// Conexión a la base de datos MySQL LOCAL. De dónde sale la configuración (en este orden):
    ///  1) la variable de entorno RUTA_DB_CONNECTION (cadena de conexión completa), si existe;
    ///  2) el archivo %LocalAppData%\RutaApp\conexion.json (se crea solo la primera vez con los valores de un MySQL local);
    /// Para cambiar usuario, contraseña o base de datos se edita ese archivo y se vuelve a abrir la app.
    /// La contraseña NO va en el código: así no se sube a git.
    /// </summary>
    public static class Connection
    {
        /// <summary>Archivo donde se guarda la configuración de este equipo.</summary>
        public static string Archivo => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp", "conexion.json");

        public static ConfigConexion Actual { get; private set; } = new();

        /// <summary>Cadena de conexión que usan todos los servicios (Dapper + MySqlConnector).</summary>
        public static string ConnectionString { get; private set; } = "";

        /// <summary>true si la cadena viene de la variable de entorno RUTA_DB_CONNECTION (entonces conexion.json no se usa).</summary>
        public static bool UsaVariableDeEntorno { get; private set; }

        static Connection()
        {
            Recargar();
        }

        public static void Recargar()
        {
            Actual = LeerArchivo();
            string entorno = Environment.GetEnvironmentVariable("RUTA_DB_CONNECTION");
            UsaVariableDeEntorno = !string.IsNullOrWhiteSpace(entorno);
            ConnectionString = UsaVariableDeEntorno ? entorno : Construir(Actual);
        }

        /// <summary>Guarda la configuración y la empieza a usar al momento.</summary>
        public static void Guardar(ConfigConexion config)
        {
            config = Normalizar(config);
            Directory.CreateDirectory(Path.GetDirectoryName(Archivo));
            File.WriteAllText(Archivo, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            Actual = config;
            if (!UsaVariableDeEntorno) ConnectionString = Construir(config);
            MySqlConnection.ClearAllPools();   // las conexiones abiertas con los datos viejos se descartan
        }

        private static ConfigConexion LeerArchivo()
        {
            try
            {
                if (File.Exists(Archivo))
                    return Normalizar(JsonSerializer.Deserialize<ConfigConexion>(File.ReadAllText(Archivo)));
            }
            catch (Exception)
            {
                // archivo dañado: se vuelve a los valores locales por defecto
            }

            var predeterminada = new ConfigConexion();
            try { Guardar(predeterminada); } catch (Exception) { }   // se deja creado para que sea fácil encontrarlo y editarlo
            return predeterminada;
        }

        private static ConfigConexion Normalizar(ConfigConexion c)
        {
            c ??= new ConfigConexion();
            return new ConfigConexion
            {
                Servidor = string.IsNullOrWhiteSpace(c.Servidor) ? "localhost" : c.Servidor.Trim(),
                Puerto = c.Puerto is > 0 and <= 65535 ? c.Puerto : 3306,
                BaseDeDatos = string.IsNullOrWhiteSpace(c.BaseDeDatos) ? "rutadb" : c.BaseDeDatos.Trim(),
                Usuario = string.IsNullOrWhiteSpace(c.Usuario) ? "root" : c.Usuario.Trim(),
                Contrasena = c.Contrasena ?? ""
            };
        }

        private static bool EsLocal(string servidor) =>
            servidor.Equals("localhost", StringComparison.OrdinalIgnoreCase) || servidor == "127.0.0.1" || servidor == "::1";

        public static string Construir(ConfigConexion c)
        {
            c = Normalizar(c);
            return new MySqlConnectionStringBuilder
            {
                Server = c.Servidor,
                Port = (uint)c.Puerto,
                Database = c.BaseDeDatos,
                UserID = c.Usuario,
                Password = c.Contrasena,
                SslMode = EsLocal(c.Servidor) ? MySqlSslMode.Preferred : MySqlSslMode.Required,   // en un servidor remoto siempre cifrado
                AllowPublicKeyRetrieval = true,   // MySQL 8 (caching_sha2_password) sin SSL lo necesita
                ConnectionTimeout = 6
            }.ConnectionString;
        }

        /// <summary>
        /// Prueba la conexión (con la configuración dada, o la actual) y explica en español qué falla.
        /// Si funciona, dice cuántas rutas y paradas encontró.
        /// </summary>
        public static async Task<(bool Ok, string Mensaje)> ProbarAsync(ConfigConexion config = null)
        {
            bool actual = config == null;
            config = actual ? Actual : Normalizar(config);
            string cadena = actual && UsaVariableDeEntorno ? ConnectionString : Construir(config);
            string donde = config.Servidor + ":" + config.Puerto;
            try
            {
                using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await using var db = new MySqlConnection(cadena);
                await db.OpenAsync(limite.Token);

                await using var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT (SELECT COUNT(*) FROM routes), (SELECT COUNT(*) FROM stop)";
                await using var lector = await cmd.ExecuteReaderAsync(limite.Token);
                await lector.ReadAsync(limite.Token);
                long rutas = lector.GetInt64(0), paradas = lector.GetInt64(1);

                return rutas == 0
                    ? (true, "Conectado a «" + config.BaseDeDatos + "», pero no tiene rutas todavía: importa los datos de rutas y paradas (carpeta Database).")
                    : (true, "Conexión correcta: " + rutas + " rutas y " + paradas + " paradas en «" + config.BaseDeDatos + "».");
            }
            catch (MySqlException ex)
            {
                return (false, ex.ErrorCode switch
                {
                    MySqlErrorCode.AccessDenied => "MySQL respondió, pero el usuario o la contraseña de «" + config.Usuario + "» no son correctos.",
                    MySqlErrorCode.UnknownDatabase => "MySQL respondió, pero la base de datos «" + config.BaseDeDatos + "» no existe. Créala con Database/esquema.sql.",
                    MySqlErrorCode.NoSuchTable => "Se conectó a «" + config.BaseDeDatos + "», pero faltan tablas. Créalas con Database/esquema.sql.",
                    MySqlErrorCode.UnableToConnectToHost => "No se pudo conectar a " + donde + ". ¿Está encendido el servicio MySQL y es ese el puerto?",
                    _ when ex.InnerException is SocketException => "No se pudo conectar a " + donde + ". ¿Está encendido el servicio MySQL y es ese el puerto?",
                    _ => "MySQL respondió con un error: " + ex.Message
                });
            }
            catch (OperationCanceledException)
            {
                return (false, "MySQL en " + donde + " no respondió a tiempo.");
            }
            catch (Exception ex)
            {
                return (false, "No se pudo conectar a " + donde + ": " + ex.Message);
            }
        }
    }
}
