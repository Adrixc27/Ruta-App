using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ruta_App
{
    public class Sugerencia
    {
        public string Numero { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Detalle { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

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

        // ── Pop-ups encima del mapa ───────────────────────────────────────────
        // WebView2 siempre se dibuja por encima de los controles de WPF. Para mostrar un pop-up sobre el mapa,
        // se cambia el mapa por una foto suya mientras el pop-up está abierto.
        public async Task CongelarAsync()
        {
            try
            {
                if (!IsVisible || !_listo || Web.CoreWebView2 == null || Web.Visibility != System.Windows.Visibility.Visible) return;   // solo los mapas que se ven
                using (var ms = new System.IO.MemoryStream())
                {
                    await Web.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, ms);
                    ms.Position = 0;
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    img.Freeze();
                    Foto.Source = img;
                }
                Foto.Visibility = System.Windows.Visibility.Visible;
                Web.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Congelar mapa: " + ex.Message);
                Web.Visibility = System.Windows.Visibility.Hidden;   // al menos que no tape el pop-up
            }
        }

        public void Descongelar()
        {
            Web.Visibility = System.Windows.Visibility.Visible;
            Foto.Visibility = System.Windows.Visibility.Collapsed;
            Foto.Source = null;
        }

        // ── Inicialización ────────────────────────────────────────────────────
        private async Task Iniciar()
        {
            if (_iniciado) return;
            _iniciado = true;
            try
            {
                // Carpeta de datos propia y escribible (por defecto WebView2 usa la del .exe, que puede fallar)
                string baseDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp");
                string datos = System.IO.Path.Combine(baseDir, "WebView2");
                string web = System.IO.Path.Combine(baseDir, "web");
                System.IO.Directory.CreateDirectory(datos);
                System.IO.Directory.CreateDirectory(web);
                System.IO.File.WriteAllText(System.IO.Path.Combine(web, "mapa.html"), Html, new UTF8Encoding(false));

                var entorno = await CoreWebView2Environment.CreateAsync(null, datos);
                await Web.EnsureCoreWebView2Async(entorno);

                Web.CoreWebView2.WebMessageReceived += OnMensaje;
                // La página del mapa puede pedir la ubicación del equipo ("Mi ubicación actual")
                Web.CoreWebView2.PermissionRequested += (s, e) =>
                {
                    if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
                        e.State = CoreWebView2PermissionState.Allow;
                };
                Web.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    if (!e.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("Mapa: fallo de navegación " + e.WebErrorStatus);
                        return;
                    }
                    _listo = true;
                    foreach (var js in _pendientes) Web.CoreWebView2.ExecuteScriptAsync(js);
                    _pendientes.Clear();
                };

                // Origen https real (con Referer): los servidores de teselas rechazan peticiones sin él
                Web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "ruta.app", web, CoreWebView2HostResourceAccessKind.Allow);
                Web.CoreWebView2.Navigate("https://ruta.app/mapa.html");

                // Calienta las conexiones del buscador para que la primera búsqueda del usuario no tarde
                var _ = BuscadorLugares.BuscarAsync(_http, "plaza");
            }
            catch (Exception ex)
            {
                _iniciado = false;
                System.Diagnostics.Debug.WriteLine("Mapa: " + ex);
                System.Windows.MessageBox.Show(
                    "No se pudo iniciar el mapa:\n\n" + ex.Message +
                    "\n\nVerifica que esté instalado el WebView2 Runtime de Microsoft.",
                    "Mapa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void OnMensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();
            if (msg != null && msg.StartsWith("miubic:"))
            {
                double lat, lng;
                var tcs = _miUbicacion;
                var partes = msg.Substring(7).Split(',');
                if (tcs != null && partes.Length == 2 &&
                    double.TryParse(partes[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat) &&
                    double.TryParse(partes[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lng))
                    tcs.TrySetResult(new[] { lat, lng });
                else if (tcs != null)
                    tcs.TrySetResult(null);
                return;
            }
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

        private int _versionRuta;
        private static readonly Dictionary<string, List<double[]>> _cacheCalles = new Dictionary<string, List<double[]>>();

        /// <summary>Dibuja la línea azul de la ruta con sus paradas, origen y destino.</summary>
        public void DibujarRuta(Ruta r)
        {
            // Primero un trazo rápido; en cuanto OSRM responde se reemplaza por el que sigue las calles
            if (r.EsViaje) { DibujarViaje(r); return; }
            if (r.Trazo != null && r.Trazo.Count >= 2) { DibujarLinea(r, r.Trazo); return; }   // recorrido propio de la ruta

            DibujarLinea(r, Geo.Camino(r.LatO, r.LngO, r.LatD, r.LngD));
            var _ = DibujarPorCallesAsync(r, ++_versionRuta);
        }

        private async Task DibujarPorCallesAsync(Ruta r, int version)
        {
            try
            {
                string clave = string.Format(CultureInfo.InvariantCulture, "{0},{1};{2},{3}", r.LngO, r.LatO, r.LngD, r.LatD);
                List<double[]> linea;
                if (!_cacheCalles.TryGetValue(clave, out linea))
                {
                    string url = "https://router.project-osrm.org/route/v1/driving/" + clave +
                                 "?overview=full&geometries=geojson";
                    string json = await _http.GetStringAsync(url);
                    linea = ParsearOsrm(json);
                    if (linea == null || linea.Count < 2) return;
                    _cacheCalles[clave] = linea;
                }
                if (version != _versionRuta) return;   // el usuario ya eligió otra ruta
                DibujarLinea(r, linea);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OSRM error: " + ex.Message);   // se queda el trazo rápido
            }
        }

        // Respuesta de OSRM: "coordinates":[[lng,lat],[lng,lat],...]
        private static List<double[]> ParsearOsrm(string json)
        {
            using (var doc = System.Text.Json.JsonDocument.Parse(json))
            {
                var raiz = doc.RootElement;
                if (raiz.GetProperty("code").GetString() != "Ok") return null;
                var coords = raiz.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates");
                var lista = new List<double[]>();
                foreach (var c in coords.EnumerateArray())
                    lista.Add(new[] { c[1].GetDouble(), c[0].GetDouble() });   // Leaflet usa [lat, lng]
                return lista;
            }
        }

        /// <summary>Opción de viaje: tramos caminando (punteado) y en camión (azul) con sus paradas.</summary>
        private void DibujarViaje(Ruta r)
        {
            var tramos = r.Tramos;
            if (tramos == null || tramos.Count == 0) return;

            var o = tramos[0].Puntos[0];
            var ult = tramos[tramos.Count - 1].Puntos;
            var d = ult[ult.Count - 1];

            var sb = new StringBuilder("dibujarViaje([");
            foreach (var tr in tramos)
            {
                sb.Append("[").Append(tr.EsBus ? "1" : "0").Append(",[");
                foreach (var p in tr.Puntos) sb.Append("[").Append(J(p[0])).Append(",").Append(J(p[1])).Append("],");
                sb.Append("]],");
            }
            sb.Append("],[");
            foreach (var tr in tramos)
            {
                if (!tr.EsBus) continue;
                var a = tr.Puntos[0]; var b = tr.Puntos[tr.Puntos.Count - 1];
                if (tr.TextoInicio != null)
                    sb.Append("[").Append(J(a[0])).Append(",").Append(J(a[1])).Append(",").Append(S(tr.TextoInicio)).Append("],");
                if (tr.TextoFin != null)
                    sb.Append("[").Append(J(b[0])).Append(",").Append(J(b[1])).Append(",").Append(S(tr.TextoFin)).Append("],");
            }
            sb.Append("],");
            sb.Append("{lat:").Append(J(o[0])).Append(",lng:").Append(J(o[1])).Append(",n:").Append(S(Estado.Origen)).Append("},");
            sb.Append("{lat:").Append(J(d[0])).Append(",lng:").Append(J(d[1])).Append(",n:").Append(S(Estado.Destino)).Append("},[");
            if (r.Escalas != null)
                foreach (var e in r.Escalas)
                    sb.Append("{lat:").Append(J(e.Lat)).Append(",lng:").Append(J(e.Lng)).Append(",n:").Append(S(e.Nombre)).Append("},");
            sb.Append("]);");
            Ejecutar(sb.ToString());
        }

        /// <summary>Última ubicación del equipo obtenida (null si aún no se pide o no se pudo).</summary>
        public double[] MiPosicion { get { return _miUbicacionCache; } }

        private void DibujarLinea(Ruta r, List<double[]> pts)
        {
            // Tres paradas repartidas a lo largo del recorrido (25 %, 50 % y 75 %)
            var paradas = new List<double[]>();
            foreach (var f in new[] { 0.25, 0.5, 0.75 })
                paradas.Add(pts[(int)((pts.Count - 1) * f)]);

            var ini = pts[0];
            var fin = pts[pts.Count - 1];

            var sb = new StringBuilder("dibujarRuta([");
            foreach (var p in pts) sb.Append("[").Append(J(p[0])).Append(",").Append(J(p[1])).Append("],");
            sb.Append("],[");
            foreach (var p in paradas) sb.Append("[").Append(J(p[0])).Append(",").Append(J(p[1])).Append("],");
            sb.Append("],");
            sb.Append("{lat:").Append(J(ini[0])).Append(",lng:").Append(J(ini[1])).Append(",n:").Append(S(r.OrigenNombre)).Append("},");
            sb.Append("{lat:").Append(J(fin[0])).Append(",lng:").Append(J(fin[1])).Append(",n:").Append(S(r.DestinoNombre)).Append("});");
            Ejecutar(sb.ToString());
        }

        // ── Ubicaciones escritas por el usuario (solo ciudad de Chihuahua) ────
        private readonly Dictionary<string, int> _versionUbic = new Dictionary<string, int>();

        /// <summary>
        /// Busca hasta 6 lugares de la ciudad de Chihuahua que coincidan con el texto (aunque esté abreviado, a medias
        /// o sea el nombre de una calle) y los marca en el mapa con pines numerados.
        /// Devuelve null si el usuario siguió escribiendo (resultado descartado) o la búsqueda falló.
        /// </summary>
        public async Task<List<Sugerencia>> BuscarSugerenciasAsync(string tipo, string texto)
        {
            int version;
            _versionUbic.TryGetValue(tipo, out version);
            _versionUbic[tipo] = ++version;

            texto = (texto ?? "").Trim();
            if (texto.Length < 2)
            {
                Ejecutar("mostrarSugerencias([]);");
                return new List<Sugerencia>();
            }

            try
            {
                var lista = await BuscadorLugares.BuscarAsync(_http, texto);

                if (_versionUbic[tipo] != version) return null;   // el usuario siguió escribiendo

                var sb = new StringBuilder("mostrarSugerencias([");
                foreach (var s in lista)
                    sb.Append("[").Append(J(s.Lat)).Append(",").Append(J(s.Lng)).Append(",").Append(S(s.Nombre)).Append("],");
                sb.Append("]);");
                Ejecutar(sb.ToString());
                return lista;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Búsqueda de lugares: " + ex.Message);
                return null;
            }
        }

        /// <summary>Marca la ubicación elegida (origen o destino) y quita los pines de sugerencias.</summary>
        public void ElegirUbicacion(string tipo, string nombre, double lat, double lng, string etiqueta = "")
        {
            Ejecutar("mostrarSugerencias([]);ponerUbicacion(" + S(tipo) + "," + J(lat) + "," + J(lng) + "," + S(nombre) + "," + S(etiqueta) + ");");
        }

        public void QuitarUbicacion(string tipo)
        {
            Ejecutar("quitarUbicacion(" + S(tipo) + ");");
        }

        public void LimpiarSugerencias() { Ejecutar("mostrarSugerencias([]);"); }

        // ── "Mi ubicación actual" ─────────────────────────────────────────────
        private TaskCompletionSource<double[]> _miUbicacion;
        private double[] _miUbicacionCache;

        /// <summary>
        /// Obtiene la ubicación del equipo (servicio de ubicación de Windows) y la marca como origen.
        /// Si Windows no la da, usa una ubicación aproximada por IP. Devuelve false si no se pudo.
        /// </summary>
        public async Task<bool> MostrarMiUbicacionAsync()
        {
            double[] pos = _miUbicacionCache;
            if (pos == null)
            {
                _miUbicacion = new TaskCompletionSource<double[]>();
                Ejecutar("miUbicacion();");
                var espera = await Task.WhenAny(_miUbicacion.Task, Task.Delay(12000));
                pos = espera == _miUbicacion.Task ? _miUbicacion.Task.Result : null;
                if (pos == null) pos = await UbicacionPorIpAsync();
                _miUbicacionCache = pos;
            }
            if (pos == null) return false;

            Ejecutar("ponerUbicacion('origen'," + J(pos[0]) + "," + J(pos[1]) + ",'Mi ubicación actual');");
            return true;
        }

        private static async Task<double[]> UbicacionPorIpAsync()
        {
            try
            {
                string json = await _http.GetStringAsync("https://ipwho.is/");
                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    var r = doc.RootElement;
                    if (!r.GetProperty("success").GetBoolean()) return null;
                    return new[] { r.GetProperty("latitude").GetDouble(), r.GetProperty("longitude").GetDouble() };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ubicación por IP: " + ex.Message);
                return null;
            }
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
                int max = 80; // pocos puntos: mapa limpio
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
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'
        onerror=""document.title='ERROR';document.addEventListener('DOMContentLoaded',function(){document.body.innerHTML='<p style=\'padding:24px;font:14px Segoe UI;color:#1B365D\'>No se pudo cargar Leaflet (unpkg.com). Revisa tu conexión a internet.</p>';});""></script>
<style>
  * { margin:0; padding:0; box-sizing:border-box; }
  html, body, #map { width:100%; height:100%; background:#C4DBF0; font-family:'Source Sans Pro','Segoe UI',sans-serif; }
  /* Tinte azulado del mapa: ajusta estos valores a tu gusto */
  .leaflet-tile-pane { filter: grayscale(1) contrast(.6) brightness(1.18) sepia(.15) hue-rotate(185deg) saturate(1.6); opacity:.9; }
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
  .m-n { width:22px; height:22px; border-radius:50%; background:#1B365D; color:#fff; font-weight:800; font-size:11px;
         border:2px solid #fff; box-shadow:0 2px 6px rgba(0,0,0,.3); display:flex; align-items:center; justify-content:center; }
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
  L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution:'&copy; <a href=""https://www.openstreetmap.org/copyright"">OpenStreetMap</a>', maxZoom:19
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

  // ── Ubicaciones que escribe el usuario (origen y destino) ──
  var capaUbic = L.layerGroup().addTo(mapa);
  var ubic = { origen:null, destino:null };

  function ponerUbicacion(tipo, lat, lng, nombre, etq) {
    if (ubic[tipo]) { capaUbic.removeLayer(ubic[tipo]); ubic[tipo] = null; }
    var esO = tipo === 'origen', esD = tipo === 'destino';
    var ic = esO ? icono('m-o', 22) : (esD ? icono('m-d', 30)
      : L.divIcon({ className:'', html:'<div class=m-n>' + (etq || '+') + '</div>', iconSize:[22, 22], iconAnchor:[11, 11] }));
    ubic[tipo] = L.marker([lat, lng], { icon: ic }).addTo(capaUbic)
      .bindTooltip(nombre, { permanent:true, direction: esO ? 'right' : 'top', offset: esO ? [12, 0] : [0, esD ? -16 : -12], className:'pill' });
    encuadrarUbic();
  }
  function quitarUbicacion(tipo) {
    if (ubic[tipo]) { capaUbic.removeLayer(ubic[tipo]); ubic[tipo] = null; }
    encuadrarUbic();
  }
  function encuadrarUbic() {
    var pts = [];
    for (var k in ubic) { if (ubic[k]) pts.push(ubic[k].getLatLng()); }
    if (pts.length === 1) mapa.setView(pts[0], 15);
    else if (pts.length > 1) mapa.fitBounds(L.latLngBounds(pts), { padding:[110, 110], maxZoom:16 });
  }

  // ── Sugerencias mientras el usuario escribe: pines numerados ──
  var capaSug = L.layerGroup().addTo(mapa);
  function mostrarSugerencias(lista) {
    capaSug.clearLayers();
    if (!lista.length) return;
    var pts = [];
    lista.forEach(function (s, i) {
      pts.push([s[0], s[1]]);
      L.marker([s[0], s[1]], { icon: L.divIcon({ className:'', html:'<div class=m-n>' + (i + 1) + '</div>', iconSize:[22, 22], iconAnchor:[11, 11] }) })
        .addTo(capaSug).bindTooltip(s[2], { direction:'top', offset:[0, -10], className:'pill' });
    });
    if (pts.length === 1) mapa.setView(pts[0], 16);
    else mapa.fitBounds(L.latLngBounds(pts), { padding:[90, 90], maxZoom:16 });
  }

  // ── Mi ubicación (servicio de ubicación de Windows) ──
  function miUbicacion() {
    if (!navigator.geolocation) { post('miubic:error'); return; }
    navigator.geolocation.getCurrentPosition(
      function (p) { post('miubic:' + p.coords.latitude + ',' + p.coords.longitude); },
      function () { post('miubic:error'); },
      { enableHighAccuracy:true, timeout:8000 });
  }

  function dibujarViaje(tramos, paradas, o, d, escalas) {
    capaRuta.clearLayers();
    var todos = [[o.lat, o.lng], [d.lat, d.lng]];
    tramos.forEach(function (t) {
      var estilo = t[0]
        ? { color:'#2563EB', weight:5, opacity:1, lineJoin:'round', lineCap:'round' }
        : { color:'#4A6FA5', weight:4, opacity:.9, dashArray:'2,9', lineCap:'round' };
      L.polyline(t[1], estilo).addTo(capaRuta);
      t[1].forEach(function (p) { todos.push(p); });
    });
    paradas.forEach(function (p) {
      L.marker([p[0], p[1]], { icon: icono('m-s', 14) }).addTo(capaRuta)
        .bindTooltip(p[2], { permanent:true, direction:'top', offset:[0, -8], className:'pill' });
    });
    escalas.forEach(function (e, i) {
      L.marker([e.lat, e.lng], { icon: L.divIcon({ className:'', html:'<div class=m-n>' + (i + 1) + '</div>', iconSize:[22, 22], iconAnchor:[11, 11] }) })
        .addTo(capaRuta).bindTooltip(e.n, { permanent:true, direction:'top', offset:[0, -12], className:'pill' });
      todos.push([e.lat, e.lng]);
    });
    L.marker([o.lat, o.lng], { icon: icono('m-o', 22) }).addTo(capaRuta)
      .bindTooltip(o.n, { permanent:true, direction:'right', offset:[12, 0], className:'pill' });
    L.marker([d.lat, d.lng], { icon: icono('m-d', 30) }).addTo(capaRuta)
      .bindTooltip(d.n, { permanent:true, direction:'top', offset:[0, -16], className:'pill' });
    mapa.fitBounds(L.latLngBounds(todos), { padding:[110, 110] });
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

    /// <summary>
    /// Buscador de lugares limitado a la ciudad de Chihuahua. Combina Photon (busca mientras escribes, sirve para
    /// nombres cortos o incompletos) con Nominatim (direcciones y calles), ambos de OpenStreetMap y sin API key.
    /// Entiende abreviaturas ("av", "blvd", "col", "fracc", "uach"...) y no devuelve nada fuera de la ciudad.
    /// </summary>
    internal static class BuscadorLugares
    {
        // Mancha urbana de la ciudad de Chihuahua (los resultados fuera de este rectángulo se descartan)
        public const double MinLon = -106.22, MaxLon = -105.97, MinLat = 28.52, MaxLat = 28.80;
        public const double CentroLat = 28.6353, CentroLng = -106.0889;

        private static readonly Dictionary<string, string> Abreviaturas = new Dictionary<string, string>
        {
            { "av", "avenida" }, { "avda", "avenida" }, { "ave", "avenida" },
            { "blvd", "boulevard" }, { "bvd", "boulevard" }, { "bulevar", "boulevard" }, { "blv", "boulevard" },
            { "calz", "calzada" }, { "col", "colonia" }, { "fracc", "fraccionamiento" }, { "frac", "fraccionamiento" },
            { "ctra", "carretera" }, { "carr", "carretera" }, { "prol", "prolongacion" }, { "pza", "plaza" },
            { "hosp", "hospital" }, { "univ", "universidad" }, { "sta", "santa" }, { "sto", "santo" },
            { "dr", "doctor" }, { "gral", "general" }, { "cd", "ciudad" }, { "mpio", "municipio" },
            { "perif", "periferico" }, { "peri", "periferico" }, { "priv", "privada" }, { "cto", "circuito" }
        };

        // Siglas y nombres que la gente escribe cortos
        private static readonly Dictionary<string, string> Alias = new Dictionary<string, string>
        {
            { "uach", "Universidad Autonoma de Chihuahua" },
            { "uacj", "Universidad Autonoma de Ciudad Juarez" },
            { "itch", "Instituto Tecnologico de Chihuahua" },
            { "tec", "Instituto Tecnologico de Chihuahua" },
            { "uvm", "Universidad del Valle de Mexico" },
            { "imss", "IMSS" }, { "issste", "ISSSTE" },
            { "central", "Central de Autobuses" },
            { "terminal", "Central de Autobuses" },
            { "aeropuerto", "Aeropuerto Internacional General Roberto Fierro Villalobos" },
            { "catedral", "Catedral Metropolitana de Chihuahua" },
            { "plaza mayor", "Plaza de Armas|Plaza Mayor" },
            { "centro", "Plaza de Armas|Catedral Metropolitana de Chihuahua|Centro Historico" },
            { "palacio", "Palacio de Gobierno" },
            { "palomar", "Parque El Palomar" },
            { "lerdo", "Parque Lerdo" },
            { "galerias", "Galerias Chihuahua" },
            { "sendero", "Plaza Sendero" },
            { "soriana", "Soriana" }, { "walmart", "Walmart" }, { "oxxo", "OXXO" }
        };

        /// <summary>Minúsculas y sin acentos ni signos, para comparar textos.</summary>
        public static string Normalizar(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            return Regex.Replace(sb.ToString(), "\\s+", " ").Trim();
        }

        /// <summary>"av independencia" → "avenida independencia"; "uach" → "Universidad Autonoma de Chihuahua".</summary>
        public static string Expandir(string texto)
        {
            string n = Normalizar(texto);
            string alias;
            if (Alias.TryGetValue(n, out alias)) return alias.Split('|')[0];

            var tokens = n.Split(' ');
            for (int i = 0; i < tokens.Length; i++)
            {
                string t = tokens[i], ext;
                if (t == "c" && i == 0 && tokens.Length > 1) tokens[i] = "calle";
                else if (Abreviaturas.TryGetValue(t, out ext)) tokens[i] = ext;
            }
            return string.Join(" ", tokens);
        }

        private static readonly string[] TiposDeCalle =
            { "calle", "avenida", "boulevard", "calzada", "privada", "paseo", "periferico", "prolongacion", "circuito", "carretera", "andador", "callejon", "cerrada", "retorno" };

        private class Candidato
        {
            public string Nombre, Detalle;
            public double Lat, Lng;
        }

        public static async Task<List<Sugerencia>> BuscarAsync(HttpClient http, string texto, int maximo = 6)
        {
            texto = (texto ?? "").Trim();
            string expandido = Expandir(texto);

            var consultas = new List<string> { texto };
            string aliasCompleto;
            if (Alias.TryGetValue(Normalizar(texto), out aliasCompleto)) consultas.AddRange(aliasCompleto.Split('|'));
            else if (Normalizar(expandido) != Normalizar(texto)) consultas.Add(expandido);
            consultas = consultas.Distinct().ToList();

            var tareas = new List<Task<List<Candidato>>>();
            foreach (var q in consultas) tareas.Add(PhotonAsync(http, q));
            tareas.Add(NominatimAsync(http, expandido));
            await Task.WhenAny(Task.WhenAll(tareas), Task.Delay(3000));
            var respuestas = tareas.Where(x => x.Status == TaskStatus.RanToCompletion).Select(x => x.Result).ToList();
            if (respuestas.All(l => l.Count == 0)) respuestas = (await Task.WhenAll(tareas)).ToList();   // aún sin resultados: se espera a las demás

            // Juntar sin repetir (mismo nombre y a menos de ~150 m)
            var todos = new List<Candidato>();
            foreach (var lista in respuestas)
                foreach (var c in lista)
                {
                    if (c.Lng < MinLon || c.Lng > MaxLon || c.Lat < MinLat || c.Lat > MaxLat) continue;
                    string n = Normalizar(c.Nombre);
                    if (n.Length == 0) continue;
                    bool esCalle = TiposDeCalle.Any(tc => n.StartsWith(tc + " "));
                    bool repetido = todos.Any(o => Normalizar(o.Nombre) == n &&
                                                   (esCalle || (Math.Abs(o.Lat - c.Lat) < 0.0015 && Math.Abs(o.Lng - c.Lng) < 0.0015)));
                    if (!repetido) todos.Add(c);
                }

            // Primero lo que empieza igual que lo escrito, luego lo que lo contiene; dentro de cada grupo, lo más cercano al centro
            var qs = consultas.Select(Normalizar).Where(x => x.Length > 0).ToList();
            var deAlias = aliasCompleto != null ? aliasCompleto.Split('|').Select(Normalizar).ToList() : new List<string>();
            Func<Candidato, int> rango = c =>
            {
                string n = Normalizar(c.Nombre);
                if (deAlias.Any(q => n.StartsWith(q))) return -1;
                if (qs.Any(q => n.StartsWith(q))) return 0;
                if (qs.Any(q => n.Contains(" " + q))) return 1;
                if (qs.Any(q => n.Contains(q))) return 2;
                return 3;
            };
            Func<Candidato, double> distCentro = c =>
            {
                double dx = (c.Lng - CentroLng) * 111320 * Math.Cos(CentroLat * Math.PI / 180), dy = (c.Lat - CentroLat) * 110574;
                return Math.Sqrt(dx * dx + dy * dy);
            };

            var res = new List<Sugerencia>();
            foreach (var c in todos.OrderBy(rango).ThenBy(distCentro).Take(maximo))
                res.Add(new Sugerencia
                {
                    Numero = (res.Count + 1).ToString(),
                    Nombre = c.Nombre,
                    Detalle = c.Detalle,
                    Lat = c.Lat,
                    Lng = c.Lng
                });
            return res;
        }

        private static async Task<string> PedirAsync(HttpClient http, string url)
        {
            using (var cts = new System.Threading.CancellationTokenSource(6000))
            {
                var r = await http.GetAsync(url, cts.Token);
                r.EnsureSuccessStatusCode();
                return await r.Content.ReadAsStringAsync();
            }
        }

        private static string Str(System.Text.Json.JsonElement e, string prop)
        {
            System.Text.Json.JsonElement v;
            if (e.TryGetProperty(prop, out v) && v.ValueKind == System.Text.Json.JsonValueKind.String) return v.GetString() ?? "";
            return "";
        }

        // Photon: busca con el texto a medias ("kenn" encuentra "Kennedy") y favorece lo cercano al centro
        private static async Task<List<Candidato>> PhotonAsync(HttpClient http, string q)
        {
            var lista = new List<Candidato>();
            try
            {
                string url = "https://photon.komoot.io/api/?limit=10" +
                             "&lat=" + CentroLat.ToString(CultureInfo.InvariantCulture) + "&lon=" + CentroLng.ToString(CultureInfo.InvariantCulture) +
                             "&bbox=" + string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", MinLon, MinLat, MaxLon, MaxLat) +
                             "&q=" + Uri.EscapeDataString(q);
                string json = await PedirAsync(http, url);

                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    foreach (var f in doc.RootElement.GetProperty("features").EnumerateArray())
                    {
                        var pr = f.GetProperty("properties");
                        var co = f.GetProperty("geometry").GetProperty("coordinates");

                        string estado = Str(pr, "state");
                        if (estado.Length > 0 && !Normalizar(estado).Contains("chihuahua")) continue;

                        string calle = Str(pr, "street"), numero = Str(pr, "housenumber");
                        string nombre = Str(pr, "name");
                        string direccion = calle + (numero.Length > 0 ? " " + numero : "");
                        if (nombre.Length == 0) nombre = direccion;
                        if (nombre.Length == 0) continue;

                        var partes = new List<string>();
                        if (direccion.Length > 0 && direccion != nombre) partes.Add(direccion);
                        string colonia = Str(pr, "district"); if (colonia.Length == 0) colonia = Str(pr, "locality");
                        if (colonia.Length == 0) colonia = Str(pr, "suburb");
                        if (colonia.Length > 0 && colonia != nombre) partes.Add(colonia);
                        string ciudad = Str(pr, "city"); if (ciudad.Length == 0) ciudad = Str(pr, "county");
                        partes.Add(ciudad.Length > 0 ? ciudad : "Chihuahua");

                        lista.Add(new Candidato
                        {
                            Nombre = nombre,
                            Detalle = string.Join(", ", partes),
                            Lng = co[0].GetDouble(),
                            Lat = co[1].GetDouble()
                        });
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Photon: " + ex.Message); }
            return lista;
        }

        // Nominatim: direcciones y calles; solo dentro del rectángulo de la ciudad (bounded=1)
        private static async Task<List<Candidato>> NominatimAsync(HttpClient http, string q)
        {
            var lista = new List<Candidato>();
            try
            {
                string consulta = Normalizar(q).Contains("chihuahua") ? q : q + ", Chihuahua";
                string url = "https://nominatim.openstreetmap.org/search?format=jsonv2&limit=8&countrycodes=mx&addressdetails=0" +
                             "&bounded=1&viewbox=" + string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", MinLon, MaxLat, MaxLon, MinLat) +
                             "&q=" + Uri.EscapeDataString(consulta);
                string json = await PedirAsync(http, url);

                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    foreach (var p in doc.RootElement.EnumerateArray())
                    {
                        string completo = Str(p, "display_name");
                        var partes = completo.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        string nombre = Str(p, "name");
                        if (nombre.Length == 0 && partes.Count > 0) nombre = partes[0];
                        var detalle = partes.Where(x => x != nombre).Take(3);

                        lista.Add(new Candidato
                        {
                            Nombre = nombre,
                            Detalle = string.Join(", ", detalle),
                            Lat = double.Parse(Str(p, "lat"), CultureInfo.InvariantCulture),
                            Lng = double.Parse(Str(p, "lon"), CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Nominatim: " + ex.Message); }
            return lista;
        }
    }
}
