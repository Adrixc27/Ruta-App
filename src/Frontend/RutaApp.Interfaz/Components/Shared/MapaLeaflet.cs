using System.Net;
using Microsoft.JSInterop;

namespace RutaApp.Interfaz.Components.Shared;

// Un punto con nombre en el mapa (origen, destino, parada o escala).
public record Marcador(double Lat, double Lng, string Nombre, string Simbolo = null);

// Un tramo del viaje: en camión (línea de color) o caminando (punteado).
public record TramoMapa(bool EsBus, List<double[]> Puntos, string Color = null, string Patron = null);

// Un problema reportado por la comunidad, marcado donde estaba el camión.
public record ReporteMapa(double Lat, double Lng, string Titulo, string Detalle, string Cuando);

// Un punto de referencia cargado de OpenStreetMap (parada, farmacia, hospital...).
public record PuntoReferencia(double Lat, double Lng, string Nombre, string Tipo);

// Controla un mapa Leaflet desde C#. Todo lo que hay que dibujar viaja en UNA llamada a wwwroot/mapa.js, no una por marcador:
// con rutas largas es mucho más rápido y el mapa no se traba.
public sealed class MapaLeaflet : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly string _id;
    private bool _iniciado;

    public MapaLeaflet(IJSRuntime js, string id)
    {
        _js = js;
        _id = id;
    }

    // Descripción de lo que se envía al script (nombres en minúscula: así los lee mapa.js)
    private sealed class M
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public string html { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public double ax { get; set; }
        public double ay { get; set; }
        public string rotulo { get; set; }
        public string dir { get; set; }
        public double[] off { get; set; }
        public bool perm { get; set; }
        public string popup { get; set; }
        public int z { get; set; }
    }

    private sealed class L
    {
        public List<double[]> pts { get; set; }
        public string color { get; set; }
        public int peso { get; set; } = 6;
        public string dash { get; set; }
        public double opacidad { get; set; } = 1;
    }

    // Trazo con borde blanco (dos líneas, la blanca debajo) o sin él. Un patrón vacío es línea continua.
    private static IEnumerable<L> Linea(List<double[]> pts, string color, int peso, string patron, bool borde, double opacidad = 1)
    {
        if (borde) yield return new L { pts = pts, color = "#ffffff", peso = peso + 4, opacidad = 0.95 };
        yield return new L { pts = pts, color = color, peso = peso, dash = string.IsNullOrEmpty(patron) ? null : patron, opacidad = opacidad };
    }

    private static string Rotulo(string nombre) => string.IsNullOrWhiteSpace(nombre) ? null : WebUtility.HtmlEncode(nombre);

    private static string Svg(string icono, int tam) =>
        "<svg viewBox=\"0 0 24 24\" width=\"" + tam + "\" height=\"" + tam + "\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.4\" " +
        "stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"" + Iconos.Rutas[icono] + "\"/></svg>";

    // Pin en forma de gota: la punta marca el lugar exacto
    private static M Pin(double lat, double lng, string clase, string icono, string nombre, int z)
    {
        return new M
        {
            lat = lat, lng = lng, w = 36, h = 40, ax = 18, ay = 36, z = z,
            html = "<div class=\"mk-pin " + clase + "\"><div class=\"mk-pin-cuerpo\">" + Svg(icono, 15) + "</div></div>",
            rotulo = Rotulo(nombre), dir = "right", off = new double[] { 16, -22 }, perm = true
        };
    }

    private static M Origen(double lat, double lng, string nombre) => Pin(lat, lng, "mk-pin-o", "IcoNavigation", nombre, 600);
    private static M Destino(double lat, double lng, string nombre) => Pin(lat, lng, "mk-pin-d", "IcoFlag", nombre, 700);

    private static M Numerado(double lat, double lng, string texto, string nombre, bool permanente) => new M
    {
        lat = lat, lng = lng, w = 26, h = 26, ax = 13, ay = 13, z = 500,
        html = "<div class=\"mk-num\">" + WebUtility.HtmlEncode(texto) + "</div>",
        rotulo = Rotulo(nombre), dir = "top", off = new double[] { 0, -12 }, perm = permanente
    };

    // Parada de camión: círculo blanco con el icono del camión, del color de la ruta
    private static M Parada(double lat, double lng, string nombre, string color, bool grande, bool rotuloFijo, string simbolo = null) => new M
    {
        lat = lat, lng = lng, w = grande ? 28 : 20, h = grande ? 28 : 20, ax = grande ? 14 : 10, ay = grande ? 14 : 10, z = grande ? 450 : 300,
        html = "<div class=\"mk-parada" + (grande ? " grande" : "") + (nombre != null && nombre.Contains("zona") ? " zona" : "") + "\" style=\"--c:" + (color ?? "#2563EB") + "\">" +
               (simbolo != null ? "<span class=\"mk-simbolo\">" + WebUtility.HtmlEncode(simbolo) + "</span>" : Svg("IcoBus", grande ? 15 : 11)) + "</div>",
        rotulo = Rotulo(nombre), dir = "top", off = new double[] { 0, grande ? -14 : -10 }, perm = rotuloFijo
    };

    private static M MiPosicion(double lat, double lng) => new M
    {
        lat = lat, lng = lng, w = 26, h = 26, ax = 13, ay = 13, z = 1000,
        html = "<div class=\"mk-yo\"><i></i></div>", rotulo = Rotulo("Estás aquí"), dir = "top", off = new double[] { 0, -12 }
    };

    private static (string Icono, string Color, string Texto) Estilo(string tipo) => tipo switch
    {
        "parada" => ("IcoBus", "#CA8A04", "Parada de camión"),
        "farmacia" => ("IcoPill", "#16A34A", "Farmacia"),
        "hospital" => ("IcoHospital", "#DC2626", "Hospital"),
        "escuela" => ("IcoSchool", "#2563EB", "Escuela"),
        "supermercado" => ("IcoCart", "#EA580C", "Supermercado"),
        _ => ("IcoPin", "#0D9488", "Punto de referencia")
    };

    private static double Metros(Marcador a, Marcador b) =>
        Math.Sqrt(Math.Pow((a.Lat - b.Lat) * 111000, 2) + Math.Pow((a.Lng - b.Lng) * 97500, 2));

    private ValueTask Llamar(string funcion, params object[] args)
    {
        if (!_iniciado) return ValueTask.CompletedTask;
        var todos = new object[args.Length + 1];
        todos[0] = _id;
        Array.Copy(args, 0, todos, 1, args.Length);
        return _js.InvokeVoidAsync("rutaMapa." + funcion, todos);
    }

    public async Task Iniciar(object referencia = null)
    {
        await _js.InvokeVoidAsync("rutaMapa.iniciar", _id, referencia);
        _iniciado = true;
    }

    // Activa o desactiva que un clic en el mapa se avise a C# (para elegir un lugar tocando el mapa).
    public Task ActivarSeleccion(bool activo) => Llamar("seleccion", activo).AsTask();

    // Hay que llamarlo cuando cambia el tamaño del contenedor (por ejemplo al contraer el sidebar).
    public Task InvalidarTamano() => Llamar("invalidar").AsTask();

    public Task LimpiarRuta() => Llamar("limpiarRuta").AsTask();

    // Línea de la ruta con sus paradas (se ven al acercar el zoom), origen y destino.
    public Task DibujarRuta(List<double[]> linea, List<Marcador> paradas, Marcador origen, Marcador destino, string color, bool encuadrar = true,
                            string simbolo = null, string patron = null)
    {
        var marcadores = new List<M> { Origen(origen.Lat, origen.Lng, origen.Nombre) };
        // En un ciclo cerrado el destino es el mismo punto que el origen: no se dibuja dos veces
        if (Metros(origen, destino) > 30) marcadores.Add(Destino(destino.Lat, destino.Lng, destino.Nombre));

        return Llamar("dibujarRuta", new
        {
            lineas = Linea(linea, color, 6, patron, true).ToList(),
            paradas = paradas.Select(p => Parada(p.Lat, p.Lng, p.Nombre, color, false, false, simbolo)).ToList(),
            marcadores,
            relleno = 110,
            encuadrar
        }).AsTask();
    }

    // Opción de viaje: tramos caminando (punteado) y en camión (color de cada ruta) con sus paradas y escalas.
    public Task DibujarViaje(List<TramoMapa> tramos, List<Marcador> paradas, Marcador origen, Marcador destino, List<Marcador> escalas)
    {
        var lineas = tramos.SelectMany(t => t.EsBus
            ? Linea(t.Puntos, t.Color ?? "#2563EB", 6, t.Patron, true)
            : Linea(t.Puntos, "#475569", 5, "2,9", false, 0.95)).ToList();

        var marcadores = new List<M>();
        for (int i = 0; i < escalas.Count; i++)
            marcadores.Add(Numerado(escalas[i].Lat, escalas[i].Lng, (i + 1).ToString(), escalas[i].Nombre, true));
        marcadores.AddRange(paradas.Select(p => Parada(p.Lat, p.Lng, p.Nombre, "#2563EB", true, false, p.Simbolo)));
        marcadores.Add(Origen(origen.Lat, origen.Lng, origen.Nombre));
        marcadores.Add(Destino(destino.Lat, destino.Lng, destino.Nombre));

        return Llamar("dibujarRuta", new { lineas, paradas = new List<M>(), marcadores, relleno = 110, encuadrar = true }).AsTask();
    }

    // Ubicaciones que escribe el usuario (origen, destino y paradas intermedias)
    public Task PonerUbicacion(string tipo, double lat, double lng, string nombre, string etiqueta)
    {
        M m = tipo == "origen" ? Origen(lat, lng, nombre)
            : tipo == "destino" ? Destino(lat, lng, nombre)
            : Numerado(lat, lng, string.IsNullOrEmpty(etiqueta) ? "+" : etiqueta, nombre, true);
        return Llamar("ponerUbicacion", tipo, m).AsTask();
    }

    public Task QuitarUbicacion(string tipo) => Llamar("quitarUbicacion", tipo).AsTask();

    // Sugerencias mientras el usuario escribe: pines numerados
    public Task MostrarSugerencias(List<Sugerencia> lista)
    {
        var marcas = lista?.Select((s, i) => Numerado(s.Lat, s.Lng, (i + 1).ToString(), s.Nombre, false)).ToList();
        return Llamar("sugerencias", marcas).AsTask();
    }

    public Task CargarPuntos(List<PuntoReferencia> puntos)
    {
        var marcas = puntos.Select(p =>
        {
            var (icono, color, texto) = Estilo(p.Tipo);
            return new M
            {
                lat = p.Lat, lng = p.Lng, w = 22, h = 22, ax = 11, ay = 11, z = 200,
                html = "<div class=\"mk-poi\" style=\"--c:" + color + "\">" + Svg(icono, 12) + "</div>",
                popup = "<b>" + WebUtility.HtmlEncode(string.IsNullOrEmpty(p.Nombre) ? texto : p.Nombre) + "</b><br>" + WebUtility.HtmlEncode(texto)
            };
        }).ToList();
        return Llamar("puntos", marcas).AsTask();
    }

    // Marcas de los problemas reportados con ubicación (siempre visibles, sin importar el zoom).
    public Task MostrarReportes(List<ReporteMapa> reportes)
    {
        var marcas = reportes.Select(r => new M
        {
            lat = r.Lat, lng = r.Lng, w = 32, h = 32, ax = 16, ay = 16, z = 800,
            html = "<div class=\"mk-reporte\">" + Svg("IcoAlert", 16) + "</div>",
            popup = "<b>" + WebUtility.HtmlEncode(r.Titulo) + "</b><br>" + WebUtility.HtmlEncode(r.Detalle) + "<br><small>" + WebUtility.HtmlEncode(r.Cuando) + "</small>"
        }).ToList();
        return Llamar("reportes", marcas).AsTask();
    }

    public Task PonerPosicion(double lat, double lng, bool centrar) => Llamar("posicion", MiPosicion(lat, lng), centrar).AsTask();

    public Task Centrar(double lat, double lng) => Llamar("zoomA", lat, lng).AsTask();

    public async ValueTask DisposeAsync()
    {
        if (!_iniciado) return;
        _iniciado = false;
        try { await _js.InvokeVoidAsync("rutaMapa.quitar", _id); }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or JSException) { }
    }
}
