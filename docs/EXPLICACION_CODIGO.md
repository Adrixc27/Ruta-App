# R.U.T.A. App: explicación del código

Guía de cada archivo de código de la aplicación: qué hace, con qué otros archivos se conecta y para qué sirve.
Los archivos se agrupan cuando cumplen el mismo papel (por ejemplo, los 19 archivos de estilos).

> **Variables y funciones de cada archivo:** están explicadas una por una en la carpeta [referencia](referencia/):
> [Datos](referencia/1-Datos.md) · [Núcleo](referencia/2-Nucleo.md) · [Motor y pruebas](referencia/3-Motor-y-Pruebas.md) · [Interfaz: estado, idiomas y estilos](referencia/4-Interfaz-Estado-Idiomas-Estilos.md) · [Interfaz: pantallas y componentes](referencia/5-Interfaz-Pantallas.md) · [Escritorio](referencia/6-Escritorio.md)

## 1. Visión general

R.U.T.A. es una app de escritorio de Windows para planear viajes en camión en la ciudad de Chihuahua. Se divide en 5 proyectos:

| Proyecto | Carpeta | Papel |
|---|---|---|
| **RutaApp.Escritorio** | `src/Frontend/RutaApp.Escritorio` | La ventana de Windows (WPF). Solo aloja la interfaz web y da acceso a ubicación y portapapeles de Windows. |
| **RutaApp.Interfaz** | `src/Frontend/RutaApp.Interfaz` | Todas las pantallas (Blazor): páginas, componentes, estilos, idiomas y el estado compartido. |
| **RutaApp.Nucleo** | `src/Backend/RutaApp.Nucleo` | La lógica de la app: modelos, planificador de viajes, servicios web (OSRM, Nominatim, Photon, Overpass), ajustes y datos locales. |
| **RutaApp.Datos** | `src/Database/RutaApp.Datos` | El acceso a MySQL (Dapper): usuarios, rutas, comunidad, favoritos, opiniones. |
| **RutaApp.Motor** | `src/Backend/RutaApp.Motor` | Un motor de búsqueda de rutas independiente (Dijkstra, A*, Pareto). **Hoy solo lo usan las pruebas**; la app usa el `Planificador` del Núcleo. |

Más `tests/RutaApp.Pruebas` (pruebas xUnit del Motor) y `src/Database/Scripts/esquema.sql` (script para crear la base de datos).

### Cómo se conectan

```
RutaApp.Escritorio  ──►  RutaApp.Interfaz  ──►  RutaApp.Nucleo  ──►  RutaApp.Datos  ──►  MySQL
   (ventana WPF)          (pantallas Blazor)     (lógica + web)       (consultas SQL)

RutaApp.Pruebas  ──►  RutaApp.Motor   (separado: el resto de la app no lo referencia)
```

Cada proyecto solo conoce al de su derecha. Así la interfaz nunca toca SQL y la base de datos no sabe nada de pantallas.

### Qué pasa al abrir la app

1. `Program.Main` (Escritorio) arma el contenedor de servicios y abre `MainWindow`.
2. `MainWindow` crea un `BlazorWebView` que carga `wwwroot/index.html` y monta el componente `Routes`.
3. `Routes` pinta la hoja de estilos, el enrutador, la pantalla de carga y los avisos globales.
4. La ruta `/` muestra `Landing` (animación de entrada); desde ahí se llega a `Auth` (login/registro) o se entra como invitado.
5. Al entrar, `EstadoApp.CompletarIngresoAsync` carga las rutas desde MySQL y los datos del usuario, y abre `Planificar`.

---

## 2. RutaApp.Escritorio (la ventana de Windows)

| Archivo | Qué hace | Se conecta con |
|---|---|---|
| **Program.cs** | Punto de entrada. Fija la carpeta de datos de WebView2, registra todos los servicios (`AgregarServiciosRuta`, ubicación de Windows, portapapeles de Windows) y arranca la aplicación WPF con `MainWindow`. | Usa `RegistroServicios` (Interfaz), `UbicacionWindows`, `PortapapelesWindows`, `MainWindow`. |
| **MainWindow.cs** | Ventana principal sin XAML. Calcula un tamaño que cabe en la pantalla (laptops 1366x768 incluidas) y contiene un `BlazorWebView` que carga `index.html` y monta `Routes` (Interfaz). | `Program.cs`, `Routes.razor`. |
| **UbicacionWindows.cs** | Implementa `IServicioUbicacion` con el servicio de ubicación de Windows (`Geolocator`). Si está apagado o sin permiso, usa `ServicioUbicacionIp` como respaldo. | Reemplaza a `ServicioUbicacionIp` en el contenedor; lo usan `Mapa.razor`, `Perfil.razor` y `EstadoApp`. |
| **PortapapelesWindows.cs** | Implementa `IPortapapeles`: copia texto al portapapeles de Windows y devuelve `false` si otro programa lo tiene abierto. | Lo usa `CompartirDialogo.razor`. |
| **wwwroot/index.html** | Página HTML que hospeda la app: carga la fuente Source Sans 3, Leaflet, `mapa.js` (desde `_content/RutaApp.Interfaz/`) y `blazor.webview.js`, y muestra una pantalla "R" mientras Blazor arranca. | Contiene el `#app` donde se monta `Routes`. |
| **wwwroot/lib/leaflet/** | La librería Leaflet (mapas) de terceros, sin modificar. | La usa `mapa.js`. |
| **wwwroot/img/carrusel/** | Fotos de las ciudades del carrusel de acceso. | Las muestra `Carrusel.razor`. |

---

## 3. RutaApp.Nucleo (la lógica de la app)

### 3.1 Modelos (`Modelos/`)

Son clases de datos sencillas que viajan entre servicios y pantallas.

| Archivo | Qué representa |
|---|---|
| **Ruta.cs** | Una ruta de camión **o** una opción de viaje (camina → camión → camina). Guarda número, nombre, tarifa, minutos, frecuencia, recorrido (`Trazo`), tramos, variantes y textos ya formateados para la interfaz (`TarifaTexto`, `Meta`...). Es el modelo central: lo producen `RutaRepositorio` y `Planificador`, y lo muestran `Rutas`, `Comparar`, `Planificar` y `Mapa`. |
| **Tramo.cs** | Un tramo de un viaje: a pie (punteado) o en camión (línea de color), con puntos, paradas de subida/bajada, minutos, instrucciones para caminar y símbolo/patrón de la ruta. |
| **Trayecto.cs** | Un viaje que el usuario guardó (Perfil → Rutas guardadas → Personalizadas). Conserva los tramos para volver a verlo en el mapa y compartirlo. |
| **ViajeHistorial.cs** | Un viaje que el usuario ya hizo. Se agrega al pulsar "Comenzar viaje" y se puede volver a dibujar. |
| **LugarGuardado.cs** | Casa, escuela, trabajo u otro punto de referencia del usuario, con su icono y texto según el tipo. |
| **Publicacion.cs** | Una publicación de la comunidad (aviso, pregunta o propuesta) con autor, likes, caducidad y, si es un reporte de movilidad, la ubicación. Incluye la clase `Etiqueta`. |
| **Sugerencia.cs** | Un lugar encontrado por el buscador o una parada intermedia del viaje. |
| **ParadaBuscada.cs** | Una parada encontrada al buscar por nombre de parada (con su ruta). |
| **PasoViaje.cs** | Un paso de la navegación paso a paso: caminar o tomar un camión. |
| **Paleta.cs** | Colores, símbolos y trazos (guiones) de las rutas. Cada ruta se identifica también por símbolo y patrón para quien no distingue colores. |
| **Formato.cs** | Formatea distancias (km/mi), textos de "hace 5 min" (español/inglés), nombres de parada y comparaciones sin acentos. Lee los ajustes de unidades e idioma. |
| **Geo.cs** | Constantes del centro de Chihuahua y una función que proyecta un punto sobre una línea (para dibujar paradas sobre el recorrido). |

### 3.2 Servicios (`Servicios/`)

| Archivo | Qué hace | Se conecta con |
|---|---|---|
| **Planificador.cs** | **El cerebro de los viajes.** Recibe las rutas reales y el origen/destino, y arma opciones de viaje: camina a una parada, viaja por una ruta, baja y transborda. Construye un grafo con cada parada de cada ruta, busca el camino más barato con Dijkstra (penalizando caminar y transbordar), repite prohibiendo rutas para generar variantes, y etiqueta resultados (Recomendado, Más rápido, Económico, Menos caminata). Solo usa rutas reales. | Entra `List<Ruta>` (de `RutaRepositorio`); devuelve `Ruta` con `Tramo`. Lo llama `Comparar.razor`. |
| **RutaRepositorio.cs** | Convierte las filas de la base de datos en objetos `Ruta`. Une variantes del mismo grupo (por ejemplo "Komatsu Directo/Inverso") en una sola ruta. | Usa `RutaService` (Datos). Lo llama `EstadoApp.CargarRutasRealesAsync`. |
| **ServicioTrazos.cs** | Habla con servicios de OpenStreetMap: **OSRM** (recorridos por calles y a pie con instrucciones), **Nominatim** (nombre de calle), **Photon** (búsqueda de lugares) y **Overpass** (paradas y puntos de referencia). Guarda en caché lo ya pedido. También ajusta a las calles las rutas dibujadas con rectas y repara tramos que cortan esquinas. | Usa el cliente HTTP "externo" y `BuscadorLugares`. Lo usan `Mapa`, `Perfil`, `Comparar` y `EstadoApp`. |
| **BuscadorLugares.cs** | Busca lugares solo dentro de Chihuahua combinando Photon (mientras escribes) y Nominatim (direcciones). Expande abreviaturas ("av", "blvd", "uach"), quita duplicados y ordena por cercanía al centro. | Lo llama `ServicioTrazos.BuscarLugaresAsync`. |
| **CodigoCompartir.cs** | Crea y lee códigos de texto `RUTA1:...` para compartir lugares, viajes o rutas. Simplifica el recorrido (pocos puntos) para que el código no sea larguísimo. | Lo usan `CompartirDialogo`, `Planificar`, `Perfil` y `EstadoApp.ImportarCodigo`. |
| **DatosUsuario.cs** | Guarda por usuario, en un JSON local (`LocalAppData\RutaApp\usuarios\{id}.json`), el historial, los viajes guardados y los lugares. No toca la base de datos. Escribe a un archivo temporal y lo reemplaza para no dejar archivos rotos. | Lo usa `EstadoApp`. |
| **AjustesService.cs** | Guarda las preferencias del equipo (unidades, idioma, daltonismo, guía visual, sin animaciones y sesión recordada) en `ajustes.json`. Avisa con el evento `Cambio` para que la interfaz se redibuje. | Lo usan `Traductor`, `Routes`, `EstadoApp` y las pantallas de Ajustes. |
| **ServicioUbicacion.cs** | Define `IServicioUbicacion` y su versión por IP (`ServicioUbicacionIp`, servicio ipwho.is), que sirve de respaldo. | La reemplaza `UbicacionWindows` (Escritorio) en la app real. |

### 3.3 RegistroNucleo.cs

Registra en el contenedor todos los servicios del Núcleo, llama a `AgregarDatos()` y configura el cliente HTTP "externo" con un tiempo máximo de 8 segundos y un User-Agent (OSRM, Nominatim y Overpass lo piden).

---

## 4. RutaApp.Datos (MySQL)

| Archivo | Qué hace | Tablas |
|---|---|---|
| **Connection.cs** | Resuelve la conexión a MySQL: variable de entorno `RUTA_DB_CONNECTION` o, si no existe, `%LocalAppData%\RutaApp\conexion.json` (se crea sola). Fuerza cifrado si el servidor no es local y ofrece `ProbarAsync`, que explica en español qué falla. | — |
| **RutaService.cs** | Lee las rutas oficiales con sus paradas y trazo. Guarda el recorrido por calles (codificado como polyline) y lo cachea. | `routes`, `bus`, `stop`, `route_stop` |
| **UsuarioService.cs** | Registro e inicio de sesión (contraseña con hash BCrypt), actualizar nombre y foto, y **borrar la cuenta** limpiando todas las tablas que dependen del usuario. Incluye `Sesion.Actual`. | `user` y las tablas dependientes |
| **ComunidadService.cs** | Publicaciones, comentarios, "me gusta" y eliminación de la comunidad. | `post`, `comment`, `post_like`, `photo` |
| **FavoritoService.cs** | Rutas favoritas (guardadas) de cada usuario. | `favorite_route` |
| **PreferenciaService.cs** | Preferencias de búsqueda del usuario (costo, tiempo, transbordos). Está registrado en el contenedor, pero **hoy ninguna pantalla lo usa**. | `preference` |
| **OpinionService.cs** | Guarda los comentarios para mejorar la app (Configuración → Soporte). Crea la tabla si no existe. | `app_feedback` |
| **Modelos/RutaOficial.cs** | Ruta tal como sale de la base de datos y el `record Parada`. Lo convierte a `Ruta` el `RutaRepositorio`. | — |
| **RegistroDatos.cs** | Registra los 6 servicios de datos como singleton. | — |
| **Scripts/esquema.sql** | Crea la base `rutadb` vacía con todas las tablas. Se puede correr varias veces. | Las 16 tablas |

---

## 5. RutaApp.Interfaz (las pantallas)

### 5.1 Estado y servicios (`Servicios/`)

`EstadoApp` es el estado compartido de toda la sesión (servicio *scoped*). Se divide en 5 archivos parciales por tema. Los componentes se suscriben al evento `Cambio` para redibujarse.

| Archivo | Qué contiene |
|---|---|
| **EstadoApp.cs** | Constructor (recibe los servicios del Núcleo y Datos), eventos `Cambio` y `BarraLateralCambio`, avisos breves (toast) y pantalla de carga. |
| **EstadoApp.Sesion.cs** | Inicio de sesión, invitado, "Recordarme", restaurar sesión al abrir, cierre con confirmación, borrar cuenta y el aviso "inicia sesión para guardar". |
| **EstadoApp.Navegacion.cs** | `Navegar(clave, parametro)` entre pantallas y la búsqueda actual (origen, destino, escalas y coordenadas) que pasa de Planificar a Comparar. |
| **EstadoApp.Rutas.cs** | Catálogo de rutas, carga desde la base, errores de conexión, búsqueda por paradas, rutas guardadas y recorridos OSRM. |
| **EstadoApp.Viajes.cs** | Viajes guardados, historial, lugares guardados, conversión a `Ruta` para el mapa e importación de códigos compartidos. |
| **EstadoApp.Comunidad.cs** | Publicaciones, avisos, caducidad, problemas comunes de movilidad y reportes con ubicación. |
| **RegistroServicios.cs** | Registra `Traductor`, `EstadoApp` y llama a `AgregarNucleo`. Lo invoca `Program.cs`. |
| **IPortapapeles.cs** | Contrato para copiar texto; lo implementa el Escritorio. |
| **SolicitudLugar.cs** | Petición de Perfil a Planificar para elegir un lugar tocando el mapa. |
| **CategoriaAjustes.cs** | Un grupo de la lista de Configuración (título + ajustes). |
| **Teclado.cs** | Ayuda para que Enter y Espacio activen elementos que no son botones (accesibilidad). |

### 5.2 Idiomas (`Idiomas/`)

La interfaz está escrita en español; el inglés es una traducción por diccionario.

| Archivo | Qué hace |
|---|---|
| **Traductor.cs** | Traduce un texto: si el idioma es inglés busca la clave en `TextosIngles`; si no hay traducción, muestra el español. En los componentes se usa como `T["texto"]`. |
| **TextosIngles.cs** | Une los tres diccionarios en uno (si una clave se repite, gana la primera). |
| **InglesPantallas.cs** | Traducciones de landing, acceso, planificar, comparar, rutas, comunidad y perfil. |
| **InglesAjustes.cs** | Traducciones de Configuración, soporte, ayuda y textos legales. |
| **InglesMensajes.cs** | Traducciones de avisos, mensajes de sesión, errores de formularios y migas. |

### 5.3 Estilos (`Estilos/`, 19 archivos)

El CSS vive en C# como texto. **Hoja.cs** une todos los archivos en `Hoja.Completa` en un orden que es el de la cascada CSS (no conviene cambiarlo). El componente `HojaEstilos.razor` la inserta una vez en una etiqueta `<style>`.

| Archivo(s) | Contenido |
|---|---|
| **Tokens.cs** | Variables CSS (`--nombre`): colores, degradados, sombras y acentos por sección. |
| **CssBase.cs** | Reinicio de estilos, tipografía, scrollbar y contenedor `.ruta-app`. |
| **CssBotones.cs / CssCampos.cs / CssModales.cs** | Botones, campos de texto, diálogos y ventanas modales. |
| **CssMarca.cs / CssCarrusel.cs** | Logotipo y marca; carrusel y animación de entrada. |
| **CssPaginasPublicas.cs** | Landing, Sobre nosotros, acceso y registro. |
| **CssShell.cs** | Sidebar y pantallas Planificar, Comparar y Rutas. |
| **CssMapa.cs** | Mapa Leaflet, marcadores y su tinte azul. |
| **CssPaginasConEncabezado.cs** | Comunidad, Perfil y Configuración. |
| **CssAjustes.cs** | Bloques de las pantallas de ajustes. |
| **CssErroresBlazor.cs** | Validación de formularios y barra de error de Blazor. |
| **CssTonosAzul.cs** | Degradados, capas y sombras azuladas de toda la app. |
| **CssMejoras.cs** | Acentos de color, pantalla de carga, barra de ubicación, viaje en curso, perfil y lugares. |
| **CssResponsivo.cs** | Adaptación a distintos anchos (el menú pasa a barra inferior en pantallas chicas). |
| **CssAnimaciones.cs** | Animaciones, pantallas bajas y toasts. |
| **CssAccesibilidad.cs** | Foco visible, modo daltonismo, sin animaciones y guía visual. Se activa con clases que pone `Routes.razor`. |

### 5.4 Estructura de la app (`Components/Routes.razor`, `_Imports.razor`, `Layout/`)

| Archivo | Qué hace |
|---|---|
| **Routes.razor** | Raíz de la interfaz: pone la hoja de estilos, las clases de accesibilidad (daltonismo, guía visual, sin animaciones), el enrutador, la pantalla de carga y los avisos globales. |
| **_Imports.razor** | Usings comunes e inyecta `Nav` y `T` (traductor) en todos los componentes. |
| **Layout/MainLayout.razor** | Diseño de las páginas públicas (landing, sobre nosotros, acceso, not-found). |
| **Layout/ShellLayout.razor** | Diseño de la app con sesión o invitado: sidebar que se contrae (barra inferior en pantallas chicas), barra "dónde estás", aviso de iniciar sesión y atajos de teclado (Alt+1…5, Alt+0). |

### 5.5 Páginas (`Components/Pages/`)

| Página | Ruta | Qué hace | Se conecta con |
|---|---|---|---|
| **Landing.razor** | `/` | Pantalla de bienvenida con la animación de entrada. Si hay "Recordarme", abre la sesión sola. Botón "Planificar viaje" para invitados. | `IntroAnimacion`, `EstadoApp.Sesion`. |
| **Auth.razor** | `/acceso`, `/registro` | Login y registro con carrusel de marca, fuerza de contraseña y "Recordarme". | `UsuarioService`, `Carrusel`, `CampoTexto`, `CampoPassword`. |
| **Planificar.razor** | `/planificar` | Origen, destino y paradas intermedias con sugerencias (450 ms tras dejar de escribir); "Mi ubicación actual"; elegir un punto en el mapa; mis lugares; historial; pegar un código compartido. | `Mapa`, `EstadoApp`, `BuscadorLugares`, `CodigoCompartir`. |
| **Comparar.razor** | `/comparar` | Calcula las opciones de viaje con el `Planificador` en otro hilo, las compara y permite compartir, guardar o comenzar el viaje (navegación paso a paso con posición en vivo cada 8 s). | `Planificador`, `Mapa`, `CompartirDialogo`. |
| **Rutas.razor** | `/rutas` | Catálogo de rutas (pestañas Todas/Guardadas, búsqueda también por parada) con el mapa de la ruta elegida. | `Mapa`, `EstadoApp.Rutas`. |
| **Comunidad.razor** | `/comunidad` | Avisos, preguntas y propuestas: filtros, "me gusta", comentarios, nueva publicación (con códigos para compartir). | `ComunidadService`, `EstadoApp.Comunidad`. |
| **Perfil.razor** | `/perfil` | Datos de la cuenta (nombre, foto), historial, viajes y rutas guardadas, lugares, importar códigos. | `UsuarioService`, `ServicioTrazos`, `IServicioUbicacion`. |
| **Configuracion.razor** | `/configuracion` | Lista de ajustes por categoría con buscador. | `AjusteDetalle`. |
| **AjusteDetalle.razor** | `/configuracion/ajuste` | Muestra el detalle de un ajuste eligiendo el componente `Ajustes*` según su nombre. | `Ajustes/*`. |
| **SobreNosotros.razor** | `/sobre-nosotros` | Página pública "Sobre nosotros". | `ContenidoSobreNosotros`. |
| **NotFound.razor** | `/not-found` | Pantalla para direcciones que no existen. | `MainLayout`. |

### 5.6 Componentes compartidos (`Components/Shared/`)

| Archivo | Qué hace |
|---|---|
| **Mapa.razor** | Mapa Leaflet con el banner, la tarjeta de la ruta/viaje, controles (centrar en mí, reportar, leyenda) y el reporte de problemas. Dibuja rutas, viajes, ubicaciones, paradas y reportes. |
| **MapaLeaflet.cs** | Controla el mapa desde C#: junta todo lo que hay que dibujar y lo manda en **una sola llamada** a `mapa.js` (más rápido). Define los `record` `Marcador`, `TramoMapa`, `ReporteMapa`, `PuntoReferencia`. |
| **wwwroot/mapa.js** (en Interfaz) | Adaptador mínimo de Leaflet: recibe la descripción del dibujo y llama a la librería (capas, marcadores, clic en el mapa de vuelta a C#). Es la única parte en JavaScript propio. |
| **Icon.razor / Iconos.cs** | `Iconos.cs` guarda los iconos vectoriales (SVG, estilo Feather); `Icon.razor` dibuja uno por su clave (`"IcoBus"`). |
| **CampoTexto.razor / CampoPassword.razor** | Cajas de texto con icono; la de contraseña con botón para mostrar u ocultar. |
| **Carrusel.razor / SlideMarca.razor** | Carrusel de pantalla completa (fundido automático, flechas, puntos, pausa); `SlideMarca` es su primera diapositiva azul de marca. |
| **IntroAnimacion.razor** | Animación de entrada: un camión cruza la pantalla revelando "RUTA". Un clic la salta. |
| **PantallaCarga.razor** | Pantalla que cubre la app mientras se cargan rutas y datos. |
| **AvisosGlobales.razor** | Avisos breves (toast) y confirmación de cerrar sesión, visibles en cualquier pantalla. |
| **Dialogo.razor** | Cuadro de mensaje (Aceptar, o Sí/No). |
| **CompartirDialogo.razor** | Compartir en 3 pasos: copiar el código, enviarlo, pegarlo en Planificar. Usa `IPortapapeles`. |
| **Ayuda.razor** | Botón "?" que explica una parte de la app; solo aparece con la guía visual activada. |
| **HojaEstilos.razor** | Inserta `Hoja.Completa` una sola vez. |

### 5.7 Componentes de Configuración (`Components/Ajustes/`)

Piezas reutilizables para las pantallas de ajustes.

| Archivo(s) | Qué hacen |
|---|---|
| **AjustesGeneral / AjustesAccesibilidad / AjustesCuenta / AjustesSoporte / AjustesLegal.razor** | El contenido de cada detalle: unidades e idioma; daltonismo, guía visual y sin animaciones; sesión, cerrar y borrar cuenta; reportar un problema y centro de ayuda; privacidad, términos y Sobre nosotros. Los ajustes se guardan al momento con `AjustesService`; Soporte usa `OpinionService`. |
| **ContenidoSobreNosotros.razor** | Texto de "Sobre nosotros", reutilizado en Configuración y en la página pública. |
| **MarcoDetalle.razor** | Marco común de los detalles: migas, título, cuerpo, barra inferior y pie. |
| **DosColumnas.razor / NavegadorDocumento.razor** | Contenido a un lado y panel fijo al otro; índice de un documento legal. |
| **Tarjeta / FilaAjuste / ParValor / BarraInferior** | Tarjeta con título; fila con icono y control; par etiqueta-valor; barra con botón de guardar y aviso "Cambios guardados". |
| **Interruptor / Deslizador / Desplegable / OpcionRadio / CasillaTexto / AreaTexto** | Controles de formulario: interruptor, deslizador, selector, opción radio, casilla y área de texto con contador. |
| **Boton / Chip / Aviso / CajaIcono** | Botón, etiqueta pequeña, recuadro de aviso (info, ok, alerta) y icono dentro de una caja. |

---

## 6. RutaApp.Motor (motor de rutas independiente)

Un motor de búsqueda de rutas completo y probado, pero **no está conectado a la app**: solo lo usa `tests/RutaApp.Pruebas`. La app real planea los viajes con `Planificador.cs` (Núcleo).

| Archivo | Qué hace |
|---|---|
| **Modelos/ModelosTransporte.cs** | Todas las clases del motor: `Coordenada`, `Parada`, `RutaAutobus`, `Conexion`, `Preferencias` (pesos, límites y validación), `Recorrido` (calcula tiempos, transbordos y confiabilidad), `RutaRecomendada` y `ResultadoBusqueda`. |
| **Grafo/GrafoTransporte.cs** | Grafo dirigido de paradas y conexiones; valida lo que se agrega y define las reglas de transbordo y accesibilidad que comparten los algoritmos. |
| **Algoritmos/AlgoritmoDijkstra.cs** | Camino más barato con un montículo binario; el estado es (parada, autobús) para penalizar transbordos. También calcula cotas inferiores. |
| **Algoritmos/BuscadorRutas.cs** | Busca varias alternativas con A* sin repetir paradas, de la más rápida a la más lenta. |
| **Algoritmos/BuscadorMulticriterio.cs** | Genera variantes (más rápida, menos transbordos, menos caminata, más confiable, equilibrada) y se queda con las no dominadas (frente de Pareto). |
| **Algoritmos/EvaluadorRutas.cs** | `EvaluadorRutas` puntúa y explica cada opción; `ClasificadorRutas` elimina duplicados y ordena. |
| **Servicios/ServicioTransporte.cs** | Punto de entrada: valida, encuentra paradas cercanas, usa los algoritmos y devuelve el `ResultadoBusqueda`. |
| **Servicios/ServicioUbicaciones.cs** | Paradas cercanas a un punto: distancia Haversine y estimación de caminata. |
| **Servicios/ServicioDatosTransporte.cs** | Valida, carga (JSON) y convierte los datos en un grafo. |
| **Datos/DatosPrueba.cs** | Red ficticia de 12 paradas y 4 rutas para probar el motor. |

---

## 7. Pruebas (`tests/RutaApp.Pruebas/PruebasMotorRutas.cs`)

52 pruebas xUnit que cubren el Motor: Dijkstra, grafo, recorridos, búsqueda de alternativas, Pareto, evaluación, duplicados, ubicaciones, validación de datos y el servicio completo. Usan `DatosPrueba` y no necesitan base de datos ni internet.

---

## 8. Ejemplo de punta a punta: planear y comenzar un viaje

1. En **Planificar** escribes un destino. `EstadoApp` llama a `ServicioTrazos` y `BuscadorLugares`, que consultan Photon y Nominatim y devuelven `Sugerencia`.
2. Al buscar, `Estado.Navegar("comparar")` abre **Comparar**.
3. `Comparar` pide las rutas ya cargadas por `RutaRepositorio` (que las leyó con `RutaService` desde MySQL) y las pasa a `Planificador.Generar`.
4. El `Planificador` devuelve varias `Ruta` con `Tramo`; `Mapa` las dibuja mandando una sola llamada a `mapa.js`.
5. Al pulsar "Comenzar viaje", `EstadoApp.RegistrarViaje` lo anota en el historial y `DatosUsuarioService` lo guarda en el JSON del usuario.
