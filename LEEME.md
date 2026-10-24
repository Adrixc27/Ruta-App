# R.U.T.A.

App de escritorio en **WPF + Blazor**: la ventana es WPF (escrita en C#, sin archivos XAML) y toda la interfaz son componentes
Blazor que se muestran dentro de ella con un `BlazorWebView`. No hace falta navegador ni servidor.

| Proyecto | Qué es |
|---|---|
| `Ruta App.csproj` | **La app de escritorio** (.NET 10, WPF). `Program.cs` arranca, `MainWindow.cs` es la ventana con el `BlazorWebView`, `UbicacionWindows.cs` pide la ubicación del equipo y `wwwroot/` tiene `index.html`, Leaflet y las imágenes |
| `RutaApp.Blazor` | Biblioteca de componentes Blazor: páginas, layouts, servicios, mapa y `wwwroot/app.css` (estilos del diseño) |
| `MiApi` | API web (registro, login, camiones y rutas) |
| `Pruebas` | Pruebas xUnit del motor de rutas |

El motor de rutas (`Models/`, `Graph/`, `Algorithms/`, `Services/`, `Data/`) está en la raíz y lo comparten la biblioteca Blazor y las pruebas.

## Ejecutar

```bash
dotnet run --project "Ruta App.csproj"
```

o en Visual Studio: proyecto de inicio `Ruta App`. Requiere el SDK de .NET 10 y el WebView2 Runtime (viene con Windows 11 y Edge).

La dirección de la API está en `Program.cs` (`UrlApi`): por defecto la API publicada (`http://ruta-api.runasp.net/`); para la local usa
`http://localhost:5023/` con `MiApi` corriendo.

## Estructura de `RutaApp.Blazor`

| Carpeta | Contenido |
|---|---|
| `Components/Layout` | `MainLayout` (páginas públicas) y `ShellLayout` (sidebar + pantallas de la app) |
| `Components/Pages` | Landing, Sobre nosotros, Acceso/Registro, Planificar, Comparar, Rutas, Comunidad, Perfil, Configuración y detalle de ajustes |
| `Components/Shared` | Mapa (`Mapa.razor` + `MapaLeaflet`, que controla Leaflet desde C#), carrusel, intro, iconos, campos, stepper, diálogo |
| `Components/Ajustes` | Bloques de diseño de las pantallas de Configuración |
| `Services` | Estado de la sesión, API, planificador, buscador de lugares, recorridos (OSRM/Overpass), ubicación |

## Notas

- **Sin JavaScript propio:** el único JavaScript es la librería Leaflet, que se controla desde C#. La ubicación usa la API de Windows y, si no está disponible, una ubicación aproximada por IP.
- El estado vive mientras la app está abierta; al cerrarla se pierde.
- El mapa y la fuente (Google Fonts) necesitan conexión a internet.
