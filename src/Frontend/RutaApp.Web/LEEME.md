# Muestra web (GitHub Pages)

La misma interfaz de la app de escritorio (`RutaApp.Interfaz`), compilada a WebAssembly para que cualquiera la pruebe desde el
navegador, sin instalar nada. Se publica sola en GitHub Pages cuando se sube algo a la rama `RUTA-Muestra`
(`.github/workflows/muestra-web.yml`).

## Qué incluye

| Sección | En la muestra |
|---|---|
| Rutas | Las mismas 10 rutas de la base de datos, con recorrido, paradas, variantes y búsqueda. El marcador las guarda en la pestaña **Guardadas**. |
| Planificar viaje | Búsqueda de origen y destino, comparación de opciones, paso a paso, lugares guardados y códigos para compartir. |
| Historial | Los viajes que se comienzan quedan en el historial, que **por ahora vive solo en Rutas** (botón del menú sobre el mapa): se pueden ver otra vez en el mapa y borrar. |
| Comunidad, Mi perfil, Configuración e Iniciar sesión | Existen, pero dicen **Disponible próximamente**. |

Lo que la persona guarda (historial, lugares y rutas guardadas) se queda en el `localStorage` de su navegador: no se sube a ningún servidor.

## Avisos y ayudas de la demostración

- **Aviso de demostración:** en la landing y en una ventana al entrar ("Esto es una demostración": puede tener fallas y las rutas de camión
  son limitadas, no están todas las de la ciudad). El botón **Demo** de la barra superior (escritorio) la vuelve a abrir.
- **Lugares sugeridos:** al tocar el destino vacío se recomiendan lugares conocidos de Chihuahua (`Servicios/LugaresSugeridos.cs`). Solo salen los
  que quedan a menos de 500 m del recorrido de alguna ruta cargada y, si se conoce la ubicación, los más cercanos primero. Los de la lista se
  comprobaron con el planificador: se puede dibujar un viaje hacia ellos desde los demás.
- **Rutas guardadas:** el icono de Rutas en la barra ya no muestra el número de guardadas.

## Diseño móvil

En pantallas de hasta 820 px la muestra usa un diseño pensado para el teléfono, inspirado en Google Maps, Uber y Didi: **el mapa ocupa toda
la pantalla y lo único que se ve encima son tarjetas de vidrio** (la misma receta de `.glass`: fondo translúcido con desenfoque) con lo esencial.
Es solo de la muestra: la app de escritorio y las pantallas anchas conservan el diseño de siempre.

| Pantalla | Qué se ve |
|---|---|
| Planificar | Una tarjeta arriba, de lado a lado, con un solo campo, "¿A dónde vas?". Al tocarlo aparecen el origen, "Elegir en el mapa" y el botón de buscar; las sugerencias salen dentro de la misma tarjeta. |
| Rutas | La lista flota **en el centro** del mapa (filtros Todas/Guardadas y las rutas; sin buscador). Al elegir una ruta la lista se cierra y se dibuja el recorrido; abajo queda una tarjeta con la ruta y "Guardar". **Tocar esa tarjeta vuelve a abrir la lista** para cambiar de ruta. |
| Comparar | Una tarjeta abajo con el origen y destino, las opciones (duración, costo, transbordos) y "Comenzar viaje". El recorrido se encuadra en la parte libre del mapa. |
| Historial | **Solo en Rutas**, en el menú de hamburguesa (botón arriba a la izquierda): toca un viaje para verlo en el mapa. Planificar ya no lo muestra. |
| Barra de pestañas | 5 destinos con icono y nombre, el activo con una píldora suave. |
| Landing | La foto de la ciudad arriba (se ajusta al alto de la pantalla); el titular, los botones y el aviso de demostración abajo, a la altura del pulgar. En pantallas muy chicas se desplaza. |

En móvil **no hay compartir** (ni el botón de compartir ni "Pegar un código"), ni "Mis lugares": solo lo esencial.
En tableta vertical (600–820 px) las tarjetas no se estiran por todo el ancho.

### Animaciones de carga

- **Al abrir la app:** pantalla de arranque con un camión que recorre la carretera y una barra con el avance real de la descarga (`--blazor-load-percentage`).
- **Al calcular un viaje:** en Comparar, un camión sobre una línea punteada y esqueletos con la forma de las opciones que van a llegar.
- **Al mostrar una ruta o un viaje:** un aviso de vidrio en el centro del mapa ("Dibujando ruta…", "Cargando viaje…") con el mismo camión.

Todas respetan "Sin animaciones" y `prefers-reduced-motion`.

Dónde está: `Estilos/CssMovil.cs` (CSS móvil, bajo `.ruta-app.muestra`), `Estilos/CssEstados.cs` (animaciones de carga), los componentes
`MenuHistorial`, `CargandoMapa` y `CargandoOpciones`, y `index.html` (arranque). `wwwroot/mapa.js` deja libres las franjas que tapan las
tarjetas cuando encuadra un recorrido.

## Cómo funciona

- `RutaApp.Interfaz` no cambia de pantallas: el servicio `ModoMuestra` (activo solo aquí) esconde lo que necesita cuenta o servidor.
- En el navegador no hay MySQL, archivos ni Windows, así que `Program.cs` cambia cada servicio por su versión web (carpeta `Servicios/`):

  | Escritorio | Muestra web |
  |---|---|
  | `RutaService` (MySQL) | `RutaServiceWeb`: lee `wwwroot/datos/rutas.json` |
  | `FavoritoService` (MySQL) | `FavoritoServiceWeb`: `localStorage` |
  | `DatosUsuarioService` (archivo por usuario) | `DatosUsuarioWeb`: `localStorage` |
  | `UbicacionWindows` | `UbicacionNavegador`: `navigator.geolocation`, con respaldo por IP |
  | `PortapapelesWindows` | `PortapapelesNavegador`: `navigator.clipboard` |

- La búsqueda de lugares, los recorridos a pie y por calles y el mapa usan los mismos servicios externos que la app (Photon, Nominatim, OSRM y OpenStreetMap); todos permiten llamadas desde el navegador.
- La ubicación solo se usa si está a menos de 80 km de Chihuahua; fuera de ahí la persona escribe su origen (las rutas son de Chihuahua).

## Probarla en tu equipo

```powershell
dotnet run --project src/Frontend/RutaApp.Web
```

## Actualizar las rutas

`wwwroot/datos/rutas.json` es una copia de las tablas `routes`, `stop`, `route_stop` y `bus` (no incluye usuarios ni comunidad).
Si cambias las rutas en tu MySQL local, vuelve a generarlo y sube el archivo:

```powershell
dotnet run --project tools/ExportarRutas -- src/Frontend/RutaApp.Web/wwwroot/datos/rutas.json
```

## Publicarla en GitHub Pages

1. En GitHub: **Settings → Pages → Build and deployment → Source: GitHub Actions** (una sola vez).
2. Sube la rama `RUTA-Muestra`. El flujo publica y deja la dirección en la pestaña **Actions** (normalmente `https://<usuario>.github.io/<repositorio>/`).
3. Si el despliegue se rechaza por la rama, agrega `RUTA-Muestra` en **Settings → Environments → github-pages → Deployment branches**.
