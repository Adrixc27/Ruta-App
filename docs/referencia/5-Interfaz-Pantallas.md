# Referencia de variables y funciones: RutaApp.Interfaz (pantallas y componentes)

Parte de la [guía del código](../EXPLICACION_CODIGO.md). El estado compartido, los idiomas y los estilos están en [4-Interfaz-Estado-Idiomas-Estilos.md](4-Interfaz-Estado-Idiomas-Estilos.md).

**Cómo leer un archivo `.razor`:** arriba va el HTML (la parte visual) y abajo, dentro de `@code { ... }`, el C# (variables y funciones). Aquí solo se explica el C#.

**Palabras que se repiten en todos los archivos:**
| Palabra | Qué significa |
|---|---|
| `[Parameter] public ...` | Una **entrada** del componente: el componente padre le pasa ese valor al usarlo (`<Chip Texto="Hola" />`). |
| `EventCallback` | Una **salida**: el componente avisa al padre que pasó algo (un clic, un cambio). |
| `@inject EstadoApp Estado` | Recibe el estado compartido (ver el archivo 4). |
| `T["texto"]` | Traduce el texto según el idioma elegido. |
| `Estado.Cambio += Refrescar` | La pantalla se suscribe para redibujarse cuando el estado cambia; en `Dispose()` se da de baja (`-=`) para no dejar suscripciones colgadas. |
| `InvokeAsync(StateHasChanged)` | Pide a Blazor redibujar la pantalla (desde cualquier hilo). |
| `OnInitialized` / `OnAfterRenderAsync(firstRender)` | Se ejecutan al crear el componente / después del primer dibujo (el mapa solo existe desde ahí). |
| `_mapa`, `Mapa` | El componente de mapa de la pantalla (ver `Mapa.razor`). |

---

# Páginas (`Components/Pages/`)

## Landing.razor (ruta `/`)
| Miembro | Qué hace |
|---|---|
| `TextoBienvenida` | "¡Bienvenido a R.U.T.A.!" o, con cuenta, "¡Hola, Ana! Qué bueno verte de nuevo." |
| `_mostrarIntro` | `true` mientras se muestra la animación de entrada. |
| `_claveIntro` | Número que cambia para forzar que la animación se cree de nuevo. |
| `OnInitialized()` | Decide si hay intro (solo la primera vez) y se suscribe a `IntroSolicitada` y `Cambio`. |
| `OnInitializedAsync()` | Llama a `RestaurarSesionAsync`: si el usuario pidió "Recordarme", entra solo. |
| `VolverAVerIntro()` | Se ejecuta al tocar el logo estando en la landing: reinicia la animación. |
| `IntroTerminada()` | Marca `IntroVista` y oculta la intro. |
| `PlanificarViaje()` / `Entrar()` | Entran (como invitado si no hay sesión) y abren Planificar con `CompletarIngresoAsync`. |
| `Refrescar()`, `Dispose()` | Redibujar y darse de baja de los eventos. |

## Auth.razor (rutas `/acceso` y `/registro`)
| Miembro | Qué hace |
|---|---|
| `_registro` | `true` = muestra el formulario de registro; `false` = login. |
| `_ocupado` | Evita enviar el formulario dos veces. |
| `_terminos` | Casilla "acepto los términos". |
| `_recordar` | Casilla "Recordarme" (activada por defecto). |
| `_loginCorreo`, `_loginPass` | Campos del login. |
| `_regNombre`, `_regCorreo`, `_regPass`, `_regPass2` | Campos del registro (contraseña y repetirla). |
| `_loginError`, `_regError`, `_aviso` | Mensajes de error y aviso. |
| `Titular`, `Subtitular` | Textos del carrusel, distintos en login y registro. |
| `OnInitialized()` | Abre el registro si la dirección es `/registro`. |
| `IrRegistro()`, `IrLogin()` | Cambian de formulario y limpian errores. |
| `TeclaLogin(e)`, `TeclaRegistro(e)` | Enter envía el formulario. |
| `IniciarSesion()` | Valida correo y contraseña, llama a `UsuarioService.IniciarSesionAsync`, guarda la sesión (`Sesion.Actual`, `Estado.IniciarSesion`, `RecordarSesion`) y entra. Muestra errores claros si falla. |
| `Registrar()` | Valida nombre, correo, contraseña (≥ 6, coinciden) y términos; crea la cuenta con `RegistrarAsync` y entra. |
| `Invitado()` | Entra sin cuenta. |
| `EsCorreo(s)` | Revisa que tenga un solo `@`, algo antes y un punto después. |
| `Puntos` | Fuerza de la contraseña de 0 a 4 (8+ caracteres, mayúscula y minúscula, número, símbolo). |
| `ColorFuerza`, `TextoFuerza` | Color y texto de las 4 barras según `Puntos`. |
| `Aviso(texto)` | Muestra un aviso. |

## Planificar.razor (ruta `/planificar`)
Pantalla dividida: formulario a la izquierda y mapa a la derecha.

**Constantes y tipos**
| Miembro | Qué es |
|---|---|
| `MaxParadasIntermedias` | 3: máximo de paradas entre origen y destino. |
| `TipoLugar` y `TiposLugar` | Opciones al guardar un lugar: casa, escuela, trabajo, otro (clave, icono, texto). |
| `Escala` | Una parada intermedia: `Id` (`escala1`...) y `Texto`. |

**Variables**
| Variable | Qué guarda |
|---|---|
| `_mapa` | El componente de mapa. |
| `_origen`, `_destino` | Textos de los campos. |
| `_escalas`, `_contadorEscalas` | Paradas intermedias y contador para darles ids únicos. |
| `_error`, `_buscando` | Mensaje de error y si está buscando. |
| `_campoActivo` | Campo que se está editando (`origen`, `destino` o el id de una escala). |
| `_tipoActivo`, `_sugerencias` | De qué campo son las sugerencias abiertas y la lista. |
| `_elegido` | Lugar elegido por cada campo (`Dictionary`). |
| `_esperas` | Un temporizador por campo para esperar 450 ms tras dejar de escribir. |
| `_viajeMostrado`, `_historialSel` | Viaje que se ve en el mapa (del historial, perfil o compartido) y el elemento del historial elegido. |
| `_guardandoLugar`, `_nombreLugar`, `_tipoLugar`, `_direccionLugar`, `_errorLugar`, `_resultadosLugar`, `_esperaLugar`, `_buscandoUbicacion`, `_lugarMapa` | Estado del formulario "Guardar un lugar". |
| `_pegarCodigo`, `_viajeCompartido`, `_codigoTexto`, `_mensajeCodigo`, `_codigoAbierto` | Estado de "Pegar un código". |
| `_eligiendo` | `null`, `"lugar"` o el campo para el que se elige un punto tocando el mapa. |
| `_solicitud`, `_lugarVer` | Datos que llegan desde Perfil (elegir un lugar / ver un lugar). |
| `NombreCampoActivo` | "el origen", "el destino" o "la parada intermedia" (para los textos). |

**Funciones**
| Función | Qué hace |
|---|---|
| `OnInitialized()` | Toma los parámetros de navegación, restaura origen, destino y paradas desde `Estado` (por ejemplo al volver de Comparar con "Editar búsqueda"). |
| `OnAfterRenderAsync` | Según de dónde vino: muestra un lugar, empieza a elegir un lugar, muestra un viaje guardado o marca los puntos de la búsqueda anterior ("Mi ubicación actual" si es la primera vez). |
| `VerHistorial(v)`, `MostrarViajeAsync(r, subtitulo)` | Dibuja un viaje anterior en el mapa con banner y tarjeta. |
| `OcultarViajeMostradoAsync()` | Quita el viaje del mapa al empezar otra búsqueda. |
| `QuitarMarcasDeBusquedaAsync()` | Quita origen, destino y paradas del mapa. |
| `ReplanificarMostrado()` | Vuelve a planificar el mismo viaje que se está viendo. |
| `TextoDe(tipo)`, `PonerTexto(tipo, texto)` | Leen y escriben el texto de un campo. |
| `AnadirParada()`, `QuitarParada(id)` | Agregan o quitan una parada intermedia (y renumeran las demás en el mapa). |
| `EtiquetaMarcador(tipo)` | Número de la parada intermedia ("1", "2"...) o vacío. |
| `UsarLugar(l)` | Pone un lugar guardado en el campo activo. |
| `LugarParaGuardar`, `PuedeGuardarLugar` | Lugar que se puede guardar (el destino, o el origen elegido) y si ya no está guardado. |
| `EmpezarGuardarLugar()`, `CancelarGuardarLugar()`, `GuardarLugar()` | Flujo de guardar un lugar con nombre y tipo. |
| `UsarMiUbicacionLugar()` | Opción 1: usa dónde estás (con el nombre de la calle). |
| `BuscarDireccionLugar(texto)`, `ElegirDireccionLugar(s)` | Opción 3: escribir una dirección y elegirla de la lista (espera 450 ms y busca). |
| `AbrirCodigo()` | Lee el código pegado: viaje (lo muestra), lugar (lo pone de destino) o ruta (abre Rutas). |
| `GuardarCompartido()` | Guarda en el perfil lo compartido (pide cuenta). |
| `VerLugar(l)` | Muestra un lugar guardado. |
| `CampoParaElegir`, `EmpezarElegir(objetivo)`, `CancelarElegir()`, `AlClicMapa(pos)` | Elegir un punto tocando el mapa: activa la selección, pide el nombre de la calle y lo usa como lugar, origen, parada o destino. |
| `AlEscribir(tipo, texto)` | Espera 450 ms sin teclear (cancelando la espera anterior) y busca sugerencias. |
| `MarcarUbicacionAsync(tipo, texto)` | Busca y arma la lista de sugerencias: primero "Mi ubicación actual" (solo en origen), lugares guardados que coinciden (★), paradas de camión (P) y luego los lugares encontrados. |
| `Sugerencias(tipo)` | Fragmento visual que dibuja la lista de sugerencias (con clic y teclado). |
| `ElegirSugerencia(s)` | Escribe la sugerencia en el campo y la marca en el mapa. |
| `CerrarSugerencias()` | Cierra la lista (con 150 ms de espera para que alcance a registrarse el clic). |
| `Buscar()` | Valida que haya destino, convierte cada campo en coordenadas (`ResolverAsync`), guarda todo en `Estado` y navega a Comparar. |
| `ResolverAsync(tipo)` | Coordenadas `[lat, lng]` del campo: "Mi ubicación actual", el lugar elegido, o el primer resultado de lo escrito. |
| `Refrescar()`, `Dispose()` | Redibujar y cancelar temporizadores. |

## Comparar.razor (ruta `/comparar`)
| Variable | Qué guarda |
|---|---|
| `_mapa` | Componente de mapa. |
| `_opciones`, `_seleccion` | Opciones de viaje calculadas y la elegida. |
| `_version` | Número para descartar cálculos viejos si llega otra búsqueda. |
| `_conteo` | Texto "3 opciones encontradas · elige una". |
| `_enViaje`, `_llego`, `_confirmarFin` | Estados del viaje en curso. |
| `_pasos`, `_paso`, `PasoActual` | Pasos de navegación y el actual. |
| `_distanciaSiguiente` | Texto "Faltan 120 m para el siguiente paso". |
| `_seguimiento`, `_actualizando` | `Timer` de ubicación en vivo y bandera para no solapar actualizaciones. |
| `_compartir`, `_tituloCompartir`, `_codigo` | Datos del diálogo de compartir. |
| `MinutosRestantes` | Suma de los minutos de los tramos que faltan. |

| Función | Qué hace |
|---|---|
| `Compartir(r)` | Crea el código del viaje y abre el diálogo de compartir. |
| `OnAfterRenderAsync` | Calcula las opciones (o avisa si no hay origen y destino) y selecciona la primera. |
| `CalcularViajeAsync()` | Calcula cada tramo A→B, B→C... con `Planificador.Generar` en otros hilos (`Task.Run`), y une la opción N de cada tramo. Pone origen, destino y escalas en cada opción. |
| `Unir(partes)` | Junta varios tramos en un solo viaje ("Viaje en 2 tramos") sumando tarifa, minutos, transbordos y caminata. |
| `Seleccionar(r)` | Marca la opción, actualiza banner y tarjeta, y la dibuja. |
| `Comenzar()` | Registra el viaje en el historial, arma los pasos y empieza el seguimiento con un `Timer` cada 8 s. |
| `ActualizarBanner()` | Muestra "Viaje en curso · Llegas a las 14:35" o "¡Llegaste!". |
| `ActualizarPosicionAsync(centrar)` | Pide la ubicación, calcula la distancia al siguiente paso y avanza solo si está a menos de 60 m; detecta la llegada al destino. |
| `Metros(pos, lat, lng)` | Distancia aproximada entre dos puntos (convierte grados a metros). |
| `SiguientePaso()`, `IrAlPaso(i)` | Avanzar manualmente o saltar a un paso y centrar el mapa. |
| `TerminarViaje()` | Detiene el seguimiento, limpia la búsqueda y vuelve a Planificar. |
| `Refrescar()`, `Dispose()` | Redibujar y detener el `Timer`. |

## Rutas.razor (ruta `/rutas`)
| Miembro | Qué hace |
|---|---|
| `_mapa`, `_seleccion`, `_pedida` | Mapa, ruta elegida y ruta que otra pantalla pidió ver. |
| `_soloGuardadas`, `_busqueda` | Pestaña activa y texto de búsqueda. |
| `_compartir`, `_tituloCompartir`, `_codigo` | Diálogo de compartir. |
| `Visibles` | Rutas filtradas por pestaña y búsqueda (nombre, origen, destino o nombre de una parada). |
| `OnInitialized()` | Toma la ruta pedida o la última elegida y carga publicaciones (alertas). |
| `OnAfterRenderAsync` | Dibuja la ruta elegida o pide elegir una. |
| `Refrescar()` | Actualiza la tarjeta si cambió "guardada". |
| `CambiarPestana(soloGuardadas)`, `AlBuscar(texto)` | Cambian pestaña y búsqueda. |
| `Seleccionar(r)` | Marca la ruta y la dibuja con banner y tarjeta. |
| `QuitarSeleccion()` | Quita la ruta del mapa. |
| `PlanificarConRuta()` | Pone origen y destino de la ruta en `Estado` y abre Planificar. |
| `GuardarSeleccion()` | Guarda o quita de favoritas. |
| `Compartir(r)` | Código de ruta y diálogo. |

## Comunidad.razor (ruta `/comunidad`)
| Miembro | Qué hace |
|---|---|
| `_tipo`, `_busqueda` | Filtro por tipo (aviso/pregunta/propuesta) y búsqueda. |
| `_modal`, `_ocupado`, `_nuevoTipo`, `_rutaModal`, `_titulo`, `_mensaje`, `_errorModal` | Estado de la ventana "Nueva publicación". |
| `Dias` | Días de vida de la nueva publicación (`EstadoApp.DiasDeVida`). |
| `ElegirTipoPublicacion(tipo)`, `ElegirProblema(p)` | Eligen el tipo y, en un aviso, el problema. |
| `_dialogo`, `_textoDialogo`, `Mensaje(texto)` | Cuadro de mensaje. |
| `_abierta`, `_comentarios`, `_nuevoComentario` | Publicación con comentarios abiertos. |
| `IdUsuario`, `ConCuenta`, `EsMia(p)` | Datos del usuario y si la publicación es suya (para mostrar "eliminar"). |
| `AvisoReciente` | Último aviso (la franja de arriba). |
| `OnInitializedAsync()` | Carga las publicaciones (también para invitados). |
| `Recargar()` | Vuelve a pedirlas. |
| `TieneCodigo(p)`, `TextoVisible(p)`, `Importar(p)` | Detectan un código `RUTA1:` dentro de la publicación, muestran el texto sin el código y lo guardan en el perfil. |
| `Visibles` | Publicaciones filtradas y ordenadas (más "me gusta" primero, luego las más recientes). |
| `Contiene(texto, q)`, `Contar(tipo)` | Ayudas de búsqueda y conteo por tipo. |
| `Like(p)` | Da o quita "me gusta" (pide cuenta) y actualiza el conteo. |
| `AlternarComentarios(p)`, `Comentar()` | Abren y envían comentarios. |
| `Eliminar(p)` | Borra una publicación propia. |
| `Cta(p)` | Botón de la publicación: "Responder" abre comentarios; "Ver en el mapa" abre la ruta. |
| `Nueva()`, `Publicar()` | Abren el modal y publican (valida título y mensaje). |

## Perfil.razor (ruta `/perfil`)
| Miembro | Qué hace |
|---|---|
| `TipoLugar`/`TiposLugar`, `Opcion`/`Periodos`/`OrdenesGuardadas` | Opciones fijas para tipos de lugar, periodos del historial y orden de guardadas. |
| `_foto`, `_errorFoto` | Foto de perfil (base64) y error. |
| `_modalNombre`, `_errorNombre`, `_mensajeNombre`, `_guardando`, `_nuevoNombre` | Estado de "editar nombre". |
| `_confirmar`, `_porEliminar` | Confirmación y viaje guardado que se va a eliminar. |
| `_confirmarBorrar` | Confirmación de "Borrar historial". |
| `_pestana` | Pestaña activa: `historial`, `guardadas` o `lugares`. |
| `_filtro` | Filtro de guardadas: `todas`, `individuales` (rutas) o `personalizadas` (viajes). |
| `_busquedaHistorial`, `_periodo`, `_soloGuardados`, `_antiguosPrimero` | Filtros del historial. |
| `_busquedaGuardadas`, `_ordenGuardadas` | Filtros de guardadas. |
| `_codigoImportar`, `_mensajeImportar` | Importar un código. |
| `_compartir`, `_tituloCompartir`, `_codigo` | Diálogo de compartir. |
| `_agregandoLugar`, `_nombreLugar`, `_tipoLugar`, `_direccionLugar`, `_errorLugar`, `_elegidaLugar`, `_resultados`, `_espera`, `_buscandoUbicacion` | Formulario de agregar un lugar. |
| `HistorialFiltrado` | Historial filtrado por texto, periodo y "solo guardados", ordenado por fecha. |
| `RutasGuardadas`, `TrayectosGuardados`, `HayVacioGuardadas` | Rutas y viajes guardados filtrados (sin repetir) y si no hay nada que mostrar. |
| `OnInitializedAsync()` | Pide la foto a MySQL (si no hay conexión muestra las iniciales). |
| `EditarNombre()`, `GuardarNombre()`, `TeclaNombre(e)` | Editan y guardan el nombre en MySQL (Enter guarda, Escape cierra). |
| `CambiarFoto(e)` | Lee la imagen (máx. 2 MB), la convierte a base64 y la guarda. |
| `VerHistorial(v)`, `VerTrayecto(t)` | Abren el viaje en el mapa de Planificar. |
| `PedirEliminar(t)`, `Eliminar()` | Confirman y quitan un viaje guardado. |
| `AbrirCompartir`, `CompartirRuta`, `CompartirTrayecto`, `CompartirLugar` | Crean el código y abren el diálogo. |
| `Importar()` | Importa un código pegado. |
| `BuscarDireccion(texto)`, `ElegirDireccion(s)` | Buscan y eligen la dirección del lugar. |
| `ElegirEnMapa()` | Abre Planificar para tocar el lugar en el mapa. |
| `UsarMiUbicacion()` | Usa dónde estás como lugar. |
| `GuardarLugar()` | Valida nombre y dirección y guarda el lugar. |

## Configuracion.razor (ruta `/configuracion`) y AjusteDetalle.razor (ruta `/configuracion/ajuste`)
| Miembro | Qué hace |
|---|---|
| `Configuracion._busqueda` | Texto del buscador de ajustes. |
| `Configuracion._categorias` | Lista de `CategoriaAjustes`: GENERAL, ACCESIBILIDAD, CUENTA, SOPORTE, LEGAL con sus ajustes. |
| `Configuracion.Visibles` | Ajustes que coinciden con el texto (en español o en el idioma actual), sin categorías vacías. |
| `AjusteDetalle.Nombre` | Parámetro de la dirección (`?nombre=...`) con el nombre del ajuste. |
| `AjusteDetalle.Grupo`, `Clave` | Grupo y clave deducidos del nombre. |
| `AjusteDetalle.OnParametersSet()` | Convierte el nombre en (grupo, clave) y así decide qué componente `Ajustes*` mostrar. |

## SobreNosotros.razor y NotFound.razor
- `SobreNosotros.Explorar()`: entra como invitado (si no hay sesión) y abre la pantalla Rutas.
- `NotFound`: solo HTML (título y botón "Volver al inicio").

---

# Layout (`Components/Layout/`) y Routes

| Archivo | Miembros |
|---|---|
| **Routes.razor** | `ClasesAccesibilidad`: junta `a11y-dalton`, `a11y-guia` y `a11y-sin-anim` según los ajustes (activan los estilos de `CssAccesibilidad`). `OnInitialized` se suscribe a `Ajustes.Cambio`; `Refrescar`; `Dispose`. La etiqueta `@key="T.EsIngles"` hace que todo se redibuje al cambiar de idioma. |
| **_Imports.razor** | Sin funciones: lista los `using` comunes de todos los componentes e inyecta `Nav` (`NavigationManager`, para navegar) y `T` (el `Traductor`), por eso cualquier componente puede usar `Nav` y `T["texto"]` sin declararlos. |
| **Shared/HojaEstilos.razor** | Escribe `Hoja.Completa` dentro de una etiqueta `<style>`. `_pintada` y `ShouldRender()`/`OnAfterRender`: se dibuja una sola vez (después de la primera vez `ShouldRender` devuelve `false`) para no reinsertar miles de líneas de CSS. |
| **MainLayout.razor** | Solo HTML: marco de las páginas públicas y el cuadro "Ocurrió un error inesperado" de Blazor. |
| **ShellLayout.razor** | `_atajos` (rutas de Alt+1…5), `_colapsado` (sidebar contraído), `_contenido` (referencia al área de contenido para darle foco), `TextoSesion` ("Iniciar sesión" o "Cerrar sesión"), `OnInitialized` (si no ha ingresado, vuelve a `/`), `AlPulsar(e)` (atajos Alt con guía visual), `Ruta` (dirección actual), `Activo`/`AriaActual`/`EsActivo` (marcan el botón de la pantalla actual; Comparar cuenta como Planificar), `Migas` (texto "Planificar viaje › Comparar rutas"), `NombreAjuste()` (nombre del ajuste desde la dirección), `AlternarBarra()` (contrae el sidebar y avisa a los mapas), `BotonSesion()`, `Refrescar`, `AlCambiarUbicacion`, `Dispose`. |

---

# Componentes compartidos (`Components/Shared/`)

## Mapa.razor
Envuelve un mapa Leaflet con banner, tarjeta, controles, leyenda y el reporte de problemas. Las pantallas lo usan con `@ref` y llaman a sus funciones públicas.

**Parámetros (salidas):** `OnQuitar`, `OnPlanificar`, `OnGuardar` (botones de la tarjeta) y `OnClicMapa` (el usuario tocó el mapa con la selección activa).

| Variable | Qué guarda |
|---|---|
| `Banner` (clase) | Mensaje superior izquierdo: `Modo` (texto/viaje/ruta), `Texto`, `Subtexto`, `ConQuitar`. |
| `_id` | Identificador único del mapa (`mapa-` + Guid). |
| `_listo` | `TaskCompletionSource`: las funciones esperan a que el mapa esté listo. |
| `_mapa` | El `MapaLeaflet` que habla con JavaScript. |
| `_destruido` | `true` cuando el componente ya se destruyó. |
| `_banner`, `_tarjeta` | Banner y ruta de la tarjeta inferior. |
| `_versionRuta` | Cuenta cada dibujo para descartar los viejos. |
| `_leyendaAbierta`, `_pasosAbiertos`, `_detallesAbiertos` | Paneles plegables. |
| `_versionUbic` | Versión de la búsqueda por cada campo (descarta respuestas viejas). |
| `_reporte`, `_enviandoReporte`, `_reporteEnviado`, `_tipoReporte`, `_detalleReporte`, `_mensajeReporte`, `_rutaReporte` | Estado del reporte de problemas. |
| `TipoReporte` | El `ProblemaComun` elegido. |
| `_referencia` | Referencia a este componente para que JavaScript pueda llamarlo (`AlHacerClic`). |
| `_firmaReportes` | Texto con los ids de los reportes dibujados (evita redibujar sin necesidad). |

| Función | Qué hace |
|---|---|
| `DescripcionRecorrido(r)` | "Circuito de ida y vuelta · 38.1 km · 88 paradas". |
| `AlHacerClic(lat, lng)` (`[JSInvokable]`) | La llama JavaScript cuando el usuario toca el mapa. |
| `ActivarSeleccionAsync(activo)` | Activa o desactiva "tocar el mapa para elegir". |
| `OnInitialized()` | Crea el `MapaLeaflet` y se suscribe a `BarraLateralCambio` y `Cambio`. |
| `ActualizarReportesAsync()` | Dibuja los reportes con ubicación solo si cambió la lista. |
| `OnAfterRenderAsync` | Inicia el mapa en JavaScript y carga publicaciones. |
| `Listo()`, `EnMapa(accion)` | Esperan a que el mapa esté listo y ejecutan la acción ignorando errores de cierre. |
| `AlCambiarBarraLateral()` | Espera 300 ms y recalcula el tamaño del mapa. |
| `Quitar()`, `Planificar()`, `Guardar()` | Pasan los clics a los parámetros de salida. |
| `BannerTexto`, `BannerViaje`, `BannerRuta`, `OcultarBanner` | Muestran u ocultan el banner. |
| `MostrarTarjeta(r)`, `OcultarTarjeta()` | Tarjeta inferior de la ruta o del viaje. |
| `CambiarVariante(i)` | Cambia el ramal de la ruta y la redibuja. |
| `CentrarEn(lat, lng)`, `CentrarEnAsync` | Centran el mapa. |
| `LimpiarRuta()` | Cancela dibujos pendientes y limpia. |
| `DibujarRuta(r)` | Dibuja una ruta o un viaje. En una ruta: la dibuja de inmediato y luego la corrige (repara tramos o ajusta a calles con `ServicioTrazos`), guardando el resultado en la propia ruta para no pedirlo otra vez. |
| `DibujarLinea(r, pts, encuadrar)` | Dibuja la línea y sus paradas (proyectadas sobre el trazo; si una queda a más de 200 m, no se dibuja). En modo daltonismo usa símbolo y patrón. |
| `DibujarViaje(r, version)` | Dibuja el viaje y luego ajusta los tramos pendientes a las calles (a pie: caminata con instrucciones). |
| `DibujarTramos(r, encuadrar)` | Convierte los tramos en marcadores y líneas para el mapa. |
| `BuscarSugerenciasAsync(tipo, texto)` | Busca lugares (mínimo 2 letras) y los marca como pines numerados; devuelve `null` si llegó una búsqueda más nueva. |
| `ElegirUbicacion(tipo, nombre, lat, lng, etiqueta)`, `QuitarUbicacion(tipo)`, `LimpiarSugerencias()` | Marcan, quitan y limpian puntos. |
| `MiPosicion` | Última ubicación conocida. |
| `MostrarMiUbicacionAsync()` | Marca "Mi ubicación actual" como origen. |
| `ActualizarPosicionAsync(centrar)` | Vuelve a pedir la ubicación y dibuja el punto azul. |
| `CentrarEnMi()` | Botón "centrar en mí". |
| `CargarParadasAsync(...)` | Carga paradas y puntos de referencia (Overpass). |
| `AbrirReporte()`, `EnviarReporte()` | Abren el reporte (pide cuenta) y lo publican con ubicación si es de movilidad. |
| `Dispose()`, `DisposeAsync()` | Se dan de baja y cierran el mapa. |

## MapaLeaflet.cs
Clase que **manda al mapa JavaScript todo lo que hay que dibujar en una sola llamada**.

| Miembro | Qué hace |
|---|---|
| Records `Marcador`, `TramoMapa`, `ReporteMapa`, `PuntoReferencia` | Datos simples de lo que se dibuja. |
| `_js`, `_id`, `_iniciado` | Acceso a JavaScript, id del mapa y si ya se inició. |
| Clases `M` y `L` | Descripciones de marcador y línea; sus propiedades van en minúscula porque `mapa.js` las lee con esos nombres (`lat`, `lng`, `html`, `rotulo`, `pts`, `color`, `peso`, `dash`...). |
| `Linea(...)` | Genera la línea (con borde blanco debajo si se pide). |
| `Rotulo(nombre)`, `Svg(icono, tam)` | Texto seguro para HTML e icono SVG. |
| `Pin`, `Origen`, `Destino`, `Numerado`, `Parada`, `MiPosicion`, `Estilo(tipo)` | Construyen cada tipo de marcador (pin de origen/destino, círculo numerado, parada, punto azul, icono por tipo de punto de referencia). |
| `Metros(a, b)` | Distancia aproximada (para no dibujar destino igual a origen en un circuito). |
| `Llamar(funcion, args)` | Invoca `rutaMapa.<función>` solo si el mapa ya se inició. |
| `Iniciar(referencia)` | Crea el mapa en JavaScript. |
| `ActivarSeleccion`, `InvalidarTamano`, `LimpiarRuta` | Controles básicos. |
| `DibujarRuta(...)`, `DibujarViaje(...)` | Dibujan una ruta o un viaje. |
| `PonerUbicacion`, `QuitarUbicacion`, `MostrarSugerencias`, `CargarPuntos`, `MostrarReportes`, `PonerPosicion`, `Centrar` | Cada tipo de capa del mapa. |
| `DisposeAsync()` | Destruye el mapa en JavaScript. |

## wwwroot/mapa.js (JavaScript)
Un objeto global `window.rutaMapa`. Cada función recibe el `id` del mapa.

| Miembro | Qué hace |
|---|---|
| `mapas` | Todos los mapas creados, por id. |
| `CENTRO`, `ZOOM_PARADAS` (14), `ZOOM_PUNTOS` (15) | Centro de Chihuahua y niveles de zoom desde los que se muestran paradas y puntos de referencia. |
| `marcador(m, e)` | Crea un marcador de Leaflet con su icono HTML, rótulo y popup. |
| `linea(l, destino)` | Dibuja una polilínea. |
| `nuevo(id)` | Crea el mapa Leaflet con el mapa base de OpenStreetMap, las capas (ruta, paradas, puntos, sugerencias, reportes) y el control de zoom. |
| `alternar(mapa, capa, mostrar)` | Muestra u oculta una capa. |
| `encuadrar(e, pts, relleno, zoomMax)` | Ajusta la vista para que se vean todos los puntos (deja espacio abajo para la tarjeta). |
| `encuadrarUbicaciones(e)` | Encuadra origen, destino y paradas. |
| `con(id, accion)` | Ejecuta una acción si el mapa existe. |
| `iniciar(id, ref)` | Crea el mapa y avisa a C# (`AlHacerClic`) al tocarlo. |
| `seleccion(id, activo)` | Activa el modo "tocar para elegir" (cambia el cursor). |
| `quitar(id)`, `invalidar(id)` | Destruye el mapa / recalcula su tamaño. |
| `dibujarRuta(id, spec)`, `limpiarRuta(id)` | Dibuja o limpia líneas, paradas y marcadores. |
| `ponerUbicacion`, `quitarUbicacion`, `sugerencias`, `reportes`, `puntos`, `posicion`, `zoomA` | Manejan cada capa del mapa. |

## Componentes pequeños
| Archivo | Variables y funciones |
|---|---|
| **Icon.razor** | Parámetros `Nombre` (clave del icono), `Tam`, `Color`, `Relleno`, `Clase`; `Datos` busca el dibujo del icono en `Iconos.Rutas`. |
| **Iconos.cs** | `Rutas`: diccionario clave → trazo SVG (`"IcoBus"`, `"IcoMap"`...). |
| **CampoTexto.razor** | Parámetros: `Text`/`TextChanged` (enlace del texto), `Hint`, `Icono` o `Punto` (color), `Fondo`, `Alto`, `Redondeo`, `Tipo`, `MaxLength`, `OnFoco`, `OnFocoPerdido`, `OnTecla`, `ChildContent`. Privados: `_input`, `_enfocado`, `Estilo`. Funciones: `AlEscribir`, `AlEnfocar`, `AlPerderFoco`, `AlTecla`, `Enfocar()`. |
| **CampoPassword.razor** | `Text`/`TextChanged`, `OnTecla`; `_visible` alterna mostrar u ocultar la contraseña. |
| **Carrusel.razor** | `ConMarca`, `Titular`, `Subtitular`; `Fotos` (3 ciudades); `Cada` (6 s); `Total`; `_actual`, `_pausado`, `_timer`; `OnInitialized` crea el temporizador; `Pausar`/`Reanudar` (al pasar el mouse); `Ir(i)`, `Anterior()`, `Siguiente()`; `Dispose`. |
| **SlideMarca.razor** | Parámetros `Titular` y `Subtitular`: la diapositiva azul de marca. |
| **IntroAnimacion.razor** | `Fase` (Oculta, Pasando, Mostrando, Desvaneciendo); `SegundosPasada` 3.6, `SegundosLetras` 1.4, `SegundosDesvanecer` 0.7; `_visible`, `_saliendo`, `_terminada`, `_fase`, `_cts`; `OnTerminada`; `SecuenciaAsync` (recorre las fases con esperas); `Saltar()` (un clic); `TerminarAsync()`; `Dispose`. |
| **PantallaCarga.razor** | Solo se suscribe a `Estado.Cambio` para mostrar `Cargando` y `MensajeCarga`. |
| **AvisosGlobales.razor** | Se suscribe a `Estado.Cambio` para el toast (`Aviso`) y la confirmación de cierre de sesión. |
| **Dialogo.razor** | `Visible`/`VisibleChanged`, `Mensaje`, `TextoAceptar`, `TextoCancelar`, `OnAceptar`; `Aceptar()` y `Cancelar()` cierran el diálogo. |
| **CompartirDialogo.razor** | `Visible`/`VisibleChanged`, `Titulo`, `Codigo`; `_mensaje`, `_copiado`; `OnParametersSet` limpia al cerrar; `Cerrar()`; `Copiar()` usa `IPortapapeles`. |
| **Ayuda.razor** | `Titulo`, `Texto`, `Derecha`; `_abierta`; `Alternar()`, `Cerrar()`, `Tecla(e)` (Escape cierra); se redibuja cuando cambian los ajustes (el botón solo aparece con la guía visual). |

---

# Componentes de Configuración (`Components/Ajustes/`)

## Pantallas de detalle
| Archivo | Variables y funciones |
|---|---|
| **AjustesGeneral.razor** | `Clave` (`unidades` o `idioma`), `Millas`, `Ingles`; `ElegirIdioma(idioma)` y `Elegir(unidad)` guardan el ajuste, avisan y todo se redibuja. |
| **AjustesAccesibilidad.razor** | `Clave`, `Datos`; `CambiarDaltonismo(valor)`, `CambiarGuia(valor)`, `CambiarAnimaciones(valor)`: guardan y avisan. |
| **AjustesCuenta.razor** | `Clave`, `_confirmacion`, `_error`, `_borrando`; `Recordada` (si la sesión se recuerda), `PuedeBorrar` (solo si se escribió "BORRAR"); `Recordar(valor)`; `Borrar()` llama a `Estado.BorrarCuentaAsync`. |
| **AjustesSoporte.razor** | **Reportar:** `_tipo`, `_detalle`, `_mensaje`, `_ruta`, `_enviando`, `_exito`, `Seleccionado`, `Dias`, `TextoDuracion`, `ElegirTipo`, `Enviar()` (publica el aviso con ubicación si es de movilidad). **Comentarios:** `CategoriaOpinion`/`Categorias`, `_categoria`, `_comentario`, `_mensajeOpinion`, `_estrellas`, `_enviandoOpinion`, `_exitoOpinion`, `EnviarComentario()` (mínimo 10 letras, usa `OpinionService`). **Ayuda:** `PreguntaAyuda`/`Preguntas` (11 preguntas frecuentes), `_busqueda`, `_abierta`, `Visibles`, `Alternar(pregunta)`. |
| **AjustesLegal.razor** | Solo `Clave` (`terminos`, `privacidad` o `sobre`): el contenido es texto. |
| **ContenidoSobreNosotros.razor** | `Ubicacion` y `Lateral` (ancho de la columna lateral). |

## Piezas reutilizables
| Archivo | Parámetros y funciones |
|---|---|
| **MarcoDetalle.razor** | `Categoria`, `Titulo`, `Subtitulo`, `ChildContent`, `Barra`: marco de las pantallas de detalle. |
| **DosColumnas.razor** | `Lateral` (316 px), `Izquierda`, `Principal`, `Secundario`: contenido y panel fijo. |
| **NavegadorDocumento.razor** | `Titulo`, `Subtitulo`, `Secciones`, `Boton`: índice de un documento legal. |
| **Tarjeta.razor** | `Titulo`, `Subtitulo`, `Relleno` (24 px), `Clase`, `ChildContent`. |
| **FilaAjuste.razor** | `Icono`, `Titulo`, `Descripcion`, `Linea`, `ChildContent` (el control de la derecha). |
| **ParValor.razor** | `Etiqueta`, `Valor`, `Linea`, `Recuadro`. |
| **BarraInferior.razor** | `Info`, `TextoBoton`; `_guardado`; `Guardar()` muestra "Cambios guardados" 1.8 s. |
| **Interruptor.razor** | `Activo`/`ActivoChanged`; `Alternar()` invierte y avisa. |
| **Deslizador.razor** | `Valor`/`ValorChanged` (0–100); `AlCambiar(e)`. |
| **Desplegable.razor** | `Icono`, `Texto`: selector que solo muestra el valor. |
| **OpcionRadio.razor** | `Titulo`, `Descripcion`, `Seleccionada`, `Insignia`, `OnClick`. |
| **CasillaTexto.razor** | `Texto`, `Marcada`: casilla con texto. |
| **AreaTexto.razor** | `Texto`, `Maximo` (500): área con contador "12 / 500". |
| **Boton.razor** | `Texto`, `Icono`, `Relleno` (relleno o solo contorno), `OnClick`. |
| **Chip.razor** | `Texto`, `Variante` (primario, tinte, panel, sidebar), `Tam`. |
| **Aviso.razor** | `Icono`, `Titulo`, `Tipo` (info, ok, alerta), `ChildContent`. |
| **CajaIcono.razor** | `Icono`, `Lado` (38 px), `Relleno`. |
