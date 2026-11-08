# Referencia de variables y funciones: RutaApp.Motor y Pruebas

Parte de la [guía del código](../EXPLICACION_CODIGO.md).

**Importante:** el Motor no está conectado a la app (solo lo usan las pruebas). Es un motor independiente y bien probado; la app real planea con `Planificador.cs` (ver [2-Nucleo.md](2-Nucleo.md)).

**Unidades del Motor:** tiempos en minutos, distancias en metros, coordenadas en grados decimales.

**Palabras clave:** *grafo* = red de paradas (nodos) unidas por conexiones (aristas); *Dijkstra* = algoritmo que halla el camino de menor costo; *A\** = Dijkstra con una "cota" que adivina lo que falta; *Pareto* = una opción **domina** a otra si es igual o mejor en todo y mejor en algo.

---

# Modelos/ModelosTransporte.cs

### `Coordenada`
| Miembro | Qué es |
|---|---|
| `Latitud`, `Longitud` | Punto geográfico. |
| constructor `Coordenada(lat, lng)` | Crea el punto (también hay uno vacío para el JSON). |
| `EsValida` | `true` si no es NaN y está en rango (latitud ±90, longitud ±180). |
| `ToString()` | Texto `"28.63530, -106.08890"`. |

### `Parada`
`Id`, `Nombre`, `Latitud`, `Longitud`, `Accesible` (tiene rampa o andén para accesibilidad), `Posicion` (calculada: la `Coordenada`) y `ToString()`.

### `SentidoRecorrido` (enum) y `RutaAutobus`
- `SentidoRecorrido`: `Unidireccional` (solo en el orden de `IdsParadas`), `Bidireccional` (ida y vuelta) y `Circular` (la última parada vuelve a la primera).
- `RutaAutobus`: `Id`, `Nombre`, `IdsParadas` (orden de recorrido, sin repetir), `Sentido` y `Confiabilidad` (0 a 1, `null` = sin datos).

### `Conexion`
Un tramo dirigido entre dos paradas: `IdOrigen`, `IdDestino`, `IdRuta` (`null` = caminata de transbordo), `TiempoMin`, `DistanciaM`. `EsCaminata` (calculada: `IdRuta == null`) y `ToString()`.

### `Prioridad` (enum) y `Preferencias`
`Prioridad`: `Equilibrado`, `MasRapida`, `MenosTransbordos`, `MenosCaminata`, `MasConfiable`.

| Propiedad de `Preferencias` | Valor por defecto | Qué controla |
|---|---|---|
| `PesoTiempo`, `PesoTransbordos`, `PesoCaminata`, `PesoConfiabilidad` | 1 cada uno | Importancia de cada criterio al puntuar (se normalizan para sumar 1). |
| `MaxTransbordos` | 3 | Máximo de cambios de camión. |
| `RadioBusquedaMetros` | 800 | Radio para buscar paradas cerca del origen y destino. |
| `MaxParadasCandidatas` | 3 | Cuántas paradas cercanas se prueban en cada extremo. |
| `MaxAlternativasPorPar` | 15 | Alternativas máximas por par de paradas. |
| `MaxExpansiones` | 20000 | Tope de pasos del A* (evita búsquedas eternas). |
| `FactorTiempoMaximo` | 2 | Se descartan recorridos que tarden más que (el más rápido × este factor). |
| `SoloParadasAccesibles` | `false` | Exige paradas accesibles al subir, bajar y transbordar. |
| `VelocidadCaminataKmh` | 4.5 | Velocidad al caminar. |
| `FactorRodeoCaminata` | 1.3 | Caminando = línea recta × 1.3 (las calles no son rectas). |
| `RadioTransbordoMetros` | 300 | Distancia máxima para transbordar caminando. |
| `PenalizacionTransbordoMin` | 5 | Minutos que "cuesta" un transbordo en la búsqueda (no se suman al tiempo reportado). |

| Función / propiedad | Qué hace |
|---|---|
| `Para(prioridad)` | Crea preferencias donde el criterio elegido pesa 6 y los demás 1. |
| `SumaPesos` | Suma de los cuatro pesos. |
| `VelocidadCaminataMetrosPorMin` | Convierte km/h a metros por minuto. |
| `Validar()` | Devuelve la lista de problemas (pesos negativos o todos en cero, límites inválidos...); vacía si todo está bien. |

### `TramoRecorrido` y `Recorrido`
`TramoRecorrido`: un pedazo seguido de un `Recorrido` (`EsCaminata`, `IdRuta`, `NombreRuta`, `Paradas`, `Minutos`, `DistanciaM`).

`Recorrido`: un camino completo ya medido. Propiedades (solo se asignan dentro de `Crear` y `AsignarAccesos`):
| Propiedad | Qué es |
|---|---|
| `Paradas`, `Conexiones`, `Autobuses`, `Tramos` | Las paradas visitadas, las conexiones usadas, las rutas de camión usadas y los tramos agrupados. |
| `Origen`, `Destino` | Ubicación real de quien viaja (se asigna con `AsignarAccesos`). |
| `AccesoM`/`AccesoMin`, `EgresoM`/`EgresoMin` | Caminata del origen a la primera parada y de la última parada al destino. |
| `TiempoAutobusMin`, `TiempoCaminataMin`, `TiempoTotalMin` | Tiempos (el total es la suma de los dos). |
| `DistanciaAutobusM`, `DistanciaCaminataM`, `DistanciaTotalM` | Distancias. |
| `Transbordos` | Cuántos cambios (autobús → caminata → autobús cuenta como uno). |
| `Confiabilidad` | Producto de las confiabilidades de los autobuses usados; `null` si falta el dato en alguno. |
| `TodasParadasAccesibles` | Si las paradas de subida/bajada son accesibles. |
| `ParadaInicial`, `ParadaFinal` | Primera y última parada. |
| `Trazo` | Lista `[origen, paradas..., destino]` lista para dibujar. |
| `Firma` | Texto que identifica el recorrido por (ruta, parada de subida, parada de bajada) de cada tramo; sirve para detectar duplicados. |
| `FirmaDeRutas` | Texto que identifica solo la secuencia de líneas. |

| Función | Qué hace |
|---|---|
| `Crear(conexiones, paradas, rutas)` | Arma el recorrido recorriendo las conexiones: valida que sean consecutivas, sin tramos ni paradas repetidas, agrupa en tramos, suma tiempos y distancias, cuenta transbordos y calcula confiabilidad y accesibilidad. Lanza `ArgumentException` si hay huecos, ciclos o referencias inexistentes. |
| `AsignarAccesos(origen, accesoM, accesoMin, destino, egresoM, egresoMin)` | Suma la caminata inicial y final; resta lo asignado antes para no duplicar si se llama dos veces. |

### `RutaRecomendada`, `TipoError`, `ResultadoBusqueda`
- `RutaRecomendada`: `Posicion` (1 = mejor), `Recorrido`, `Puntuacion` (0 a 100, mayor es mejor), `Costo` (0 a 1, menor es mejor), `Ventajas`, `Desventajas`, `Explicacion`.
- `TipoError`: `Ninguno`, `UbicacionInvalida`, `PreferenciasInvalidas`, `CantidadInvalida`, `SinParadasCercanasAlOrigen`, `SinParadasCercanasAlDestino`, `OrigenIgualDestino`, `MismaParada`, `DestinoInaccesible`, `SinAlternativas`.
- `ResultadoBusqueda`: `Exito` (calculada), `Error`, `Mensaje`, `Advertencias`, `Recomendaciones` y `Fallo(error, mensaje)` (crea un resultado de error).

---

# Grafo/GrafoTransporte.cs: clase `GrafoTransporte`

| Miembro | Qué hace |
|---|---|
| `_paradas`, `_rutas` | Diccionarios por id. |
| `_salientes`, `_entrantes` | Listas de conexiones que salen de / llegan a cada parada. |
| `_clavesConexion` | Conjunto para detectar conexiones duplicadas. |
| `SinConexiones` | Lista vacía reutilizable. |
| `Paradas`, `Rutas`, `Conexiones`, `NumeroParadas` | Vistas de solo lectura. |
| `AgregarParada(p)`, `AgregarRuta(r)` | Las agregan; lanzan error si falta el id o ya existe. |
| `AgregarConexion(c)` | Rechaza paradas o rutas inexistentes, conexión a sí misma, tiempos negativos o no numéricos y duplicados. |
| `ExisteParada(id)`, `ObtenerParada(id)`, `ObtenerRuta(id)` | Consultas por id (devuelven `null` si no existe). |
| `Salientes(id)`, `Entrantes(id)` | Conexiones que salen / llegan (lista vacía si no hay). |
| `CrearRecorrido(conexiones)` | Delega en `Recorrido.Crear`. |
| `PuedeContinuar(previa, siguiente, soloAccesibles)` | **Reglas de transición** que comparten Dijkstra y A*: no se empieza ni se encadenan caminatas; con `soloAccesibles`, toda parada donde se sube, baja o cambia debe ser accesible. |
| `PuedeTerminar(ultima, soloAccesibles)` | Solo se termina bajando de un autobús (y en parada accesible si se pidió). |
| `EsTransbordo(previa, siguiente)` | `true` si `siguiente` es subir a un autobús nuevo. |
| `EsAccesible(id)`, `EsNoNegativo(v)`, `Lista(d, clave)` | Ayudas privadas: accesibilidad de una parada, validar números y obtener-o-crear una lista de un diccionario. |

---

# Algoritmos/

## AlgoritmoDijkstra.cs
| Miembro | Qué hace |
|---|---|
| `MonticuloMinimo<T>` | Cola de prioridad (montículo binario): `Insertar(prioridad, valor)` y `Extraer(out prioridad)` devuelven siempre el de menor prioridad; el desempate por orden de inserción hace los resultados deterministas. `Cantidad` es cuántos hay. Internos: `_datos`, `_contador`, `Menor` e `Intercambiar`. |
| `ResultadoDijkstra` | `Alcanzable`, `Costo`, `Conexiones`, `Recorrido` y `Motivo` (por qué no hay camino). |
| `_grafo` | El grafo sobre el que se busca. |
| `Nodo` (clase privada) | Estado de la búsqueda: `IdParada`, `Llegada` (conexión usada), `Padre` y `Costo`. |
| `CaminoMasCorto(origen, destino, costo, penalizacionTransbordo, soloAccesibles)` | Dijkstra. `costo` es una función que da el peso de cada conexión (0 o más; infinito la descarta; negativo o NaN lanza error). El estado es (parada + autobús con el que se llegó) porque cambiar de autobús cuesta. Devuelve un `ResultadoDijkstra`. |
| `DistanciasHaciaDestino(destino, costo)` | Dijkstra al revés: costo mínimo de cada parada al destino. Sirve de cota inferior para el A*. |
| `Reconstruir(final)` | Sigue los `Padre` hasta el origen y crea el `Recorrido`. |
| `Clave(n)` | Texto `parada|ruta` que identifica un estado (`#` si se llegó caminando). |
| `NoAlcanzable(motivo)` | Crea el resultado de "no hay camino". |

## BuscadorRutas.cs: clase `BuscadorRutas`
| Miembro | Qué hace |
|---|---|
| `_grafo`, `_dijkstra` | Grafo y algoritmo auxiliar. |
| `Parcial` | Camino a medias: `IdParada`, `Llegada`, `Padre`, `Tiempo`, `Transbordos`. |
| `Buscar(origen, destino, preferencias)` | **A\*** que no descarta caminos al llegar a una parada. Primero halla el camino más rápido (fija el límite de tiempo = mejor × `FactorTiempoMaximo`) y las cotas. Luego extiende caminos por (tiempo + cota) para que salgan ordenados por tiempo; poda por transbordos, límite de tiempo y paradas repetidas; se detiene al llegar a `MaxAlternativasPorPar` o `MaxExpansiones`. |
| `YaPasoPor(parcial, id)` | `true` si el camino ya pasó por esa parada (evita ciclos). |
| `Armar(final)` | Construye el `Recorrido` de un camino terminado. |

## BuscadorMulticriterio.cs: clase `BuscadorMulticriterio`
| Miembro | Qué hace |
|---|---|
| `Tolerancia` | 1e-6: diferencia mínima para considerar que algo es mejor o peor. |
| `_grafo`, `_dijkstra`, `_buscador` | Herramientas que usa. |
| `Buscar(origen, destino, preferencias)` | Junta los candidatos de `BuscadorRutas` con una versión de Dijkstra por cada `Variante`, descarta los que exceden transbordos o tiempo, quita duplicados y se queda con los no dominados. |
| `Domina(a, b)` | `true` si `a` es igual o mejor que `b` en tiempo, transbordos y caminata (y confiabilidad, solo si se conoce en ambos) y estrictamente mejor en al menos uno. |
| `FiltrarNoDominados(recorridos)` | Frente de Pareto: conserva los que ningún otro domina. |
| `Variante` | Un criterio a optimizar: función `Costo` y `PenalizacionTransbordo`. |
| `Variantes(p)` | Genera: más rápida; menos transbordos (penalización 1000); menos caminata (caminar cuesta ×20); equilibrada según tus pesos; y más confiable (solo si hay datos, usa `-log(confiabilidad)` para que sumar costos equivalga a multiplicar confiabilidades). |

## EvaluadorRutas.cs
**`EvaluadorRutas`** (estática):
| Función | Qué hace |
|---|---|
| `Evaluar(candidatos, preferencias, advertencias)` | Calcula `Costo` (0 a 1) y `Puntuacion` (0 a 100) = `(1 − costo) × 100`. Normaliza tiempo, transbordos, caminata e "inconfiabilidad" entre los candidatos y pondera con los pesos. Si falta la confiabilidad en alguno, la omite de **todos** y avisa. |
| `Normalizar(valores)` | Mínimo-máximo: 0 = el mejor, 1 = el peor; si todos son iguales, todos valen 0. |
| `Explicar(alternativas)` | Llena `Ventajas`, `Desventajas` y `Explicacion` comparando cada opción con las demás ("Es directa", "La más rápida: 14 min", "Camina 400 m (...)"). |
| `Resumir(r)` | "Línea 1 (A → B) → camina 120 m hasta C → Línea 2 (...)". |
| `ConstruirExplicacion(alt)` | Une resumen, totales, ventajas y desventajas en un texto. |
| `Min`, `Metros`, `Plural` | Dan formato a minutos, metros y plurales. |

**`ClasificadorRutas`** (estática):
| Función | Qué hace |
|---|---|
| `EliminarDuplicados(recorridos, porSecuenciaDeRutas)` | Quita repetidos conservando el más rápido de cada grupo (clave `Firma` o `FirmaDeRutas`). Hay una versión para `RutaRecomendada` que conserva la de mayor puntuación. |
| `Ordenar(alternativas)` | Ordena por puntuación (desempata por tiempo, transbordos, caminata y firma) y numera `Posicion` desde 1. |
| `SeleccionarMejores(alternativas, cantidad)` | Las primeras `cantidad` ordenadas. |
| `Comparar`, `CompararRecorridos` | Comparaciones que usan las anteriores (negativo = el primero es mejor). |

---

# Servicios/

## ServicioTransporte.cs: clase `ServicioTransporte`
| Miembro | Qué hace |
|---|---|
| `DistanciaMinimaViajeM` | 50 m: más cerca que esto no hace falta camión. |
| `_datos`, `_ubicaciones` | Datos de transporte y buscador de paradas cercanas. |
| `_candado`, `_claveGrafo`, `_grafo` | Caché del grafo; el candado evita que dos hilos lo reconstruyan a la vez. |
| `AdvertenciasDatos` | Advertencias detectadas al cargar los datos. |
| constructor | Valida los datos (lanza si hay errores) y prepara lo necesario. |
| `BuscarMejoresRutas(origen, destino, preferencias, cantidad)` | **Función principal.** Valida entradas (devuelve un `ResultadoBusqueda.Fallo` con el `TipoError` correspondiente), busca paradas cercanas a origen y destino, prueba cada par con el `BuscadorMulticriterio`, asigna la caminata inicial y final, quita duplicados y dominadas, puntúa, quita secuencias repetidas, toma las mejores y las explica. |
| `ObtenerGrafo(p)` | Devuelve el grafo en caché o lo reconstruye (si cambian los parámetros de caminata), agregando los transbordos caminando entre paradas cercanas con servicio. |
| `MensajeSinParadas(lugar, p)` | Mensaje de "no hay paradas a menos de X m". |

## ServicioUbicaciones.cs
| Miembro | Qué hace |
|---|---|
| `ParadaCercana` | Una `Parada` con `DistanciaLineaRectaM` (la que filtra por radio), `DistanciaCaminandoM` (estimada) y `TiempoCaminandoMin`. |
| `RadioTierraM` | 6 371 008.8 m. |
| `_paradas` | Lista de paradas. |
| `DistanciaLineaRectaM(...)` | Fórmula de **Haversine**: distancia sobre la esfera terrestre entre dos puntos. |
| `EstimarDistanciaCaminandoM(recta, factor)` | `recta × factor de rodeo`. |
| `TiempoCaminandoMin(distancia, velocidadKmh)` | Minutos caminando esa distancia. |
| `BuscarParadasCercanas(ubicacion, preferencias, maximo)` | Paradas dentro del radio, de la más cercana a la más lejana. |
| `ParesCercanos(radioM, preferencias)` | Pares de paradas a menos de `radioM` (para los transbordos a pie). |
| `Crear(parada, recta, preferencias)` | Arma una `ParadaCercana` con la estimación de caminata. |

## ServicioDatosTransporte.cs
| Miembro | Qué hace |
|---|---|
| `DatosTransporte` | `Paradas`, `Rutas`, `Conexiones`. |
| `ResultadoValidacion` | `Errores`, `Advertencias`, `EsValido`. |
| `OpcionesJson` | Opciones del JSON (sentidos como texto). |
| `Validar(datos)` | Revisa ids repetidos, coordenadas, rutas con paradas inexistentes o repetidas, conexiones inconsistentes con el sentido de su ruta, duplicados; lista errores y advertencias. No modifica nada. |
| `ConstruirGrafo(datos)` | Valida (lanza `InvalidDataException` con todos los errores) y arma el grafo. |
| `DesdeJson`, `DesdeArchivo`, `AJson` | Leer y escribir los datos como JSON. |
| `TramosEsperados(r)` | Tramos `origen>destino@ruta` que debe tener una ruta según su sentido. |
| `Descripcion(tramo)` | Convierte un tramo a texto legible para los errores. |

## Datos/DatosPrueba.cs: clase estática `DatosPrueba`
| Función | Qué hace |
|---|---|
| `Crear()` | Construye la red ficticia: 12 paradas (P01–P12), 4 rutas (R1–R4) y sus conexiones. |
| `CercaDe(idParada, desplazamientoLatitud, desplazamientoLongitud)` | Una coordenada a unos metros de una parada de prueba (0.0008° ≈ 90 m). |
| `TiemposPorTramo(idRuta)` | Minutos inventados de cada tramo de una ruta. |
| `P(...)` | Crea una `Parada`. |
| `Tramo(...)` | Crea una `Conexion` con la distancia en línea recta entre las dos paradas. |

---

# tests/RutaApp.Pruebas/PruebasMotorRutas.cs

52 pruebas xUnit, agrupadas por tema (el prefijo del nombre indica qué se prueba). Cada una arma un grafo o un servicio, ejecuta y comprueba el resultado con `Assert`.

**Ayudas privadas del archivo**
| Miembro | Qué hace |
|---|---|
| `Tiempo` | Función de costo = minutos de la conexión. |
| `C(origen, destino, ruta, min, metros)` | Crea una `Conexion` rápidamente. |
| `Mini(paradas, rutas, conexiones...)` | Crea un grafo pequeño en (0,0) para probar la lógica sin geografía. |
| `GrafoPrueba()` | Grafo con `DatosPrueba`. |
| `GrafoPruebaConCaminata()` | Igual, con el transbordo caminando P03 ↔ P09. |
| `Estrechas(prioridad)` | `Preferencias` con radio de 200 m (solo la parada más cercana a cada extremo). |
| `Servicio()` | `ServicioTransporte` con los datos de prueba. |
| `Recorrer(g, de, a)` | Dijkstra por tiempo y devuelve el `Recorrido`. |
| `SoloPorRuta(g, de, a, idRuta)` | Recorrido forzado por una ruta. |
| `DirectoContraConTransbordo(out directo, out rapido)` | Prepara dos recorridos para probar el evaluador. |

**Grupos de pruebas**
| Prefijo | Qué verifican |
|---|---|
| `DatosPrueba_` | La red de prueba es válida. |
| `Dijkstra_` / `DistanciasHaciaDestino_` | Camino más rápido, sentidos únicos, destino inaccesible, rechazo de costos negativos, ciclos, penalización por transbordo, solo accesibles, cotas inferiores. |
| `Grafo_` | Rechaza conexiones y paradas inválidas. |
| `Recorrido_` / `Confiabilidad_` | Conteo de transbordos, no duplicar tramos ni caminata, rechazo de ciclos y huecos, confiabilidad nunca inventada. |
| `BuscadorRutas_` | Directos y con transbordo ordenados por tiempo, límites configurables, destinos inaccesibles. |
| `Multicriterio_` | Descarta dominadas, conserva alternativas con ventajas distintas, confiabilidad desconocida no domina. |
| `Evaluador_` / `Preferencias_` | Las preferencias cambian el orden, normalización 0–1, sin datos de confiabilidad advierte, validación. |
| `Clasificador_` | Duplicados exactos y por secuencia, orden determinista. |
| `Ubicaciones_` | Distancia geográfica, caminata estimada, radio, ubicaciones inválidas, accesibilidad. |
| `Datos_` | Validación, advertencias, JSON de ida y vuelta, archivos inválidos, ruta circular. |
| `Servicio_` | El servicio completo: alternativas ordenadas y explicadas, prioridad, caminatas, transbordos, confiabilidad desconocida, errores (ubicaciones, preferencias, cantidad, sin paradas, inaccesible, misma parada), solo accesibles, sin duplicadas ni dominadas, reutilización del grafo. |
