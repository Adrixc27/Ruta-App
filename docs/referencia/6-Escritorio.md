# Referencia de variables y funciones: RutaApp.Escritorio (ventana de Windows)

Parte de la [guía del código](../EXPLICACION_CODIGO.md).

## Program.cs: clase estática `Program`
| Miembro | Qué hace |
|---|---|
| `Main()` | Punto de entrada (`[STAThread]`: el hilo de la ventana de WPF debe ser de un solo hilo). Paso a paso: (1) crea la carpeta `%LocalAppData%\RutaApp\WebView2` y la fija con la variable de entorno `WEBVIEW2_USER_DATA_FOLDER` (el navegador interno necesita una carpeta con permiso de escritura; la del `.exe` puede fallar). (2) Crea el contenedor de servicios `ServiceCollection`. (3) Registra: `AddWpfBlazorWebView` (el navegador interno), el registro de depuración, `AgregarServiciosRuta` (todo el Núcleo, Datos e Interfaz), `UbicacionWindows` como `IServicioUbicacion` (reemplaza a la versión por IP) y `PortapapelesWindows` como `IPortapapeles`. (4) En modo `DEBUG` agrega las herramientas de desarrollo de Blazor. (5) Construye el proveedor de servicios y abre `MainWindow`; la app se cierra al cerrar la ventana principal (`ShutdownMode.OnMainWindowClose`). |
| `servicios`, `proveedor`, `app`, `datos` | Variables locales de `Main`: el registro de servicios, el contenedor ya construido, la aplicación WPF y la ruta de la carpeta de WebView2. |

## MainWindow.cs: clase `MainWindow : Window`
| Miembro | Qué hace |
|---|---|
| constructor `MainWindow(servicios)` | Pone el título "R.U.T.A."; calcula el tamaño: máximo 1440×900 pero nunca más del 96 % del área de trabajo (`SystemParameters.WorkArea`), con mínimos de 380×560, y se centra. Crea un `BlazorWebView` con `HostPage = wwwroot\index.html` y los servicios, y le agrega dos componentes raíz: `Routes` en el elemento `#app` (toda la interfaz) y `HeadOutlet` en `head::after` (para los títulos de página). |
| `area`, `web` | Variables locales: área de trabajo de la pantalla y el navegador interno. |

## UbicacionWindows.cs: clase `UbicacionWindows : IServicioUbicacion`
| Miembro | Qué hace |
|---|---|
| `_porIp` | El servicio de ubicación por IP (respaldo), recibido por el constructor. |
| `ObtenerAsync()` | Pide permiso con `Geolocator.RequestAccessAsync`. Si lo dan, obtiene la posición (límite de 8 s, acepta una posición de hasta 5 minutos de antigüedad) y devuelve `[latitud, longitud]`. Si no hay permiso, el servicio está apagado o hay error, devuelve la ubicación aproximada por IP. |

## PortapapelesWindows.cs: clase `PortapapelesWindows : IPortapapeles`
| Miembro | Qué hace |
|---|---|
| `CopiarAsync(texto)` | Copia el texto al portapapeles de Windows en el hilo de la ventana (`Dispatcher.InvokeAsync`). Devuelve `true` si funcionó y `false` si otro programa tiene el portapapeles abierto (`COMException`). |

## wwwroot/index.html
Página que hospeda la interfaz. No tiene funciones; contiene:
- `<div id="app">` con una pantalla de carga con la "R" que se ve mientras Blazor arranca (después `Routes` la reemplaza).
- Las hojas de estilo: fuente Source Sans 3 y `lib/leaflet/leaflet.css`.
- Los scripts: `lib/leaflet/leaflet.js` (librería de mapas), `_content/RutaApp.Interfaz/mapa.js` (nuestro adaptador) y `_framework/blazor.webview.js` (el puente entre Blazor y la ventana).
- `<html lang="es">` y un `viewport` que impide hacer zoom con gestos.
