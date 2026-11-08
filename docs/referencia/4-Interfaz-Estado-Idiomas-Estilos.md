# Referencia de variables y funciones: RutaApp.Interfaz (estado, idiomas y estilos)

Parte de la [guía del código](../EXPLICACION_CODIGO.md). Las pantallas y componentes están en [5-Interfaz-Pantallas.md](5-Interfaz-Pantallas.md).

**Cómo leer los nombres:** `_nombre` = campo privado; `Nombre` = propiedad pública; `partial class` = una misma clase repartida en varios archivos (`EstadoApp` está en 6 archivos).

---

# Servicios/EstadoApp (el estado compartido)

`EstadoApp` es el "cerebro" de la sesión: guarda quién inició sesión, las rutas cargadas, la búsqueda en curso, los viajes y lugares guardados y las publicaciones. Las pantallas lo reciben con `@inject EstadoApp Estado`. Cuando algo cambia llama a `Notificar()` y todas las pantallas suscritas se redibujan.

## EstadoApp.cs (base)

| Miembro | Qué hace |
|---|---|
| `_nav`, `_repositorio`, `_trazos`, `_favoritos`, `_datos`, `_comunidad`, `_ajustes`, `_usuarios`, `_ubicacion` | Los servicios que `EstadoApp` usa (navegación, rutas, OSRM, favoritos, datos locales, comunidad, ajustes, usuarios, ubicación). Los entrega el constructor. |
| `Cambio` (evento) | Las pantallas se suscriben (`Estado.Cambio += ...`) para redibujarse. |
| `Notificar()` | Dispara `Cambio`. |
| `BarraLateralCambio` (evento) y `NotificarBarraLateral()` | Aviso especial al contraer o expandir el sidebar: los mapas cambian de ancho y recalculan su tamaño. |
| `Ajustes` | Acceso a `AjustesService` (unidades, idioma, accesibilidad). |
| `Aviso` | Texto del aviso breve actual (toast) o `null`. |
| `_versionAviso` | Contador para que un aviso nuevo no sea borrado por el temporizador del anterior. |
| `MostrarAviso(texto)` | Muestra el aviso 2.6 s y luego lo quita (solo si no llegó otro mientras tanto). |
| `Cargando`, `MensajeCarga` | Si la pantalla de carga está visible y qué mensaje muestra. |
| `MostrarCarga(mensaje)` / `OcultarCarga()` | Encienden y apagan la pantalla de carga. |

## EstadoApp.Sesion.cs

| Miembro | Qué hace |
|---|---|
| `HaIngresado` | `true` si ya entró (con cuenta o como invitado). |
| `Nombre`, `Correo` | Datos que muestra la interfaz (para invitado: "Invitado" / "Sin cuenta"). |
| `Invitado` | `true` si entró sin cuenta. |
| `Usuario` | El `Usuario` de la base (`null` si es invitado). |
| `ConCuenta` | `true` si hay una cuenta real con sesión (los invitados no guardan nada). |
| `DestinoTrasIngreso` | Pantalla a la que se va al entrar (por defecto `planificar`). |
| `IniciarSesion(usuario)` | Guarda los datos del usuario y marca `HaIngresado`. |
| `EntrarComoInvitado()` | Entra sin cuenta. |
| `IntroSolicitada` (evento), `IntroVista` | Controlan si la animación de entrada ya se vio o debe repetirse. |
| `IrALanding(conAnimacion)` | Navega a `/`; con animación vuelve a reproducir la intro. |
| `CompletarIngresoAsync()` | Tras entrar: prepara la cuenta (carga rutas, favoritos y datos del usuario) y navega a `DestinoTrasIngreso`. |
| `PrepararCuentaAsync(mensaje)` | Muestra la pantalla de carga mientras llama a `CargarRutasRealesAsync`, `CargarFavoritosAsync` y `CargarDatosUsuarioAsync`. |
| `_restauracionIntentada` | Evita intentar restaurar la sesión más de una vez. |
| `RecordarSesion(recordar)` | Guarda (o quita) en los ajustes el id del usuario para "Recordarme". |
| `RestaurarSesionAsync()` | Al abrir la app, si hay id recordado, busca al usuario y vuelve a abrir su sesión; si falla la conexión, no hace nada. |
| `CargarFavoritosAsync()` | Marca como `Guardada` las rutas favoritas del usuario. |
| `ConfirmandoCierre` | `true` mientras se muestra "¿Cerrar sesión?". |
| `PedirCierreSesion()` / `CancelarCierreSesion()` / `ConfirmarCierreSesion()` | Abren, cancelan y confirman el cierre. Un invitado, en lugar de cerrar, va a iniciar sesión. |
| `BorrarCuentaAsync()` | Borra la cuenta en MySQL y los datos locales; devuelve un texto de error o `null` si salió bien. |
| `CerrarSesion()` | Limpia todo (sesión, rutas guardadas, historial, trayectos, lugares, búsqueda actual). |
| `AvisoSesion` | Mensaje "inicia sesión para..." que muestra el diseño de la app. |
| `RequiereCuenta(motivo)` | Devuelve `true` si hay cuenta; si no, activa `AvisoSesion` con el motivo y devuelve `false`. Se usa antes de guardar algo. |
| `CerrarAvisoSesion()` / `IrAIniciarSesion()` | Cierran el aviso / llevan a `/acceso`. |

## EstadoApp.Navegacion.cs

| Miembro | Qué hace |
|---|---|
| `ParametroNavegacion` | Objeto que se pasa de una pantalla a otra (por ejemplo una `Ruta`); se consume una sola vez. |
| `Navegar(clave, parametro)` | Convierte la clave (`planificar`, `comparar`, `rutas`, `comunidad`, `perfil`, `config`, `ajuste`) en una dirección y navega. |
| `TomarParametro<T>()` | Devuelve el parámetro si es del tipo pedido y lo borra. |
| `MiUbicacion` | Constante `"Mi ubicación actual"`. |
| `Origen`, `Destino` | Textos de la búsqueda actual. |
| `OrigenLat`, `OrigenLng`, `DestinoLat`, `DestinoLng` | Coordenadas de la búsqueda (`NaN` = desconocidas). |
| `Escalas` | Paradas intermedias de la búsqueda. |
| `HayCoordenadas` | `true` si origen y destino tienen coordenadas. |
| `NuevaBusqueda()` | Olvida destino, escalas y coordenadas. |
| `MiPosicion` | Última ubicación del equipo (`[lat, lng]`) o `null`. |

## EstadoApp.Rutas.cs

| Miembro | Qué hace |
|---|---|
| `BuscarParadas(texto, max)` | Busca paradas por nombre en todas las rutas (mínimo 3 letras), sin repetir la misma parada de varias rutas. |
| `ParadaQueCoincide(ruta, texto)` | Nombre de la primera parada de la ruta que coincide con el texto. |
| `Rutas` | Catálogo de rutas reales cargado de la base. |
| `Publicaciones`, `Historial`, `Trayectos`, `Lugares` | Listas de publicaciones de la comunidad, viajes hechos, viajes guardados y lugares guardados. |
| `RutaSeleccionada` | Ruta que se ve en el mapa de la pantalla Rutas (se conserva al cambiar de pantalla). |
| `_rutasReales` | `true` cuando ya se cargaron las rutas (no se vuelven a pedir). |
| `ErrorRutas`, `ErrorBaseDatos` | Si falló la carga y el motivo explicado en español. |
| `CargarRutasRealesAsync()` | Pide las rutas a `RutaRepositorio`; si falla, llama a `Connection.ProbarAsync` para explicar por qué. |
| `ReintentarRutasAsync()` | Repite la carga con pantalla de carga. |
| `TrazoOsrmAsync(coordenadas)` | Delega en `ServicioTrazos`. |
| `TotalGuardadas` | Cuántas rutas están guardadas. |
| `BuscarRuta(numero)` | Ruta por número. |
| `AlternarGuardada(ruta)` | Guarda o quita la ruta de favoritas (pide cuenta), avisa y guarda en MySQL en segundo plano. |
| `GuardarFavoritoAsync(ruta, guardar)` | Escribe en MySQL; si falla, deshace el cambio. |

## EstadoApp.Viajes.cs

| Miembro | Qué hace |
|---|---|
| `CargarDatosUsuarioAsync()` | Lee el JSON del usuario (historial, viajes, lugares), quita viajes repetidos y sincroniza las marcas de guardado. |
| `ClaveTrayecto(t)` | Texto `origen>destino|líneas` en minúsculas; dos viajes con la misma clave son el mismo. |
| `GuardarSinRepetir(t, out yaEstaba)` | Agrega el viaje o devuelve el que ya estaba. |
| `QuitarTrayectosRepetidos()` | Limpia repetidos viejos y reasigna los `GuardadoId` del historial. |
| `GuardarDatos()` | Guarda historial, trayectos y lugares en el archivo del usuario (sin esperar). |
| `AlternarGuardadoHistorial(v)` | Guarda un viaje del historial en el perfil o lo quita. |
| `_viajesGuardados` | Opciones de Comparar que están marcadas como guardadas. |
| `AlternarFavoritoViaje(r)` | Estrella de Comparar: una ruta normal se guarda como favorita; una opción de viaje, como viaje personalizado. |
| `TrayectoDeOpcion(r)` | Convierte una opción de Comparar en `Trayecto` (para guardar o compartir). |
| `QuitarTrayecto(t)` | Quita un viaje guardado. |
| `SincronizarGuardados()` | Quita la marca de guardado del historial y de Comparar si su viaje ya no está en el perfil. |
| `CopiarTramos(tramos)` | Copia profunda de los tramos (para no modificar el original). |
| `RutaDeViaje(...)` (3 versiones) | Arma una `Ruta` de tipo viaje desde tramos, un `ViajeHistorial` o un `Trayecto`, lista para dibujarse en el mapa. |
| `RegistrarViaje(ruta)` | Anota el viaje en el historial (máximo 30, el más nuevo primero) y, si salió de "Mi ubicación actual", busca la calle en segundo plano. Solo con cuenta. |
| `BorrarHistorial()` | Vacía el historial. |
| `CompletarCalleAsync(v, lat, lng)` | Pone el nombre de la calle en el origen del viaje. |
| `AgregarLugar(nombre, tipo, direccion, lat, lng)` | Guarda un lugar (pide cuenta). |
| `QuitarLugar(l)` | Elimina un lugar. |
| `ImportarCodigo(texto)` | Lee un código `RUTA1:` y agrega el lugar, viaje o ruta al perfil; devuelve el mensaje a mostrar. |
| `BuscarLugaresAsync(texto)`, `CalleDeAsync(lat, lng)` | Delegan en `ServicioTrazos`. |
| `Iniciales(nombre)` | Iniciales del nombre para el avatar ("Ana Pérez" → "AP"). |

## EstadoApp.Comunidad.cs

| Miembro | Qué hace |
|---|---|
| `_publicacionesCargadas`, `ErrorPublicaciones` | Si ya se cargaron y si falló. |
| `CargarPublicacionesAsync(forzar)` | Lee las publicaciones de MySQL, las convierte y omite las caducadas. |
| `RegexVence`, `RegexUbicacion` | Expresiones regulares que leen `[[vence:...]]` y `[[ubic:lat,lng]]` al final del texto. Esos datos no tienen columna propia: viajan dentro del texto de la publicación. |
| `AgregarCaducidad(cuerpo, dias)` | Agrega al texto la fecha de caducidad. |
| `ProblemaComun` (record) y `ProblemasComunes` | Lista de problemas para reportar (tráfico, accidente, ponchadura, unidad llena, obras...) con clave, icono, texto, días de vigencia y si es de movilidad (lleva ubicación). |
| `DiasDeVida(tipo, titulo)` | Cuánto dura una publicación: aviso según su problema, pregunta 7 días, propuesta 30. |
| `UbicacionDeReporteAsync()` | Ubicación actual para marcar un reporte en el mapa. |
| `AgregarUbicacion(cuerpo, ubicacion)` | Agrega `[[ubic:...]]` al texto. |
| `PublicarAvisoAsync(titulo, detalle, ruta, dias, ubicacion)` | Crea el aviso en MySQL (con caducidad y ubicación) y lo agrega a la lista. Devuelve `(Ok, Error)`. |
| `AgregarPublicacion(p)` | Agrega una publicación nueva sin recargar todo. |
| `ConvertirPublicacion(d, orden)` | Convierte `PostDb` en `Publicacion` (separa caducidad y ubicación del texto, agrega la etiqueta de la ruta). |
| `ReportesEnMapa()` | Avisos con ubicación que aún no caducan (los marcadores del mapa). |
| `AvisosDe(ruta, dias)` | Avisos recientes sobre una ruta (alertas en el mapa). |

## Archivos pequeños de `Servicios/`
| Archivo | Contenido |
|---|---|
| **RegistroServicios.cs** | `AgregarServiciosRuta`: llama a `AgregarNucleo` y registra `Traductor` (singleton) y `EstadoApp` (*scoped*). |
| **IPortapapeles.cs** | Contrato `CopiarAsync(texto)` → `true` si se copió. Lo implementa `PortapapelesWindows`. |
| **SolicitudLugar.cs** | `Nombre` y `Tipo` del lugar que Perfil le pide a Planificar que ubique. |
| **CategoriaAjustes.cs** | `Titulo` y lista de `Ajustes` de un grupo de Configuración. |
| **Teclado.cs** | `EsActivacion(e)`: `true` si la tecla es Enter o Espacio (para elementos que no son botones). |

---

# Idiomas/

| Archivo | Contenido |
|---|---|
| **Traductor.cs** | `_ajustes`: acceso al idioma elegido. `EsIngles`: `true` si el idioma es inglés. Indexador `T["texto"]`: devuelve la traducción si el idioma es inglés y existe en `TextosIngles`; si no, el mismo texto en español. `F("Hola, {0}", valor)`: traduce un texto con huecos y los rellena con `string.Format`. |
| **TextosIngles.cs** | `Textos`: diccionario español → inglés. `Reunir(...)` une los tres grupos de traducciones (si una clave se repite, gana la primera). |
| **InglesPantallas.cs**, **InglesAjustes.cs**, **InglesMensajes.cs** | Cada uno tiene `Pares`: lista de `(Español, Ingles)` agrupada con comentarios (Landing, Acceso, Planificar, Cuenta, Soporte, Legal, avisos...). Son solo datos. |

---

# Estilos/ (CSS dentro de C#)

Cada archivo tiene una constante `Reglas` (texto CSS entre `"""`). Excepciones: `Tokens.Raiz()` (función que crea el bloque `:root` con las variables) y `Hoja.Completa`.

| Archivo | Miembro y contenido |
|---|---|
| **Tokens.cs** | `Variables`: lista de pares (nombre, valor) de las variables CSS (colores de marca, bordes, fondos, estados, sombras, escala de azules, acentos por sección). `Raiz()` las convierte en `:root { --nombre: valor; }`. |
| **Hoja.cs** | `Completa`: une `Tokens.Raiz()` y todas las `Reglas` en un orden fijo (importa por la cascada del CSS). |
| **CssBase.cs** | Reinicio, tipografía (Source Sans 3, 14 px), scrollbar delgado y contenedor `.ruta-app`. |
| **CssBotones.cs**, **CssCampos.cs**, **CssModales.cs**, **CssMarca.cs** | Botones, campos de texto, diálogos, logotipo. |
| **CssCarrusel.cs**, **CssPaginasPublicas.cs** | Carrusel e intro; landing, acceso y registro. |
| **CssShell.cs** | Sidebar y las pantallas Planificar, Comparar y Rutas (pantalla dividida panel + mapa). |
| **CssMapa.cs** | Mapa, marcadores y tinte azulado. |
| **CssPaginasConEncabezado.cs**, **CssAjustes.cs** | Comunidad, Perfil, Configuración y bloques de ajustes. |
| **CssErroresBlazor.cs** | Validación de formularios y la barra de error de Blazor. |
| **CssTonosAzul.cs**, **CssMejoras.cs** | Degradados y sombras azuladas; acentos de color, carga, barra de ubicación, viaje en curso, perfil. |
| **CssResponsivo.cs** | Adaptación a distintos anchos. |
| **CssAnimaciones.cs** | Animaciones, pantallas bajas y toasts. |
| **CssAccesibilidad.cs** | Foco visible, daltonismo, sin animaciones, guía visual. Se activa con las clases `a11y-dalton`, `a11y-guia` y `a11y-sin-anim` que pone `Routes.razor`. |
