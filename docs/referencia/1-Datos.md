# Referencia de variables y funciones: RutaApp.Datos (MySQL)

Parte de la [guía del código](../EXPLICACION_CODIGO.md). Aquí se explica, archivo por archivo, cada variable (campo o propiedad) y cada función.

**Cómo leer los nombres:** `_nombre` es un campo privado de la clase; `Nombre` con mayúscula es una propiedad pública; las funciones `...Async` devuelven `Task` (se esperan con `await`).

**Patrón común de todos los servicios:** cada función abre una `MySqlConnection(Connection.ConnectionString)`, ejecuta SQL con Dapper (`QueryAsync`, `ExecuteAsync`) y cierra la conexión sola (`await using`). Los parámetros van como `@nombre` para evitar inyección SQL. Los alias del SELECT (`AS Id`) hacen que Dapper rellene las propiedades de la clase.

---

## Connection.cs

Decide con qué datos se conecta la app a MySQL.

### Clase `ConfigConexion` (datos de conexión)
| Propiedad | Valor por defecto | Qué es |
|---|---|---|
| `Servidor` | `localhost` | Equipo donde corre MySQL. |
| `Puerto` | `3306` | Puerto de MySQL. |
| `BaseDeDatos` | `rutadb` | Nombre de la base. |
| `Usuario` | `root` | Usuario de MySQL. |
| `Contrasena` | vacío | Contraseña; nunca va escrita en el código. |

### Clase estática `Connection`
| Miembro | Qué hace |
|---|---|
| `Archivo` | Ruta de `%LocalAppData%\RutaApp\conexion.json`, donde se guardan los datos de conexión. |
| `Actual` | La `ConfigConexion` en uso (solo se puede cambiar desde dentro de la clase). |
| `ConnectionString` | Texto de conexión que usan **todos** los servicios de esta carpeta. |
| `UsaVariableDeEntorno` | `true` si la conexión viene de la variable de entorno `RUTA_DB_CONNECTION` (entonces `conexion.json` se ignora). |
| constructor estático `Connection()` | Se ejecuta una sola vez al usar la clase por primera vez; llama a `Recargar`. |
| `Recargar()` | Lee `conexion.json`, revisa si existe la variable de entorno y calcula `ConnectionString`. |
| `Guardar(config)` | Normaliza y escribe `conexion.json`, actualiza `Actual` y `ConnectionString`, y descarta las conexiones abiertas con `ClearAllPools` (tenían los datos viejos). *Hoy ninguna pantalla lo llama.* |
| `LeerArchivo()` | Lee `conexion.json`. Si no existe o está dañado, usa los valores por defecto y los deja guardados para que sea fácil editarlos. |
| `Normalizar(c)` | Rellena lo que falta o sea inválido (servidor vacío → `localhost`, puerto fuera de 1–65535 → `3306`...). |
| `EsLocal(servidor)` | `true` si el servidor es `localhost`, `127.0.0.1` o `::1`. |
| `Construir(c)` | Arma la cadena de conexión: cifrado preferido en local y **obligatorio** en remoto; `AllowPublicKeyRetrieval` por MySQL 8; 6 s de espera máxima. |
| `ProbarAsync(config?)` | Abre una conexión de prueba (límite de 10 s), cuenta rutas y paradas y devuelve `(Ok, Mensaje)`. Traduce cada error de MySQL a un mensaje en español (contraseña incorrecta, base inexistente, faltan tablas, servicio apagado...). Lo llama `EstadoApp.CargarRutasRealesAsync`. |

---

## Servicios/UsuarioService.cs

### Clase `Usuario` y clase estática `Sesion`
| Miembro | Qué es |
|---|---|
| `Usuario.Id`, `Nombre`, `Correo` | Datos básicos de la cuenta. |
| `Usuario.Foto` | Foto de perfil como texto base64 (puede ser `null`). |
| `Sesion.Actual` | Usuario que inició sesión mientras la app está abierta; `null` si no hay sesión. Lo asignan `Auth.razor` y `EstadoApp`. |

### Clase `UsuarioService`
| Miembro | Qué hace |
|---|---|
| `UsuarioDb` (clase privada) | Fila de la tabla `user` con el `Hash` de la contraseña; solo se usa dentro de `IniciarSesionAsync` para no exponer el hash. |
| `RegistrarAsync(nombre, correo, password)` | Valida (nombre ≥ 2 letras, correo con `@`, contraseña ≥ 6), comprueba que el correo no exista, guarda **solo el hash BCrypt** y devuelve `(Usuario, Error)`: uno de los dos es `null`. |
| `IniciarSesionAsync(correo, password)` | Busca por correo y compara la contraseña con el hash (`BCrypt.Verify`). Devuelve el `Usuario` o `null` si no coincide. |
| `ObtenerAsync(id)` | Lee un usuario por su id (se usa para restaurar la sesión y refrescar la foto). |
| `ActualizarNombreAsync(id, nombre)` | Cambia el nombre (mínimo 2 letras). Devuelve `true` si se modificó una fila. |
| `ActualizarFotoAsync(id, foto)` | Guarda o quita (`null`) la foto en base64. |
| `EliminarAsync(id)` | Borra la cuenta dentro de una **transacción**: primero todo lo que depende del usuario (favoritos, viajes, preferencias, reportes, fotos, comentarios, likes y publicaciones) y al final el usuario; si algo falla, hace `Rollback` y no borra nada. |

---

## Servicios/RutaService.cs

Lee las rutas de la base y, si hace falta, calcula su recorrido por calles.

| Miembro | Qué hace |
|---|---|
| `_cache` | Lista de rutas ya leída; evita volver a consultar MySQL en cada pantalla. `null` = todavía no se ha leído. |
| `ObtenerRutasAsync()` | Si hay caché la devuelve. Si no, hace dos consultas (rutas activas con su bus, y paradas con su orden), une cada ruta con sus paradas y construye `RutaOficial`. Si la ruta tiene `trazo` guardado lo decodifica (ya sigue las calles); si no, usa una línea recta entre paradas y marca `TrazoEnCalles = false`. |
| `GenerarTrazoAsync(idRuta)` | Pide a OSRM el recorrido por calles entre las paradas (en grupos de 25, con 1 s de pausa entre peticiones), lo codifica y lo guarda en `routes.trazo`. Devuelve cuántos puntos tiene. *Hoy no se llama desde la app: es una herramienta de mantenimiento.* |
| `LimpiarCache()` | Olvida `_cache` para volver a leer de la base. |
| `Decodificar(texto)` | Convierte un *polyline* de precisión 6 (formato compacto de Google/OSRM) en lista de `[lat, lng]`. |
| `SiguienteValor(texto, ref i)` | Lee el siguiente número del polyline avanzando el índice `i` (`ref` = se modifica fuera de la función). |
| `Codificar(puntos)` | Inverso de `Decodificar`: convierte `[lat, lng]` en el texto compacto que se guarda en la base. |
| `Escribir(sb, valor)` | Escribe un número en el polyline usando bloques de 5 bits. |
| `RutaRow`, `ParadaRow` (clases privadas) | Moldes de las filas devueltas por cada SELECT. |

---

## Servicios/ComunidadService.cs

| Miembro | Qué hace |
|---|---|
| `PostDb` | Una publicación: `Id`, `IdUsuario`, `Autor`, `Tipo` (aviso/pregunta/propuesta), `Titulo`, `Contenido`, `Fecha`, `IdRuta`, conteos de `Comentarios` y `Likes`, `MeGusta` (1 si el usuario actual ya dio like) y `Fotos`. |
| `ComentarioDb` | Un comentario: `Id`, `IdUsuario`, `Autor`, `Contenido`, `Fecha`. |
| `ObtenerPublicacionesAsync(idUser)` | Las 100 publicaciones más recientes con autor y conteos (subconsultas `COUNT`), y luego sus fotos en una segunda consulta. `idUser` sirve para saber cuáles ya tienen "me gusta" suyo. |
| `CrearAsync(idUser, tipo, titulo, contenido, idRuta, fotos)` | Inserta la publicación y sus fotos en una transacción. Devuelve el id nuevo, o `0` si el texto está vacío. |
| `ObtenerComentariosAsync(idPost)` | Comentarios de una publicación, del más antiguo al más nuevo. |
| `ComentarAsync(idUser, idPost, contenido)` | Agrega un comentario (ignora textos vacíos). |
| `AlternarLikeAsync(idUser, idPost)` | Si ya había like lo quita y devuelve `false`; si no, lo agrega y devuelve `true`. |
| `EliminarAsync(idUser, idPost)` | Borra la publicación con sus likes, comentarios y fotos, **solo si es del usuario** (si no, devuelve `false`). |

---

## Servicios/FavoritoService.cs

| Función | Qué hace |
|---|---|
| `ObtenerRutasAsync(idUser)` | Ids de las rutas favoritas del usuario. |
| `AgregarAsync(idUser, idRoutes)` | Marca una ruta como favorita (`INSERT IGNORE`: si ya estaba, no falla). |
| `QuitarAsync(idUser, idRoutes)` | Quita la ruta de favoritas. |

## Servicios/PreferenciaService.cs

| Miembro | Qué hace |
|---|---|
| `Preferencias` | Tres números: `Costo`, `Tiempo` y `Transbordos` (importancia que el usuario da a cada criterio). |
| `ObtenerAsync(idUser)` | Lee las preferencias guardadas. |
| `GuardarAsync(idUser, p)` | Las guarda o actualiza (`ON DUPLICATE KEY UPDATE`). *Hoy ninguna pantalla lo usa.* |

## Servicios/OpinionService.cs

| Miembro | Qué hace |
|---|---|
| `CrearTabla` | Texto SQL que crea la tabla `app_feedback`. |
| `_tablaLista` | Recuerda que la tabla ya se creó para no repetirlo en cada envío. |
| `EnviarAsync(idUsuario, categoria, calificacion, mensaje, idioma)` | Crea la tabla si falta y guarda el comentario. `idUsuario` es `null` si no hay cuenta. |

---

## Modelos/RutaOficial.cs

| Miembro | Qué es |
|---|---|
| `RutaOficial.Id`, `Nombre` | Identificador y nombre de la ruta. |
| `Grupo`, `Variante` | Para unir ramales de una misma ruta (por ejemplo grupo "Komatsu", variantes "Directo" e "Inverso"). `Variante` vale "Único" si no hay. |
| `BusNombre` | Tipo/nombre del camión (tabla `bus`). |
| `Descripcion`, `Precio` | Texto y tarifa de la ruta. |
| `TiempoRecorrido`, `TiempoEstimado` | Duración medida y estimada. |
| `Paradas` | Lista de `Parada` en orden. |
| `Trazo` | Lista de puntos `[lat, lng]` del recorrido. |
| `Reparado`, `TrazoEnCalles` | Si ya se corrigieron los tramos que cortaban esquinas y si el trazo guardado sigue las calles. |
| `record Parada(Id, Nombre, Latitud, Longitud, Orden)` | Una parada; `Orden` es su posición en la ruta. |

## RegistroDatos.cs

| Función | Qué hace |
|---|---|
| `AgregarDatos(servicios)` | Registra como *singleton* (una sola instancia para toda la app) `RutaService`, `UsuarioService`, `FavoritoService`, `PreferenciaService`, `ComunidadService` y `OpinionService`. |

## Scripts/esquema.sql

Crea `rutadb` y las 16 tablas (`user`, `bus`, `routes`, `stop`, `route_stop`, `favorite_route`, `post`, `photo`, `comment`, `post_like`, `preference`, `favorite`, `trip`, `trip_result`, `report`, `app_feedback`). Las columnas más importantes: `user.password` guarda el hash BCrypt, `user.photo_user` la foto en base64, `routes.trazo` el recorrido codificado, y `route_stop.stop_order` el orden de las paradas.
