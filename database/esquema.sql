-- R.U.T.A. — esquema de la base de datos MySQL local (rutadb).
-- Está deducido de las consultas que hace la app (RutaApp.Blazor): sirve para crear una base VACÍA y que la app arranque.
-- Es seguro correrlo varias veces: usa CREATE ... IF NOT EXISTS. Si ya tienes tu propia base con datos, NO lo necesitas.
--
-- Cómo correrlo (desde la carpeta del repositorio):
--   "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p < Database/esquema.sql
-- o abre este archivo en MySQL Workbench y pulsa el rayo.

CREATE DATABASE IF NOT EXISTS rutadb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE rutadb;

-- Cuentas de usuario (la contraseña se guarda con hash BCrypt; la foto es una imagen en base64)
CREATE TABLE IF NOT EXISTS `user` (
  id_user    INT AUTO_INCREMENT PRIMARY KEY,
  name       VARCHAR(100) NOT NULL,
  email      VARCHAR(150) NOT NULL UNIQUE,
  password   VARCHAR(100) NOT NULL,
  photo_user LONGTEXT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Camiones y rutas
CREATE TABLE IF NOT EXISTS bus (
  id_bus INT AUTO_INCREMENT PRIMARY KEY,
  name   VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS routes (
  id_routes       INT AUTO_INCREMENT PRIMARY KEY,
  name            VARCHAR(150) NOT NULL,
  grupo           VARCHAR(150) NULL,          -- agrupa variantes de una misma ruta (Komatsu Directo / Inverso)
  variante        VARCHAR(100) NULL,
  description     TEXT NULL,
  price           DECIMAL(6,2) NULL,
  travel_time     TIME NULL,
  estimated_time  TIME NULL,
  trazo           LONGTEXT NULL,              -- recorrido por calles (polyline de precisión 6)
  id_bus          INT NULL,
  active_inactive TINYINT(1) NOT NULL DEFAULT 1,
  CONSTRAINT fk_routes_bus FOREIGN KEY (id_bus) REFERENCES bus (id_bus)
);

CREATE TABLE IF NOT EXISTS stop (
  id_stop   INT AUTO_INCREMENT PRIMARY KEY,
  name      VARCHAR(200) NULL,
  latitude  DOUBLE NOT NULL,
  longitude DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS route_stop (
  id_route   INT NOT NULL,
  id_stop    INT NOT NULL,
  stop_order INT NOT NULL,
  PRIMARY KEY (id_route, stop_order),
  CONSTRAINT fk_rs_route FOREIGN KEY (id_route) REFERENCES routes (id_routes),
  CONSTRAINT fk_rs_stop  FOREIGN KEY (id_stop)  REFERENCES stop (id_stop)
);

-- Rutas guardadas por cada usuario
CREATE TABLE IF NOT EXISTS favorite_route (
  id_user   INT NOT NULL,
  id_routes INT NOT NULL,
  PRIMARY KEY (id_user, id_routes),
  CONSTRAINT fk_fr_user  FOREIGN KEY (id_user)   REFERENCES `user` (id_user),
  CONSTRAINT fk_fr_route FOREIGN KEY (id_routes) REFERENCES routes (id_routes)
);

-- Comunidad: publicaciones, fotos, comentarios y "me gusta"
CREATE TABLE IF NOT EXISTS post (
  id_post    INT AUTO_INCREMENT PRIMARY KEY,
  id_user    INT NOT NULL,
  type       VARCHAR(20) NOT NULL,            -- aviso | pregunta | propuesta
  title      VARCHAR(150) NULL,
  content    TEXT NOT NULL,
  id_routes  INT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_post_user  FOREIGN KEY (id_user)   REFERENCES `user` (id_user),
  CONSTRAINT fk_post_route FOREIGN KEY (id_routes) REFERENCES routes (id_routes)
);

CREATE TABLE IF NOT EXISTS photo (
  id_photo   INT AUTO_INCREMENT PRIMARY KEY,
  id_post    INT NOT NULL,
  url        LONGTEXT NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_photo_post FOREIGN KEY (id_post) REFERENCES post (id_post)
);

CREATE TABLE IF NOT EXISTS `comment` (
  id_comment INT AUTO_INCREMENT PRIMARY KEY,
  id_post    INT NOT NULL,
  id_user    INT NOT NULL,
  content    TEXT NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_comment_post FOREIGN KEY (id_post) REFERENCES post (id_post),
  CONSTRAINT fk_comment_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);

CREATE TABLE IF NOT EXISTS post_like (
  id_post INT NOT NULL,
  id_user INT NOT NULL,
  PRIMARY KEY (id_post, id_user),
  CONSTRAINT fk_like_post FOREIGN KEY (id_post) REFERENCES post (id_post),
  CONSTRAINT fk_like_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);

-- Tablas que la app limpia al borrar una cuenta (el resto de su contenido depende de cómo las uses)
CREATE TABLE IF NOT EXISTS preference (
  id_user              INT PRIMARY KEY,
  importance_cost      INT NOT NULL DEFAULT 0,
  importance_time      INT NOT NULL DEFAULT 0,
  importance_transfers INT NOT NULL DEFAULT 0,
  CONSTRAINT fk_pref_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);

CREATE TABLE IF NOT EXISTS favorite (
  id_favorite INT AUTO_INCREMENT PRIMARY KEY,
  id_user     INT NOT NULL,
  CONSTRAINT fk_fav_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);

CREATE TABLE IF NOT EXISTS trip (
  id_trip INT AUTO_INCREMENT PRIMARY KEY,
  id_user INT NOT NULL,
  CONSTRAINT fk_trip_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);

CREATE TABLE IF NOT EXISTS trip_result (
  id_trip_result INT AUTO_INCREMENT PRIMARY KEY,
  id_trip        INT NOT NULL,
  CONSTRAINT fk_tr_trip FOREIGN KEY (id_trip) REFERENCES trip (id_trip)
);

CREATE TABLE IF NOT EXISTS report (
  id_report INT AUTO_INCREMENT PRIMARY KEY,
  id_user   INT NOT NULL,
  CONSTRAINT fk_report_user FOREIGN KEY (id_user) REFERENCES `user` (id_user)
);
