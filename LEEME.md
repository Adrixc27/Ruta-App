# R.U.T.A.

App de escritorio en **WPF + Blazor**: la ventana es WPF (en C#, sin XAML) y toda la interfaz son componentes Blazor dentro de un
`BlazorWebView`. No hace falta navegador ni servidor.

## Estructura

El código está separado en tres secciones. Cada una depende solo de la que está debajo: Frontend → Backend → Database.

```
src/
├─ Frontend/
│  ├─ RutaApp.Escritorio/   Ventana WPF, index.html, Leaflet e imágenes; ubicación y portapapeles de Windows
│  └─ RutaApp.Interfaz/     Interfaz Blazor: páginas, componentes, estado de la sesión y hoja de estilos
├─ Backend/
│  ├─ RutaApp.Nucleo/       Lógica de la app: planificador, buscador de lugares, recorridos (OSRM/Overpass), ajustes
│  └─ RutaApp.Motor/        Motor de rutas (grafo, Dijkstra, multicriterio). Lo cubren las pruebas
└─ Database/
   ├─ RutaApp.Datos/        Acceso a MySQL (Dapper): usuarios, rutas, comunidad, favoritos, preferencias
   └─ Scripts/esquema.sql   Tablas de la base de datos
tests/
└─ RutaApp.Pruebas/         Pruebas xUnit del motor de rutas
```

| Dónde | Qué hay |
|---|---|
| `RutaApp.Interfaz/Components/Pages` | Landing, Sobre nosotros, Acceso/Registro, Planificar, Comparar, Rutas, Comunidad, Perfil, Configuración |
| `RutaApp.Interfaz/Components/Layout` | `MainLayout` (páginas públicas) y `ShellLayout` (sidebar + pantallas de la app) |
| `RutaApp.Interfaz/Components/Shared` | Mapa (`Mapa.razor` + `MapaLeaflet`), carrusel, intro, iconos, campos, diálogos |
| `RutaApp.Interfaz/Components/Ajustes` | Bloques de diseño de las pantallas de Configuración |
| `RutaApp.Interfaz/Servicios` | `EstadoApp` (partida en archivos por tema), registro de servicios, teclado y portapapeles |
| `RutaApp.Interfaz/Idiomas` | Español e inglés: `Traductor` y los diccionarios `Ingles*.cs` (un texto sin traducción se muestra en español) |
| `RutaApp.Interfaz/Estilos` | La hoja de estilos, escrita en C# (`Tokens` para colores y `Css*` por sección) |

## Ejecutar

```bash
dotnet run --project src/Frontend/RutaApp.Escritorio
```

o en Visual Studio: abre `RutaApp.slnx` y pon `RutaApp.Escritorio` como proyecto de inicio. Requiere el SDK de .NET 10 y el WebView2 Runtime
(viene con Windows 11 y Edge). Las pruebas:

```bash
dotnet test tests/RutaApp.Pruebas
```

## Base de datos

No hay API: la app se conecta directo a **MySQL local**. La conexión se configura en `%LocalAppData%\RutaApp\conexion.json`
(se crea solo la primera vez; si no conecta, la pantalla Rutas explica por qué). Para crear las tablas y cargar las rutas, mira
[`src/Database/LEEME.md`](src/Database/LEEME.md).

## Notas

- **JavaScript mínimo:** solo está Leaflet (`lib/leaflet`) y `RutaApp.Interfaz/wwwroot/mapa.js`, un adaptador que C# usa para dibujar el mapa.
  La accesibilidad, el portapapeles, la ubicación y los estilos son C# y Blazor.
- **Estilos en C#:** `Hoja.Completa` junta las secciones en el orden de la cascada y el componente `HojaEstilos` la inserta como `<style>`.
  El archivo `leaflet.css` es de la librería y se carga desde `index.html`, igual que la fuente de Google Fonts.
- El estado vive mientras la app está abierta; al cerrarla se pierde (salvo lo que se guarda en MySQL y en `%LocalAppData%\RutaApp`).
- El mapa y la fuente necesitan conexión a internet.
