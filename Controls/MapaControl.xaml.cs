using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ruta_App
{
    /// <summary>
    /// Mapa Leaflet dentro de un WebView2.
    /// IMPORTANTE: un WebView2 dibuja "por encima" de cualquier control WPF, por eso el banner,
    /// la tarjeta de ruta y la leyenda están hechos en HTML dentro del propio mapa.
    /// Los botones del HTML avisan a C# con postMessage (eventos AccionXxx).
    /// </summary>
    public partial class MapaControl : UserControl
    {
        private static readonly HttpClient _http = new HttpClient();

        public event Action AccionQuitar;
        public event Action AccionPlanificar;
        public event Action AccionGuardar;

        private bool _iniciado;
        private bool _listo;
        private readonly List<string> _pendientes = new List<string>();

        static MapaControl()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
        }

        public MapaControl()
        {
            InitializeComponent();
            Loaded += async (s, e) => await Iniciar();
        }

        // ── Inicialización ────────────────────────────────────────────────────
        private async Task Iniciar()
        {
            if (_iniciado) return;
            _iniciado = true;
            await Web.EnsureCoreWebView2Async(null);

            Web.CoreWebView2.WebMessageReceived += OnMensaje;
            Web.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                _listo = true;
                foreach (var js in _pendientes) Web.CoreWebView2.ExecuteScriptAsync(js);
                _pendientes.Clear();
            };
            Web.CoreWebView2.NavigateToString(Html);
        }

        private void OnMensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();
            Action h = null;
            if (msg == "quitar") h = AccionQuitar;
            else if (msg == "planificar") h = AccionPlanificar;
            else if (msg == "guardar") h = AccionGuardar;
            if (h != null) h();
        }

        /// <summary>Ejecuta JavaScript; si la página aún no carga, lo deja en cola.</summary>
        private void Ejecutar(string js)
        {
            if (_listo) Web.CoreWebView2.ExecuteScriptAsync(js);
            else _pendientes.Add(js);
        }

        // ── Helpers para armar JavaScript ─────────────────────────────────────
        private static string J(double d) { return d.ToString("R", CultureInfo.InvariantCulture); }

        private static string S(string s)
        {
            return "'" + (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ") + "'";
        }

        // ── API pública ───────────────────────────────────────────────────────
        public void BannerTexto(string texto) { Ejecutar("bannerTexto(" + S(texto) + ");"); }

        public void BannerViaje(string titulo, string subtitulo)
        {
            Ejecutar("bannerViaje(" + S(titulo) + "," + S(subtitulo) + ");");
        }

        public void BannerRuta(string texto, bool conQuitar)
        {
            Ejecutar("bannerRuta(" + S(texto) + "," + (conQuitar ? "true" : "false") + ");");
        }

        public void OcultarBanner() { Ejecutar("ocultarBanner();"); }

        public void MostrarTarjeta(Ruta r)
        {
            var sb = new StringBuilder();
            sb.Append("mostrarTarjeta(").Append(S(r.NumeroTexto)).Append(",")
              .Append(S(r.Nombre)).Append(",").Append(S(r.Subtitulo)).Append(",")
              .Append(r.Guardada ? "true" : "false").Append(",[");
            sb.Append("[").Append(S(r.FrecuenciaMin + " min")).Append(",'Frecuencia'],");
            sb.Append("[").Append(S(r.TarifaTexto)).Append(",'Tarifa'],");
            sb.Append("[").Append(S(r.Paradas + " paradas")).Append(",'Recorrido'],");
            sb.Append("[").Append(S(r.MinutosTexto)).Append(",'Trayecto']");
            sb.Append("]);");
            Ejecutar(sb.ToString());
        }

        public void OcultarTarjeta() { Ejecutar("ocultarTarjeta();"); }

        public void LimpiarRuta() { Ejecutar("limpiarRuta();"); }

        /// <summary>Dibuja la línea azul de la ruta con sus paradas, origen y destino.</summary>
        public void DibujarRuta(Ruta r)
        {
            var pts = Geo.Camino(r.LatO, r.LngO, r.LatD, r.LngD);
            var mitad = new[] { (pts[2][0] + pts[3][0]) / 2, (pts[2][1] + pts[3][1]) / 2 };
            var paradas = new List<double[]> { pts[1], pts[2], mitad };

            var sb = new StringBuilder("dibujarRuta([");
            foreach (var p in pts) sb.Append("[").Append(J(p[0])).Append(",").Append(J(p[1])).Append("],");
            sb.Append("],[");
            foreach (var p in paradas) sb.Append("[").Append(J(p[0])).Append(",").Append(J(p[1])).Append("],");
            sb.Append("],");
            sb.Append("{lat:").Append(J(r.LatO)).Append(",lng:").Append(J(r.LngO)).Append(",n:").Append(S(r.OrigenNombre)).Append("},");
            sb.Append("{lat:").Append(J(r.LatD)).Append(",lng:").Append(J(r.LngD)).Append(",n:").Append(S(r.DestinoNombre)).Append("});");
            Ejecutar(sb.ToString());
        }

        // ── Paradas y puntos de referencia (Overpass / OpenStreetMap) ─────────
        public async Task CargarParadasAsync(double latO, double lngO, double latD, double lngD)
        {
            try
            {
                double s = Math.Min(latO, latD) - 0.005;
                double o = Math.Min(lngO, lngD) - 0.005;
                double n = Math.Max(latO, latD) + 0.005;
                double e = Math.Max(lngO, lngD) + 0.005;

                string query = string.Format(CultureInfo.InvariantCulture,
                    "[out:json][timeout:25];(" +
                    "node[highway=bus_stop]({0},{1},{2},{3});" +
                    "node[amenity=bus_station]({0},{1},{2},{3});" +
                    "node[amenity=pharmacy]({0},{1},{2},{3});" +
                    "node[amenity=hospital]({0},{1},{2},{3});" +
                    "node[amenity=school]({0},{1},{2},{3});" +
                    "node[shop=supermarket]({0},{1},{2},{3});" +
                    ");out body;",
                    s, o, n, e);

                var contenido = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("data", query)
                });
                var respuesta = await _http.PostAsync("https://overpass-api.de/api/interpreter", contenido);
                string json = await respuesta.Content.ReadAsStringAsync();

                var puntos = ParsearOverpass(json);

                var sb = new StringBuilder("cargarPuntos([");
                int max = 400; // para no saturar el mapa
                foreach (var p in puntos)
                {
                    if (max-- <= 0) break;
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "{{lat:{0},lng:{1},nombre:{2},tipo:'{3}'}},", p.Lat, p.Lng, S(p.Nombre), p.Tipo);
                }
                sb.Append("]);");
                Ejecutar(sb.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Overpass error: " + ex.Message);
            }
        }

        // Parser JSON de Overpass sin dependencias externas (el mismo de tu versión anterior)
        private List<PuntoMapa> ParsearOverpass(string json)
        {
            var lista = new List<PuntoMapa>();
            try
            {
                var bloques = json.Split(new[] { "{\"type\":\"node\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var bloque in bloques)
                {
                    var mLat = Regex.Match(bloque, "\"lat\":([0-9.\\-]+)");
                    var mLng = Regex.Match(bloque, "\"lon\":([0-9.\\-]+)");
                    if (!mLat.Success || !mLng.Success) continue;

                    double lat = double.Parse(mLat.Groups[1].Value, CultureInfo.InvariantCulture);
                    double lng = double.Parse(mLng.Groups[1].Value, CultureInfo.InvariantCulture);
                    var mName = Regex.Match(bloque, "\"name\":\"([^\"]+)\"");
                    string nombre = mName.Success ? mName.Groups[1].Value : "";

                    string tipo;
                    if (bloque.Contains("\"highway\":\"bus_stop\"") || bloque.Contains("\"amenity\":\"bus_station\""))
                        tipo = "parada";
                    else if (bloque.Contains("\"amenity\":\"pharmacy\"")) tipo = "farmacia";
                    else if (bloque.Contains("\"amenity\":\"hospital\"")) tipo = "hospital";
                    else if (bloque.Contains("\"amenity\":\"school\"")) tipo = "escuela";
                    else if (bloque.Contains("\"shop\":\"supermarket\"")) tipo = "supermercado";
                    else continue;

                    lista.Add(new PuntoMapa { Lat = lat, Lng = lng, Nombre = nombre, Tipo = tipo });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Parser error: " + ex.Message);
            }
            return lista;
        }

        private class PuntoMapa
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string Nombre { get; set; } = "";
            public string Tipo { get; set; } = "";
        }

        // ── Página HTML del mapa (todo con comillas simples para no escapar en C#) ──
        private const string Html = @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<style>
  * { margin:0; padding:0; box-sizing:border-box; }
  html, body, #map { width:100%; height:100%; background:#C4DBF0; font-family:'Source Sans Pro','Segoe UI',sans-serif; }
  /* Tinte azulado del mapa: ajusta estos valores a tu gusto */
  .leaflet-tile-pane { filter: sepia(.25) hue-rotate(170deg) saturate(1.5); }
  .leaflet-control-zoom a { color:#1B365D !important; border-color:#B0CDEA !important; }
  .leaflet-control-attribution { font-size:10px; }

  .glass { position:absolute; z-index:1000; background:rgba(230,240,250,.94); backdrop-filter:blur(10px);
           border:1px solid #B0CDEA; color:#1B365D; }

  #banner { top:24px; left:24px; border-radius:12px; padding:12px 20px; display:none; align-items:center; gap:12px;
            font-size:14px; font-weight:600; }
  #banner .circ { width:40px; height:40px; border-radius:50%; border:3px solid #2563EB; background:#fff; flex:none; }
  #banner .bv { display:flex; flex-direction:column; }
  #banner .bv b { font-size:14px; font-weight:800; }
  #banner .bv small { font-size:12px; font-weight:400; color:#4A6FA5; }
  #banner .dot { width:10px; height:10px; border-radius:50%; background:#2563EB; flex:none; }
  #banner a { color:#4A6FA5; font-size:12px; font-weight:700; cursor:pointer; margin-left:8px; }
  #banner a:hover { color:#2563EB; }

  #leyenda { right:24px; bottom:28px; width:180px; border-radius:16px; padding:16px; }
  #leyenda .lt { font-size:12px; font-weight:700; color:#4A6FA5; margin-bottom:12px; }
  #leyenda .li { display:flex; align-items:center; gap:8px; font-size:13px; margin-bottom:8px; }
  #leyenda .li:last-child { margin-bottom:0; }
  #leyenda i { width:10px; height:10px; border-radius:50%; display:block; }

  #tarjeta { left:24px; bottom:28px; width:470px; border-radius:16px; padding:19px; display:none;
             background:rgba(244,248,253,.97); }
  #tarjeta .ch { display:flex; align-items:center; gap:12px; }
  #tarjeta .bn { width:38px; height:38px; border-radius:10px; background:#2563EB; color:#fff; font-weight:800;
                 font-size:17px; display:flex; align-items:center; justify-content:center; flex:none; }
  #tarjeta .ct { flex:1; display:flex; flex-direction:column; }
  #tarjeta .ct b { font-size:16px; font-weight:800; }
  #tarjeta .ct small { font-size:12px; color:#4A6FA5; }
  #tarjeta .bg { font-size:12px; font-weight:700; color:#2563EB; background:#DBEAFE; border-radius:8px;
                 padding:7px 10px; cursor:pointer; user-select:none; }
  #tarjeta .bg.off { background:#fff; color:#4A6FA5; border:1px solid #B0CDEA; }
  #tarjeta .stats { display:flex; gap:28px; margin:16px 0; }
  #tarjeta .st { display:flex; flex-direction:column; }
  #tarjeta .st b { font-size:15px; font-weight:800; }
  #tarjeta .st small { font-size:11px; color:#4A6FA5; }
  #tarjeta .btnp { display:block; text-align:center; background:#2563EB; color:#fff; font-weight:700; font-size:14px;
                   padding:12px; border-radius:12px; cursor:pointer; user-select:none; }
  #tarjeta .btnp:hover { opacity:.9; }

  .m-o { width:22px; height:22px; border-radius:50%; background:#F9A8A8; border:3px solid #fff;
         box-shadow:0 2px 6px rgba(0,0,0,.25); }
  .m-d { width:30px; height:30px; border-radius:50%; background:#fff; border:4px solid #2563EB;
         display:flex; align-items:center; justify-content:center; }
  .m-d i { width:12px; height:12px; border-radius:50%; background:#2563EB; display:block; }
  .m-s { width:14px; height:14px; border-radius:50%; background:#fff; border:3px solid #2563EB; }
  .m-p { width:12px; height:12px; border-radius:50%; background:#FACC15; border:2px solid #fff; box-shadow:0 1px 4px rgba(0,0,0,.25); }
  .m-r { width:12px; height:12px; border-radius:50%; background:#5EEAD4; border:2px solid #fff; box-shadow:0 1px 4px rgba(0,0,0,.25); }
  .leaflet-tooltip.pill { background:#fff; border:0; border-radius:8px; color:#2563EB; font-weight:700; font-size:11px;
                          padding:4px 10px; box-shadow:0 2px 8px rgba(27,54,93,.18); }
  .leaflet-tooltip.pill:before { display:none; }
  .leaflet-popup-content-wrapper { border-radius:10px; color:#1B365D; }
</style>
</head>
<body>
<div id='map'></div>
<div id='banner' class='glass'></div>
<div id='tarjeta' class='glass'></div>
<div id='leyenda' class='glass'>
  <div class='lt'>LEYENDA</div>
  <div class='li'><i style='background:#F9A8A8'></i>Origen</div>
  <div class='li'><i style='background:#2563EB'></i>Destino</div>
  <div class='li'><i style='background:#FACC15'></i>Parada de autobús</div>
  <div class='li'><i style='background:#5EEAD4'></i>Punto de referencia</div>
</div>

<script>
  var mapa = L.map('map', { zoomControl:false }).setView([28.6353, -106.0889], 13);
  L.control.zoom({ position:'topright' }).addTo(mapa);
  L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
    attribution:'&copy; OpenStreetMap &copy; CARTO', subdomains:'abcd', maxZoom:19
  }).addTo(mapa);

  var capaRuta = L.layerGroup().addTo(mapa);
  var capaPuntos = L.layerGroup().addTo(mapa);

  function esc(t) { var d = document.createElement('div'); d.textContent = (t == null ? '' : t); return d.innerHTML; }
  function post(m) { if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(m); }
  function quitar() { post('quitar'); }
  function planificar() { post('planificar'); }
  function guardar() { post('guardar'); }

  function icono(cls, tam) {
    var html = (cls === 'm-d') ? '<div class=m-d><i></i></div>' : '<div class=' + cls + '></div>';
    return L.divIcon({ className:'', html:html, iconSize:[tam, tam], iconAnchor:[tam/2, tam/2] });
  }

  // ── Banner superior izquierdo ──
  function bannerTexto(t) {
    var b = document.getElementById('banner');
    b.innerHTML = '<span>' + esc(t) + '</span>'; b.style.display = 'flex';
  }
  function bannerViaje(t, s) {
    var b = document.getElementById('banner');
    b.innerHTML = '<div class=circ></div><div class=bv><b>' + esc(t) + '</b><small>' + esc(s) + '</small></div>';
    b.style.display = 'flex';
  }
  function bannerRuta(t, q) {
    var b = document.getElementById('banner');
    b.innerHTML = '<i class=dot></i><span>' + esc(t) + '</span>' + (q ? '<a onclick=quitar()>&#10005; Quitar</a>' : '');
    b.style.display = 'flex';
  }
  function ocultarBanner() { document.getElementById('banner').style.display = 'none'; }

  // ── Tarjeta inferior de la ruta seleccionada ──
  function mostrarTarjeta(num, titulo, sub, guardada, stats) {
    var h = '<div class=ch><div class=bn>' + esc(num) + '</div>'
          + '<div class=ct><b>' + esc(titulo) + '</b><small>' + esc(sub) + '</small></div>'
          + '<a class=\'bg' + (guardada ? '' : ' off') + '\' onclick=guardar()>' + (guardada ? 'Guardada' : 'Guardar') + '</a></div>'
          + '<div class=stats>';
    stats.forEach(function (s) { h += '<div class=st><b>' + esc(s[0]) + '</b><small>' + esc(s[1]) + '</small></div>'; });
    h += '</div><a class=btnp onclick=planificar()>Planificar viaje con esta ruta &rarr;</a>';
    var t = document.getElementById('tarjeta');
    t.innerHTML = h; t.style.display = 'block';
  }
  function ocultarTarjeta() { document.getElementById('tarjeta').style.display = 'none'; }

  // ── Ruta ──
  function limpiarRuta() { capaRuta.clearLayers(); }

  function dibujarRuta(linea, paradas, o, d) {
    capaRuta.clearLayers();
    L.polyline(linea, { color:'#2563EB', weight:5, opacity:1, lineJoin:'round', lineCap:'round' }).addTo(capaRuta);
    paradas.forEach(function (p) { L.marker(p, { icon: icono('m-s', 14) }).addTo(capaRuta); });
    L.marker([o.lat, o.lng], { icon: icono('m-o', 22) }).addTo(capaRuta)
      .bindTooltip(o.n, { permanent:true, direction:'right', offset:[12, 0], className:'pill' });
    L.marker([d.lat, d.lng], { icon: icono('m-d', 30) }).addTo(capaRuta)
      .bindTooltip(d.n, { permanent:true, direction:'top', offset:[0, -16], className:'pill' });
    mapa.fitBounds(L.latLngBounds(linea), { padding:[110, 110] });
  }

  // ── Paradas y puntos de referencia ──
  function cargarPuntos(puntos) {
    capaPuntos.clearLayers();
    var emojis = { parada:'🚍', farmacia:'💊', hospital:'🏥', escuela:'🏫', supermercado:'🛒' };
    puntos.forEach(function (p) {
      var esParada = p.tipo === 'parada';
      var e = emojis[p.tipo] || '📍';
      var pop = '<b>' + esc(p.nombre || p.tipo) + '</b><br>' + e + ' ' + esc(p.tipo);
      L.marker([p.lat, p.lng], { icon: icono(esParada ? 'm-p' : 'm-r', 12) }).addTo(capaPuntos).bindPopup(pop);
    });
  }
</script>
</body>
</html>";
    }
}
