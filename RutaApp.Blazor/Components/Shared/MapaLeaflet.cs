using System.Net;
using Microsoft.JSInterop;

namespace RutaApp.Blazor.Components.Shared;

/// <summary>Un punto con nombre en el mapa (origen, destino, parada o escala).</summary>
public record Marcador(double Lat, double Lng, string Nombre);

/// <summary>Punto de control de una ruta oficial: se dibuja como un pin con su letra.</summary>
public record PuntoControlMapa(string Letra, string Nombre, double Lat, double Lng);

/// <summary>Un tramo del viaje: en camión (línea azul) o caminando (punteado).</summary>
public record TramoMapa(bool EsBus, List<double[]> Puntos);

/// <summary>Un punto de referencia cargado de OpenStreetMap (parada, farmacia, hospital...).</summary>
public record PuntoReferencia(double Lat, double Lng, string Nombre, string Tipo);

/// <summary>
/// Controla un mapa Leaflet desde C#. Leaflet es la única librería de JavaScript que se usa: aquí se llaman sus
/// funciones (L.map, L.marker, L.polyline...) directamente con JS interop, sin código JavaScript propio.
/// Las operaciones se hacen de una en una (un cerrojo) para que dos dibujos seguidos no se mezclen.
/// </summary>
public sealed class MapaLeaflet : IAsyncDisposable
{
    private static readonly double[] Centro = { Geo.CentroLat, Geo.CentroLng };

    private readonly IJSRuntime _js;
    private readonly string _id;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);

    private IJSObjectReference _mapa;
    private IJSObjectReference _capaRuta, _capaPuntos, _capaSugerencias;
    private readonly Dictionary<string, (IJSObjectReference Capa, double Lat, double Lng)> _ubicaciones = new();

    public MapaLeaflet(IJSRuntime js, string id)
    {
        _js = js;
        _id = id;
    }

    // ── Utilidades de JS interop ──────────────────────────────────────────
    private ValueTask<IJSObjectReference> Crear(string funcion, params object[] args) =>
        _js.InvokeAsync<IJSObjectReference>(funcion, args);

    private static async Task Soltar(params IJSObjectReference[] referencias)
    {
        foreach (var r in referencias)
            if (r != null) await r.DisposeAsync();
    }

    private async Task<T> Exclusivo<T>(Func<Task<T>> accion)
    {
        await _cerrojo.WaitAsync();
        try { return await accion(); }
        finally { _cerrojo.Release(); }
    }

    private Task Exclusivo(Func<Task> accion) => Exclusivo(async () => { await accion(); return true; });

    // ── Creación del mapa ─────────────────────────────────────────────────
    public Task Iniciar() => Exclusivo(async () =>
    {
        _mapa = await Crear("L.map", _id, new { zoomControl = false });
        await _mapa.InvokeVoidAsync("setView", Centro, 13);

        var zoom = await Crear("L.control.zoom", new { position = "topright" });
        await zoom.InvokeVoidAsync("addTo", _mapa);

        var teselas = await Crear("L.tileLayer", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", new
        {
            attribution = "&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a>",
            maxZoom = 19
        });
        await teselas.InvokeVoidAsync("addTo", _mapa);
        await Soltar(zoom, teselas);

        _capaRuta = await NuevaCapa();
        _capaPuntos = await NuevaCapa();
        _capaSugerencias = await NuevaCapa();
    });

    private async Task<IJSObjectReference> NuevaCapa()
    {
        var capa = await Crear("L.layerGroup");
        await capa.InvokeVoidAsync("addTo", _mapa);
        return capa;
    }

    /// <summary>Hay que llamarlo cuando cambia el tamaño del contenedor (por ejemplo al contraer el sidebar).</summary>
    public Task InvalidarTamano() => Exclusivo(() => _mapa.InvokeVoidAsync("invalidateSize").AsTask());

    // ── Marcadores y líneas ───────────────────────────────────────────────
    // Clases de los marcadores (definidas en app.css): m-o origen, m-d destino, m-n numerado, m-s parada del recorrido,
    // m-p parada de autobús, m-r punto de referencia.
    private static string HtmlIcono(string clase, string texto = null) => clase switch
    {
        "m-d" => "<div class=\"m-d\"><i></i></div>",
        "m-n" => "<div class=\"m-n\">" + WebUtility.HtmlEncode(texto ?? "+") + "</div>",
        _ => "<div class=\"" + clase + "\"></div>"
    };

    private async Task PonerMarcador(IJSObjectReference capa, double lat, double lng, string clase, int tam,
                                     string rotulo = null, string direccion = "top", double[] desplazamiento = null,
                                     string textoIcono = null, string popupHtml = null, bool rotuloPermanente = true)
    {
        var icono = await Crear("L.divIcon", new
        {
            className = "",
            html = HtmlIcono(clase, textoIcono),
            iconSize = new[] { tam, tam },
            iconAnchor = new[] { tam / 2.0, tam / 2.0 }
        });
        var marcador = await Crear("L.marker", new[] { lat, lng }, new { icon = icono });
        await marcador.InvokeVoidAsync("addTo", capa);

        if (rotulo != null)   // el texto se escapa: viene del usuario o de los buscadores
            await marcador.InvokeVoidAsync("bindTooltip", WebUtility.HtmlEncode(rotulo), new
            {
                permanent = rotuloPermanente,
                direction = direccion,
                offset = desplazamiento ?? new double[] { 0, 0 },
                className = "pill"
            });
        if (popupHtml != null) await marcador.InvokeVoidAsync("bindPopup", popupHtml);
        await Soltar(icono, marcador);
    }

    private async Task PonerLinea(List<double[]> puntos, object estilo)
    {
        var linea = await Crear("L.polyline", puntos, estilo);
        await linea.InvokeVoidAsync("addTo", _capaRuta);
        await Soltar(linea);
    }

    private async Task Encuadrar(IEnumerable<double[]> puntos, int relleno, int? zoomMaximo = null)
    {
        await _mapa.InvokeVoidAsync("invalidateSize");   // por si el contenedor cambió de tamaño desde que se creó
        var limites = await Crear("L.latLngBounds", puntos.ToList());
        await _mapa.InvokeVoidAsync("fitBounds", limites, new { padding = new[] { relleno, relleno }, maxZoom = zoomMaximo });
        await Soltar(limites);
    }

    private static readonly object EstiloBus = new { color = "#2563EB", weight = 5, opacity = 1, lineJoin = "round", lineCap = "round" };
    private static readonly object EstiloCaminata = new { color = "#4A6FA5", weight = 4, opacity = 0.9, dashArray = "2,9", lineCap = "round" };

    // ── Ruta ──────────────────────────────────────────────────────────────
    public Task LimpiarRuta() => Exclusivo(() => _capaRuta.InvokeVoidAsync("clearLayers").AsTask());

    /// <summary>Línea azul de la ruta con sus paradas, origen y destino.</summary>
    public Task DibujarRuta(List<double[]> linea, List<double[]> paradas, Marcador origen, Marcador destino,
                            List<PuntoControlMapa> puntosControl = null) => Exclusivo(async () =>
    {
        await _capaRuta.InvokeVoidAsync("clearLayers");
        await PonerLinea(linea, EstiloBus);
        foreach (var p in paradas) await PonerMarcador(_capaRuta, p[0], p[1], "m-s", 14);

        // Puntos de control del plano oficial (A, B, C...): pines con la letra; el nombre sale al pasar el mouse
        if (puntosControl != null)
            foreach (var c in puntosControl)
                await PonerMarcador(_capaRuta, c.Lat, c.Lng, "m-n", 22, c.Letra + " · " + c.Nombre, "top",
                                    new double[] { 0, -12 }, textoIcono: c.Letra, rotuloPermanente: false);

        await PonerMarcador(_capaRuta, origen.Lat, origen.Lng, "m-o", 22, origen.Nombre, "right", new double[] { 12, 0 });
        // En un ciclo cerrado el destino es el mismo punto que el origen: no se dibuja dos veces
        double metros = Math.Sqrt(Math.Pow((origen.Lat - destino.Lat) * 111000, 2) + Math.Pow((origen.Lng - destino.Lng) * 97500, 2));
        if (metros > 30)
            await PonerMarcador(_capaRuta, destino.Lat, destino.Lng, "m-d", 30, destino.Nombre, "top", new double[] { 0, -16 });
        await Encuadrar(linea, 110);
    });

    /// <summary>Opción de viaje: tramos caminando (punteado) y en camión (azul) con sus paradas y escalas.</summary>
    public Task DibujarViaje(List<TramoMapa> tramos, List<Marcador> paradas, Marcador origen, Marcador destino, List<Marcador> escalas) => Exclusivo(async () =>
    {
        await _capaRuta.InvokeVoidAsync("clearLayers");
        var todos = new List<double[]> { new[] { origen.Lat, origen.Lng }, new[] { destino.Lat, destino.Lng } };

        foreach (var t in tramos)
        {
            await PonerLinea(t.Puntos, t.EsBus ? EstiloBus : EstiloCaminata);
            todos.AddRange(t.Puntos);
        }
        foreach (var p in paradas)
            await PonerMarcador(_capaRuta, p.Lat, p.Lng, "m-s", 14, p.Nombre, "top", new double[] { 0, -8 });
        for (int i = 0; i < escalas.Count; i++)
        {
            var e = escalas[i];
            await PonerMarcador(_capaRuta, e.Lat, e.Lng, "m-n", 22, e.Nombre, "top", new double[] { 0, -12 }, textoIcono: (i + 1).ToString());
            todos.Add(new[] { e.Lat, e.Lng });
        }
        await PonerMarcador(_capaRuta, origen.Lat, origen.Lng, "m-o", 22, origen.Nombre, "right", new double[] { 12, 0 });
        await PonerMarcador(_capaRuta, destino.Lat, destino.Lng, "m-d", 30, destino.Nombre, "top", new double[] { 0, -16 });
        await Encuadrar(todos, 110);
    });

    // ── Ubicaciones que escribe el usuario (origen, destino y paradas intermedias) ──
    public Task PonerUbicacion(string tipo, double lat, double lng, string nombre, string etiqueta) => Exclusivo(async () =>
    {
        await QuitarCapaUbicacion(tipo);
        var capa = await Crear("L.layerGroup");
        await capa.InvokeVoidAsync("addTo", _mapa);

        bool esOrigen = tipo == "origen", esDestino = tipo == "destino";
        if (esOrigen) await PonerMarcador(capa, lat, lng, "m-o", 22, nombre, "right", new double[] { 12, 0 });
        else if (esDestino) await PonerMarcador(capa, lat, lng, "m-d", 30, nombre, "top", new double[] { 0, -16 });
        else await PonerMarcador(capa, lat, lng, "m-n", 22, nombre, "top", new double[] { 0, -12 }, textoIcono: string.IsNullOrEmpty(etiqueta) ? "+" : etiqueta);

        _ubicaciones[tipo] = (capa, lat, lng);
        await EncuadrarUbicaciones();
    });

    public Task QuitarUbicacion(string tipo) => Exclusivo(async () =>
    {
        await QuitarCapaUbicacion(tipo);
        await EncuadrarUbicaciones();
    });

    private async Task QuitarCapaUbicacion(string tipo)
    {
        if (!_ubicaciones.Remove(tipo, out var u)) return;
        await u.Capa.InvokeVoidAsync("remove");
        await Soltar(u.Capa);
    }

    private async Task EncuadrarUbicaciones()
    {
        var pts = _ubicaciones.Values.Select(u => new[] { u.Lat, u.Lng }).ToList();
        if (pts.Count == 1) await _mapa.InvokeVoidAsync("setView", pts[0], 15);
        else if (pts.Count > 1) await Encuadrar(pts, 110, 16);
    }

    // ── Sugerencias mientras el usuario escribe: pines numerados ──────────
    public Task MostrarSugerencias(List<Sugerencia> lista) => Exclusivo(async () =>
    {
        await _capaSugerencias.InvokeVoidAsync("clearLayers");
        if (lista == null || lista.Count == 0) return;

        for (int i = 0; i < lista.Count; i++)
            await PonerMarcador(_capaSugerencias, lista[i].Lat, lista[i].Lng, "m-n", 22, lista[i].Nombre, "top",
                                new double[] { 0, -10 }, textoIcono: (i + 1).ToString(), rotuloPermanente: false);

        var pts = lista.Select(s => new[] { s.Lat, s.Lng }).ToList();
        if (pts.Count == 1) await _mapa.InvokeVoidAsync("setView", pts[0], 16);
        else await Encuadrar(pts, 90, 16);
    });

    // ── Paradas y puntos de referencia ────────────────────────────────────
    public Task CargarPuntos(List<PuntoReferencia> puntos) => Exclusivo(async () =>
    {
        await _capaPuntos.InvokeVoidAsync("clearLayers");
        var emojis = new Dictionary<string, string>
        {
            ["parada"] = "🚍", ["farmacia"] = "💊", ["hospital"] = "🏥", ["escuela"] = "🏫", ["supermercado"] = "🛒"
        };
        foreach (var p in puntos)
        {
            string emoji = emojis.GetValueOrDefault(p.Tipo, "📍");
            string popup = "<b>" + WebUtility.HtmlEncode(string.IsNullOrEmpty(p.Nombre) ? p.Tipo : p.Nombre) + "</b><br>" +
                           emoji + " " + WebUtility.HtmlEncode(p.Tipo);
            await PonerMarcador(_capaPuntos, p.Lat, p.Lng, p.Tipo == "parada" ? "m-p" : "m-r", 12, popupHtml: popup);
        }
    });

    public async ValueTask DisposeAsync()
    {
        await _cerrojo.WaitAsync();
        try
        {
            if (_mapa == null) return;
            await _mapa.InvokeVoidAsync("remove");
            await Soltar(_capaRuta, _capaPuntos, _capaSugerencias, _mapa);
            foreach (var u in _ubicaciones.Values) await Soltar(u.Capa);
            _mapa = null;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or JSException) { }
        finally { _cerrojo.Release(); }
    }
}
