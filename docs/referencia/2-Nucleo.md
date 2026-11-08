# Referencia de variables y funciones: RutaApp.Nucleo (lógica)

Parte de la [guía del código](../EXPLICACION_CODIGO.md).

**Cómo leer los nombres:** `_nombre` = campo privado; `Nombre` = propiedad pública; `static` = pertenece a la clase y se usa sin crear objetos; `...Async` = función que se espera con `await`. Las coordenadas viajan como `double[] { latitud, longitud }` y los recorridos como `List<double[]>`.

---

# Parte 1: Modelos (`Modelos/`)

Las clases de datos casi no tienen lógica: sus propiedades guardan información. Las propiedades "calculadas" (`=>`) arman textos listos para mostrar.

## Ruta.cs: clase `Ruta`

Es el modelo central. Representa una ruta de camión **o** una opción de viaje ya armada.

**Datos de la ruta**
| Propiedad | Qué guarda |
|---|---|
| `Numero` | Identificador de la ruta (es el `id` de la base). También decide su color, símbolo y patrón. |
| `Nombre` | Nombre de la ruta. |
| `Modo`, `ModoTexto` | Tipo de transporte ("Autobús"...) para filtros y para mostrar. |
| `FrecuenciaMin` | Cada cuántos minutos pasa el camión (10 por defecto). |
| `Tarifa` | Precio del pasaje (2.75 por defecto). |
| `Desvio` | Marca si la ruta tiene un desvío. |
| `Horario` | Horario de servicio (texto). |
| `Paradas` | Cantidad de paradas. |
| `Minutos` | Duración del recorrido o del viaje. |
| `Transbordos` | Cuántos cambios de camión tiene la opción de viaje. |
| `Descripcion` | Texto descriptivo de la tabla `routes`. |
| `Lineas` | Texto corto del camión (chip de la pantalla Comparar). |
| `Etiqueta` | Rótulo de una opción: Recomendado / Más rápido / Económico / Menos caminata. |
| `Guardada` | `true` si el usuario la marcó como favorita. |
| `Seleccionada` | `true` si es la opción o ruta elegida en pantalla (la resalta la lista). |
| `TrayectoFavorito` | Si es una opción de viaje guardada, el `Trayecto` correspondiente (aparece en Perfil). |
| `DatosReales` | `true` si minutos y tarifa vienen de la base de datos. |
| `Reparado` | `true` si ya se corrigieron los tramos del recorrido que cortaban esquinas. |
| `TrazoEnCalles` | `false` = el recorrido solo une paradas con rectas y debe ajustarse a las calles. |
| `OrigenNombre`, `DestinoNombre` | Nombre de la primera y última parada. |
| `LatO`, `LngO`, `LatD`, `LngD` | Coordenadas del origen (O) y destino (D). |
| `Trazo` | Recorrido como lista de `[lat, lng]`. |

**Propiedades de opción de viaje:** `EsViaje` (es un viaje armado, no una ruta del catálogo), `Resumen` ("Camina 4 min · Bus 12 min..."), `CaminataMetros`, `Tramos` (caminar/camión/caminar) y `Escalas` (paradas intermedias).

**Variantes (ramales):** `Variantes` (lista de `RutaOficial`), `VarianteActual` (índice de la elegida), `TieneVariantes` (hay más de una) y `Oficial` (la variante en uso).

**Propiedades calculadas (solo lectura):**
| Propiedad | Resultado |
|---|---|
| `Color`, `Simbolo`, `Patron` | Salen de `Paleta` según `Numero`. |
| `NumeroTexto` | `Numero` como texto. |
| `TarifaTexto` | `"$2.75"` (siempre con punto, independiente del idioma de Windows). |
| `MinutosTexto` | `"20 min"`. |
| `Meta` | `"Autobús · 20 min · $2.75"`. |
| `Subtitulo` | La descripción; si no hay, `modo · horario`. |
| `FrecuenciaTexto` | El `Resumen` de viaje o `"Cada 10 min desde parada central"`. |
| `EtiquetaMayus` | `Etiqueta` en mayúsculas. |
| `TransbordosTexto` | "Sin transbordos", "1 transbordo" o "N transbordos". |
| `Kilometros` | Largo del recorrido sumando los segmentos del `Trazo` (convierte grados a metros con 111 320 m por grado de longitud ajustado por el coseno de la latitud, y 110 574 m por grado de latitud). |

**Funciones**
| Función | Qué hace |
|---|---|
| `UsarVariante(indice)` | Cambia a otra variante: copia su trazo, `TrazoEnCalles`, `Reparado` y cantidad de paradas, y recalcula origen y destino con la primera y última parada (o con el primer y último punto del trazo si no hay paradas). Ignora índices inválidos. |
| `Pasos()` | Convierte los `Tramos` en una lista de `PasoViaje` (navegación paso a paso). Un tramo a pie genera "Camina N min" con distancia, destino ("la parada X" o "tu destino") e instrucciones; un tramo en camión genera "Toma <línea>" con parada de subida, minutos a bordo, parada de bajada y frecuencia. |

## Tramo.cs: clase `Tramo`

Un pedazo del viaje, a pie o en camión.
| Propiedad | Qué guarda |
|---|---|
| `EsBus` | `true` = camión (línea de color); `false` = a pie (punteado). |
| `Puntos` | Línea del tramo, `[lat, lng]`. |
| `TextoInicio`, `TextoFin` | Etiquetas que se muestran en el mapa al subir y al bajar (solo camión). |
| `Linea` | Nombre de la ruta que se toma. |
| `Color`, `Simbolo`, `Patron` | Cómo se dibuja (color, símbolo y guiones para daltonismo). |
| `Minutos`, `Metros` | Duración; distancia (a pie). |
| `AjustarACalles` | La línea es recta y hay que ajustarla a las calles al mostrarla. |
| `Reparar` | Recorrido guardado: se corrigen solo los tramos que cortan esquinas. |
| `Ajustado` | Marca interna: ya se ajustó o reparó, para no repetirlo. |
| `Instrucciones` | Pasos para caminar (calles y giros). |
| `ParadaInicio`, `ParadaFin` | Paradas (o zonas) donde se sube y se baja. |
| `EsperaMin` | Cada cuántos minutos pasa el camión. |

## Trayecto.cs: clase `Trayecto`
Un viaje guardado por el usuario.
`Id` (único, `Guid`), `Alias` (nombre que le pone el usuario), `Recorrido` ("Casa > Escuela"), `Detalle` ("18 min · $2.75"), `Origen`/`Destino` (nombres), `OrigenLat/Lng` y `DestinoLat/Lng`, `Minutos`, `Tarifa`, `Lineas`, `Tramos`, `Escalas`, `Compartido` (llegó por un código de otra persona) y `TieneRecorrido` (calculada: tiene tramos para dibujar).

## ViajeHistorial.cs: clase `ViajeHistorial`
Un viaje que el usuario ya hizo.
`Id`, coordenadas de origen y destino, `Fecha`, `Minutos`, `Tarifa`, `Destino`, `Origen`, `Camiones` ("Bus 6 → Bus 9"), `Tramos`, `Escalas` y `GuardadoId` (id del `Trayecto` si se guardó).
Calculadas: `Titulo` ("Origen > Destino"), `CamionesTexto`, `Cuando` (usa `Formato.Hace`), `Detalle` ("hace 5 min · 18 min"), `EstaGuardado` y `TieneRecorrido`.

## LugarGuardado.cs: clase `LugarGuardado`
`Id` (único), `Nombre`, `Tipo` (`casa`, `escuela`, `trabajo`, `otro`), `Direccion`, `Lat`, `Lng`. Calculadas: `Icono` (clave del icono según el tipo: `IcoHome`, `IcoSchool`, `IcoBriefcase` o `IcoFlag`) y `TipoTexto` ("Casa", "Escuela", "Trabajo", "Punto de referencia").

## Publicacion.cs: clases `Etiqueta` y `Publicacion`
- `Etiqueta`: `Texto`, `EsRuta` y `Icono` (calculado: `IcoBus` si es de una ruta, `IcoPin` si no).
- `Publicacion`: `Id` y `IdAutor` (de la base), `Tipo` (`aviso`/`pregunta`/`propuesta`), `Autor`, `Oficial`, `Tiempo`, `Titulo`, `Cuerpo`, `Etiquetas`, `Comentarios`, `Orden` (mayor = más reciente), `RutaNumero` (0 = no ligada a una ruta), `Likes`, `MeGusta`, `Fecha`, `Vence` (`null` = no caduca), `Lat`/`Lng` (ubicación de un reporte de movilidad).
  Calculadas: `Inicial` (primera letra del autor), `TipoTexto`, `IconoTipo`, `Cta` ("Responder" o "Ver en el mapa"), `TieneUbicacion` y `VenceTexto` ("Vence en 3 h", "Vence en 2 d"...).

## Sugerencia.cs, ParadaBuscada.cs, PasoViaje.cs
- `Sugerencia`: `Numero` (posición mostrada en el pin), `Nombre`, `Detalle`, `Lat`, `Lng`. Resultado del buscador de lugares o parada intermedia.
- `ParadaBuscada` (record): `Nombre`, `Lat`, `Lng`, `RutaNombre`, `RutaNumero`.
- `PasoViaje` (record): `Icono`, `Titulo`, `Detalle`, `EsBus`, `Color`, `Lat`, `Lng`, `Instrucciones` (opcional) y `Simbolo` (opcional).

## Paleta.cs: clase estática `Paleta`
| Miembro | Qué es |
|---|---|
| `Rutas` | 10 colores hexadecimales para las rutas. |
| `Simbolos` | 10 símbolos (`●`, `▲`, `■`...) para identificar rutas sin depender del color. |
| `Patrones` | 10 patrones de guiones para el trazo en el mapa (`""` = línea continua). |
| `DeRuta(numero)`, `Simbolo(numero)`, `Patron(numero)` | Devuelven el elemento de la lista que toca al número de ruta (`numero % 10`, así siempre hay uno). |

## Formato.cs: clase estática `Formato`
| Miembro | Qué hace |
|---|---|
| `UsaMillas` | `true` si el usuario eligió millas; lo pone `AjustesService`. |
| `Ingles` | `true` si el idioma es inglés; también lo pone `AjustesService`. |
| `Comparador` | Comparador de textos de `es-MX` para ignorar mayúsculas y acentos. |
| `Distancia(metros)` | "350 m" / "1.2 km" o, con millas, "1,150 ft" / "0.7 mi". |
| `ConvertirTexto(texto)` | En un texto de instrucciones, cambia cada "(120 m)" por la unidad elegida (solo si hay millas). |
| `NombreParada(nombre)` | Quita ", Chihuahua, Chihuahua" del final del nombre. |
| `Coincide(texto, buscado)` | `true` si `texto` contiene `buscado`, sin importar mayúsculas ni acentos. |
| `Hace(fecha)` | "ahora", "hace 5 min", "hace 3 h", "ayer", "hace 4 d", o la fecha si pasó más de una semana (en inglés: "now", "5 min ago"...). |

## Geo.cs: clase estática `Geo`
| Miembro | Qué hace |
|---|---|
| `CentroLat`, `CentroLng` | Centro de Chihuahua (28.6353, -106.0889). |
| `ProyectarEnLinea(linea, lat, lng, out metros)` | Busca el punto de la línea más cercano a (lat, lng) y devuelve ese punto; en `metros` (parámetro `out`) deja la distancia. Convierte grados a metros y recorre cada segmento de la línea calculando la proyección del punto sobre él (un valor `t` entre 0 y 1). Sirve para dibujar una parada desfasada encima del recorrido. |

---

# Parte 2: Servicios (`Servicios/`)

## Planificador.cs: clase estática `Planificador`

Arma opciones de viaje. Idea general: convierte cada ruta en una lista de paradas, crea un **grafo** donde cada parada de cada ruta es un nodo, une las paradas consecutivas (viaje en camión) y las paradas cercanas de rutas distintas (transbordo a pie), y busca el camino más barato con Dijkstra. Para obtener alternativas repite la búsqueda **prohibiendo** una por una las rutas del mejor resultado.

### Constantes
| Constante | Valor | Para qué |
|---|---|---|
| `MetrosPorMinCaminando` | 80 | Velocidad al caminar (≈4.8 km/h). |
| `MetrosPorMinBus` | 333 | Velocidad del camión (≈20 km/h). |
| `MaxCaminataMetros` | 1000 | Más lejos que esto una parada no se considera "cerca". |
| `MaxCaminataTransbordoMetros` | 350 | Distancia máxima caminando para transbordar. |
| `SeparacionParadasMetros` | 400 | Cada cuántos metros se crea una parada virtual en el recorrido. |
| `ViajeMinimoMetros` | 250 | Un tramo en camión debe recorrer al menos esto, si no se descarta. |
| `PenalizacionTransbordoMin` | 3 | Minutos extra que "cuesta" cada transbordo. |
| `ParadaCercaDestinoMetros` | 500 | Si hay parada real a menos de esto del destino, se recomienda bajar ahí. |
| `ParadaPegadaAlRecorridoMetros` | 120 | Una parada a más de esto del trazo se ignora. |
| `PesoCaminata` | 2.0 | Un minuto caminando "pesa" como 2: se prefiere caminar menos. |
| `ZonaVentajaMetros` | 150 | Si el recorrido pasa 150 m o más cerca que la parada, se recomienda esa zona. |
| `DistanciaMaximaMetros` | 40000 | Más lejos que esto no se calcula. |

### Clases internas
- **`Linea`**: una ruta ya preparada. `R` (la `Ruta`), `P` (trazo `[lat, lng]`), `M` (el trazo en metros x,y), `Cum` (distancia acumulada a lo largo del trazo), `Paradas` (índices del trazo que son parada), `Reales` (índices que son paradas reales de la base), `Nombres` (nombre de cada parada real), `Zonas` (puntos donde el camión pasa muy cerca del origen o destino), `Original` (si el punto viene del trazo original o se agregó al densificar), `TieneReales` y `Corto` (nombre corto o "Ruta N").
- **`Arista`**: una conexión entre nodos. `A`, `B` (nodos), `Costo` (minutos con el peso de caminar, lo que usa el algoritmo), `Real` (minutos de verdad) y `Viaje` (`true` = en camión, `false` = transbordo a pie).
- **`Nodo`**: una parada. `L` (su línea), `Idx` (posición en el trazo), `M` (posición en metros), `Real`, `Nombre`, `Zona` y `Sal` (aristas que salen de él).
- **`Opcion`**: un resultado. `Total` (minutos), `CaminaM`, `Tramos`, `Resumen` y `Lineas` (rutas usadas).

### Funciones
| Función | Qué hace |
|---|---|
| `Generar(rutas, oLat, oLng, dLat, dLng)` | **Función principal.** Convierte coordenadas a metros (función local `aM`), descarta si origen y destino están a más de 40 km, prepara cada ruta con `Construir`, agrega zonas, arma el grafo, elige dónde se puede subir y bajar, y busca hasta 12 veces (cola `pendientes`: la primera sin rutas vetadas, luego una por cada ruta de cada resultado) evitando repetir combinaciones con `firmas`. Ordena por un puntaje (`Total` + castigo por caminata + 2 por transbordo + tarifa), se queda con 4 y las convierte con `Convertir`. |
| `Etiquetar(lista)` | Pone etiquetas: la primera es "Recomendado"; luego "Más rápido", "Económico" y "Menos caminata" a la mejor de cada criterio que aún no tenga etiqueta. Es pública: también la usa `Comparar.razor` al combinar varios tramos. |
| `Construir(r, aM)` | Prepara una `Linea`: densifica el trazo (un punto cada 100 m), lo pasa a metros, calcula distancias acumuladas (descarta rutas de menos de 250 m), ubica cada parada real en el punto más cercano del trazo y agrega paradas virtuales cada ~400 m donde no haya una real cerca. |
| `Densificar(pts, aM, paso, original)` | Inserta puntos intermedios cada `paso` metros para que ninguna parada quede lejos del trazo; marca en `original` cuáles puntos son los originales. |
| `Dist(a, b)` | Distancia entre dos puntos ya en metros (Pitágoras). |
| `MinCam(metros)` | Minutos caminando esa distancia. |
| `Espera(l)` | Espera estimada: la mitad de la frecuencia de la ruta, mínimo 2 min. |
| `Min(metros)` | Igual que `MinCam` pero redondeado a entero (para textos). |
| `ConstruirGrafo(lineas)` | Crea los nodos; une cada parada con la siguiente (tiempo en camión); si el recorrido es un circuito cerrado (inicio y fin a menos de 100 m) une el último nodo con el primero; y crea las aristas de transbordo entre paradas de rutas distintas a menos de 350 m (costo = caminar con peso + espera + penalización). |
| `Elegible(n)` | En una ruta con paradas reales solo esas sirven para transbordar; si no tiene, cualquier punto. |
| `AgregarZonas(l, punto)` | Busca en la línea los puntos donde el camión pasa más cerca de `punto` (mínimos locales, hasta 4) y los agrega como zonas de subida/bajada. |
| `ElegirExtremos(nodos, O, D, out inicio, out fin)` | Para cada ruta decide en qué nodos se puede **empezar** (cerca del origen) y **terminar** (cerca del destino). |
| `ElegirEn(nodos, ids, punto, maxParada, resultado)` | Para un punto, compara la parada real más cercana con la zona más cercana y se queda con la zona solo si está al menos 150 m más cerca. |
| `MinimosLocales(nodos, ids, punto, maxMetros)` | De una lista de nodos en orden de recorrido, los que están más cerca de `punto` que sus vecinos (uno por cada pasada de la ruta: ida y vuelta). |
| `Buscar(nodos, vetadas, O, D, origen, destino, inicioOk, finOk)` | **Dijkstra** a mano sobre el grafo. El costo inicial de un nodo es caminar desde el origen más la espera; elige el mejor nodo final (costo + caminar al destino); reconstruye el camino con `previo`; agrupa en tramos por ruta; descarta si algún tramo es demasiado corto; calcula los minutos reales (sin el peso extra) y construye los `Tramo` con sus textos ("Sube en...", "Baja en...") y el `Resumen`. Devuelve `null` si no hay camino. |
| `Avance(l, i, j)` | Metros que recorre el camión de la parada `i` a la `j`, siempre hacia adelante (si `j` quedó atrás, da la vuelta al circuito). |
| `Bus(l, i, j, inicio, fin)` | Crea el `Tramo` en camión con los puntos reales del recorrido entre `i` y `j` (si hay que dar la vuelta, hasta el final y desde el principio). |
| `Camina(desde, hasta, metros)` | Crea un `Tramo` a pie: línea recta que luego se ajusta a las banquetas. |
| `Convertir(o)` | Convierte una `Opcion` en una `Ruta` (`EsViaje = true`) con nombre, líneas, tarifa sumada, transbordos, minutos y tramos. |

## RutaRepositorio.cs: clase `RutaRepositorio`
| Miembro | Qué hace |
|---|---|
| `_rutaService`, `_log` | Servicio de datos y registro de mensajes (inyectados por el constructor). |
| `LimpiarCache()` | Pide a `RutaService` olvidar lo leído. |
| `CargarAsync()` | Lee las rutas, las agrupa por `Grupo` (variantes de un mismo grupo = una ruta) y las arma con `Armar`. Devuelve `null` si no hay datos o si hubo error (lo anota en el registro). |
| `Armar(todas)` | Se queda con las variantes que tienen trazo de al menos 2 puntos (si ninguna, devuelve `null`). Crea la `Ruta` con número, nombre (el del grupo si hay varias variantes), tarifa, minutos y variantes, y activa la primera con `UsarVariante(0)`. |

## ServicioTrazos.cs: clase `ServicioTrazos`
| Miembro | Qué hace |
|---|---|
| `_fabrica`, `_log` | Fábrica de clientes HTTP y registro. |
| `_cacheOsrm` | Diccionario de recorridos ya pedidos a OSRM (clave = coordenadas). |
| `_cacheCaminata` | Lo mismo para las caminatas con instrucciones. |
| `Http` | Cliente HTTP "externo" (8 s de límite, con User-Agent). |
| `ClaveOsrm(puntos)` | Convierte `[lat, lng]` al texto `"lng,lat;lng,lat"` que pide OSRM. |
| `TrazoOsrmAsync(coordenadas, aPie)` | Pide el recorrido por calles (en coche o a pie), lo convierte a `[lat, lng]` y lo guarda en caché. Devuelve `null` si no hay internet. |
| `CaminataAsync(a, b)` | Pide a OSRM la caminata entre dos puntos con instrucciones; devuelve `(línea, pasos)` o `(null, null)`. |
| `InstruccionCaminar(paso)` | Convierte un paso de OSRM en texto en español ("Sal hacia el norte por Calle X (120 m)", "Gira a la derecha...", "Llegas al punto"). Ignora pasos de menos de 8 m. |
| `AjustarACallesAsync(puntos, aPie)` | Toma un punto cada ~350 m (máximo 200), los pide a OSRM en grupos de 25 con hasta 3 peticiones a la vez y une los resultados; si un grupo falla conserva sus puntos originales. |
| `ReparaTramosRectosAsync(pts, minimoMetros)` | Para los tramos rectos de 400 m o más, pide la ruta por calles y la usa si es más de 15 % más larga que la recta. Devuelve `(recorrido o null, completo)`; funciones locales `Largo` y `Dist`. |
| `Muestrear(pts, separacion, maximo)` | Se queda con un punto cada `separacion` metros (siempre el primero y el último) y reduce a `maximo` puntos si hace falta. |
| `CalleDeAsync(lat, lng)` | Nombre de la calle en esas coordenadas (Nominatim). |
| `BuscarLugaresAsync(texto)` | Delega en `BuscadorLugares.BuscarAsync`. |
| `PuntoMapa` | Un punto de referencia: `Lat`, `Lng`, `Nombre`, `Tipo` (parada, farmacia, hospital, escuela, supermercado). |
| `ParadasEntreAsync(latO, lngO, latD, lngD)` | Consulta Overpass con un rectángulo alrededor de origen y destino (margen de 0.005°) y devuelve hasta 80 puntos. |
| `ParsearOverpass(json)` | Lee la respuesta de Overpass con expresiones regulares y asigna el tipo de cada punto. |

## BuscadorLugares.cs: clase estática `BuscadorLugares`
| Miembro | Qué hace |
|---|---|
| `MinLon`, `MaxLon`, `MinLat`, `MaxLat` | Rectángulo de la ciudad: lo que cae fuera se descarta. |
| `CentroLat`, `CentroLng` | Centro de la ciudad (los resultados cercanos al centro salen primero). |
| `Abreviaturas` | Diccionario "av" → "avenida", "blvd" → "boulevard", etc. |
| `Alias` | Siglas y nombres cortos: "uach" → "Universidad Autonoma de Chihuahua". Varias búsquedas se separan con `|`. |
| `TiposDeCalle` | Palabras que identifican una calle ("calle", "avenida"...), para quitar calles repetidas. |
| `Candidato` | Resultado interno (`Nombre`, `Detalle`, `Lat`, `Lng`). |
| `Normalizar(s)` | Minúsculas, sin acentos ni signos y con espacios únicos. |
| `Expandir(texto)` | Aplica `Alias` o `Abreviaturas` al texto escrito. |
| `BuscarAsync(http, texto, maximo)` | Lanza Photon (una consulta por cada variante del texto) y Nominatim a la vez, espera máximo 3 s, junta resultados sin repetir, descarta los de fuera de la ciudad, ordena (primero lo que empieza igual, luego lo que lo contiene, desempate por cercanía al centro) y devuelve hasta `maximo` `Sugerencia`. |
| `PedirAsync(http, url)` | GET con límite de 6 s. |
| `Str(elemento, propiedad)` | Lee un texto de un JSON o `""` si no existe. |
| `PhotonAsync(http, q)` | Consulta Photon (sirve con texto a medias) limitada al rectángulo y al estado de Chihuahua. |
| `NominatimAsync(http, q)` | Consulta Nominatim para direcciones y calles dentro del rectángulo. |

## CodigoCompartir.cs
| Miembro | Qué hace |
|---|---|
| `Compartido` | Lo que lleva un código: `Tipo` (`lugar`/`viaje`/`ruta`), `Lugar`, `Viaje`, `RutaId`, `RutaNombre`. |
| `Prefijo` | `"RUTA1:"`, el inicio de todo código. |
| `MaxPuntosPorTramo` | 60: máximo de puntos por tramo dentro del código. |
| `Crear(contenido)` | Reduce el viaje, lo convierte a JSON, lo comprime con GZip y lo codifica en Base64 seguro para URL (sin `+`, `/` ni `=`); devuelve `"RUTA1:..."`. |
| `Leer(texto)` | Busca `RUTA1:` dentro de un texto (aunque tenga más palabras), extrae los caracteres válidos, descomprime y valida el tipo. Devuelve `null` si no es un código válido. |
| `Reducir(t)` | Copia el `Trayecto` con tramos simplificados (`AjustarACalles = true` para que al abrirlo se ajuste a las calles). |
| `Adelgazar(pts)` | Deja como máximo 60 puntos repartidos parejo y redondea a 5 decimales. |

## DatosUsuario.cs
| Miembro | Qué hace |
|---|---|
| `DatosGuardados` | Lo que se guarda por usuario: `Historial`, `Trayectos`, `Lugares`. |
| `_carpeta` | `%LocalAppData%\RutaApp\usuarios`. |
| `_cerrojo` | `SemaphoreSlim` que deja pasar una operación de archivo a la vez (evita que dos escrituras se pisen). |
| `_log` | Registro de avisos. |
| `Archivo(idUsuario)` | Ruta `{id}.json` del usuario. |
| `CargarAsync(idUsuario)` | Lee el JSON; si no existe o falla, devuelve datos vacíos. |
| `GuardarAsync(idUsuario, datos)` | Escribe a un `.tmp` y lo reemplaza (un cierre a mitad no rompe el archivo). |
| `BorrarAsync(idUsuario)` | Borra el archivo (al eliminar la cuenta). |

## AjustesService.cs
| Miembro | Qué hace |
|---|---|
| `AjustesData` | `Unidades` (`km`/`mi`), `Idioma` (`es`/`en`), `Daltonismo`, `GuiaVisual`, `SinAnimaciones`, `SesionUsuarioId` (usuario que pidió "Recordarme"). |
| `_archivo` | `%LocalAppData%\RutaApp\ajustes.json`. |
| `Datos` | Los ajustes actuales. |
| `Cambio` | Evento que se dispara al guardar; la interfaz se redibuja. |
| constructor | Lee el archivo (o usa valores por defecto si no existe o está dañado) y aplica el formato. |
| `AplicarFormato()` | Copia unidades e idioma a `Formato.UsaMillas` y `Formato.Ingles`. |
| `Guardar()` | Aplica el formato, escribe el archivo (si no puede, los ajustes valen solo mientras la app está abierta) y dispara `Cambio`. |

## ServicioUbicacion.cs
| Miembro | Qué hace |
|---|---|
| `IServicioUbicacion.ObtenerAsync()` | Contrato: devuelve `[lat, lng]` o `null`. |
| `ServicioUbicacionIp` | Implementación por IP: consulta `ipwho.is` y devuelve la posición aproximada, o `null` si falla. Tiene `_fabrica` (clientes HTTP). |

## RegistroNucleo.cs
`AgregarNucleo(servicios)`: llama a `AgregarDatos`, configura el cliente HTTP "externo" (8 s, User-Agent `RutaApp/1.0`) y registra `ServicioTrazos`, `ServicioUbicacionIp`, `AjustesService` y `DatosUsuarioService` como *singleton* (una sola instancia) y `RutaRepositorio` como *scoped* (una por sesión). `IServicioUbicacion` apunta por defecto a `ServicioUbicacionIp`.
