# Base de datos local (MySQL)

R.U.T.A. se conecta **directo a MySQL** (no hay API). La conexión se configura en la app:
**Configuración → Conexión a la base de datos** (ahí también se prueba). Se guarda en `%LocalAppData%\RutaApp\conexion.json`,
fuera del proyecto, así que la contraseña no se sube a git. (Opcional: la variable de entorno `RUTA_DB_CONNECTION` con una cadena completa tiene prioridad.)

## Si ya tienes la base `rutadb` en tu MySQL con datos
No hace falta nada más: escribe tu usuario y contraseña en la pantalla de conexión y pulsa **Probar conexión**.

## Si tu MySQL local está vacío
1. Crea las tablas (el servicio de Windows `MySQL80` debe estar encendido):
   ```
   "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p < Database/esquema.sql
   ```
   `esquema.sql` está deducido de las consultas de la app; usa `CREATE ... IF NOT EXISTS`, así que no pisa una base existente.
2. Las rutas y paradas son datos, no esquema. Para copiarlos desde otra base de datos que ya los tenga (por ejemplo la de la nube), exporta solo esas tablas:
   ```
   mysqldump -h <SERVIDOR> -P <PUERTO> -u <USUARIO> -p --ssl-mode=REQUIRED --no-create-info --skip-triggers --set-gtid-purged=OFF rutadb bus routes stop route_stop > Database/datos_rutas.sql
   ```
   y luego impórtalas en tu MySQL local:
   ```
   "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p rutadb < Database/datos_rutas.sql
   ```
   (`datos_*.sql` está en `.gitignore`: no lo subas.)
3. En la app: Configuración → Conexión a la base de datos → **Probar conexión**. Debe decir cuántas rutas y paradas encontró.
