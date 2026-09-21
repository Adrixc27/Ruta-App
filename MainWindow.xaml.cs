using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Ruta_App
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient _http = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
            InicializarMapa();
        }

        // ── Inicializar WebView2 ───────────────────────────────────────────────
        private async void InicializarMapa()
        {
            await MapaWebView.EnsureCoreWebView2Async(null);
            MapaWebView.CoreWebView2.NavigateToString(GenerarHtmlMapa());
        }

        // ── HTML del mapa ─────────────────────────────────────────────────────
        private string GenerarHtmlMapa()
        {
            return @"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
  <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    html, body, #map { width: 100%; height: 100vh; background: #0B1628; }
    .leaflet-popup-content-wrapper {
      background: #1E293B; color: #F1F5F9;
      border: 1px solid #334155; border-radius: 10px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.5);
    }
    .leaflet-popup-tip { background: #1E293B; }
    .leaflet-popup-content {
      font-family: 'Segoe UI', sans-serif;
      font-size: 13px; line-height: 1.6;
    }
    .leaflet-popup-content b { color: #38BDF8; }
    .leaflet-popup-content .tipo { color: #94A3B8; font-size: 11px; margin-top: 2px; }
    .leaflet-control-zoom a {
      background: #1E293B !important;
      color: #94A3B8 !important;
      border-color: #334155 !important;
    }
    .leaflet-control-zoom a:hover {
      background: #0C4A6E !important;
      color: #38BDF8 !important;
    }
    .leaflet-control-attribution {
      background: rgba(15,23,42,0.8) !important;
      color: #475569 !important;
      font-size: 10px;
    }
    .leaflet-control-attribution a { color: #38BDF8 !important; }

    #panel-carga {
      position: absolute; top: 12px; left: 50%;
      transform: translateX(-50%);
      background: #1E293B; border: 1px solid #334155;
      border-radius: 10px; padding: 8px 16px;
      color: #94A3B8; font-family: Segoe UI, sans-serif;
      font-size: 12px; z-index: 9999; display: none;
    }
    #panel-carga.visible { display: block; }

    #leyenda {
      position: absolute; bottom: 28px; left: 12px;
      background: #1E293B; border: 1px solid #334155;
      border-radius: 10px; padding: 10px 14px;
      z-index: 9999; font-family: Segoe UI, sans-serif; font-size: 12px;
    }
    .leyenda-item {
      display: flex; align-items: center;
      gap: 8px; color: #94A3B8; margin-bottom: 5px;
    }
    .leyenda-item:last-child { margin-bottom: 0; }
    .dot { width: 12px; height: 12px; border-radius: 50%; flex-shrink: 0; }
    .cuad { width: 12px; height: 12px; border-radius: 3px; flex-shrink: 0; }
  </style>
</head>
<body>
<div id='map'></div>

<div id='panel-carga'>⏳ Cargando paradas...</div>

<div id='leyenda'>
  <div class='leyenda-item'><div class='dot' style='background:#38BDF8;border:2px solid #38BDF8'></div> Origen</div>
  <div class='leyenda-item'><div class='dot' style='background:#4C1D95;border:2px solid #A78BFA'></div> Destino</div>
  <div class='leyenda-item'><div class='dot' style='background:#1C2D1A;border:2px solid #FACC15'></div> Parada de autobús</div>
  <div class='leyenda-item'><div class='cuad' style='background:#0D2A1A;border:2px solid #4ADE80'></div> Punto de referencia</div>
</div>

<script>
  var mapa = L.map('map', { zoomControl: true }).setView([28.6353, -106.0889], 13);

  L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; OpenStreetMap &copy; CARTO',
    subdomains: 'abcd',
    maxZoom: 19
  }).addTo(mapa);

  var marcadorOrigen  = null;
  var marcadorDestino = null;
  var lineaRuta       = null;
  var capaParadas     = L.layerGroup().addTo(mapa);
  var capaReferencia  = L.layerGroup().addTo(mapa);

  function crearIcono(tipo) {
    var letra = tipo === 'origen' ? 'A' : 'B';
    var color = tipo === 'origen' ? '#38BDF8' : '#A78BFA';
    var bg    = tipo === 'origen' ? '#0C4A6E' : '#4C1D95';
    return L.divIcon({
      className: '',
      html: '<div style=""background:' + bg + ';color:' + color + ';border:2px solid ' + color + ';border-radius:50%;width:34px;height:34px;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:15px;font-family:Segoe UI,sans-serif;box-shadow:0 0 10px ' + color + '44"">' + letra + '</div>',
      iconSize:   [34, 34],
      iconAnchor: [17, 17],
      popupAnchor:[0, -20]
    });
  }

  function crearIconoParada() {
    return L.divIcon({
      className: '',
      html: '<div style=""background:#1C2D1A;border:2px solid #FACC15;border-radius:50%;width:14px;height:14px;box-shadow:0 0 6px #FACC1588""></div>',
      iconSize:   [14, 14],
      iconAnchor: [7, 7],
      popupAnchor:[0, -10]
    });
  }

  function crearIconoReferencia() {
    return L.divIcon({
      className: '',
      html: '<div style=""background:#0D2A1A;border:2px solid #4ADE80;border-radius:4px;width:14px;height:14px;box-shadow:0 0 6px #4ADE8088""></div>',
      iconSize:   [14, 14],
      iconAnchor: [7, 7],
      popupAnchor:[0, -10]
    });
  }

  function trazarRuta(latOrigen, lngOrigen, nombreOrigen, latDestino, lngDestino, nombreDestino) {
    if (marcadorOrigen)  mapa.removeLayer(marcadorOrigen);
    if (marcadorDestino) mapa.removeLayer(marcadorDestino);
    if (lineaRuta)       mapa.removeLayer(lineaRuta);

    var ptOrigen  = [latOrigen,  lngOrigen];
    var ptDestino = [latDestino, lngDestino];

    marcadorOrigen = L.marker(ptOrigen, { icon: crearIcono('origen') })
      .addTo(mapa).bindPopup('<b>Origen</b><br>' + nombreOrigen);

    marcadorDestino = L.marker(ptDestino, { icon: crearIcono('destino') })
      .addTo(mapa).bindPopup('<b>Destino</b><br>' + nombreDestino);

    lineaRuta = L.polyline([ptOrigen, ptDestino], {
      color: '#38BDF8', weight: 4, opacity: 0.85, dashArray: '10, 8'
    }).addTo(mapa);

    mapa.fitBounds([ptOrigen, ptDestino], { padding: [60, 60] });

    document.getElementById('panel-carga').classList.add('visible');
  }

  function cargarPuntos(puntos) {
    document.getElementById('panel-carga').classList.remove('visible');
    capaParadas.clearLayers();
    capaReferencia.clearLayers();

    var emojis = {
      parada:       '🚍',
      farmacia:     '💊',
      hospital:     '🏥',
      escuela:      '🏫',
      supermercado: '🛒'
    };

    puntos.forEach(function(p) {
      var esParada = p.tipo === 'parada';
      var icono    = esParada ? crearIconoParada() : crearIconoReferencia();
      var emoji    = emojis[p.tipo] || '📍';
      var capa     = esParada ? capaParadas : capaReferencia;

      var popup = '<b>' + (p.nombre || emoji + ' ' + p.tipo) + '</b>'
        + (p.nombre ? '<div class=""tipo"">' + emoji + ' ' + p.tipo + '</div>' : '');

      L.marker([p.lat, p.lng], { icon: icono })
        .addTo(capa)
        .bindPopup(popup);
    });
  }

  function limpiarMapa() {
    if (marcadorOrigen)  mapa.removeLayer(marcadorOrigen);
    if (marcadorDestino) mapa.removeLayer(marcadorDestino);
    if (lineaRuta)       mapa.removeLayer(lineaRuta);
    capaParadas.clearLayers();
    capaReferencia.clearLayers();
    marcadorOrigen = marcadorDestino = lineaRuta = null;
    mapa.setView([28.6353, -106.0889], 13);
  }
</script>
</body>
</html>";
        }

        // ── Botón Buscar ──────────────────────────────────────────────────────
        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtOrigen.Text.Trim();
            string destino = TxtDestino.Text.Trim();

            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                MessageBox.Show("Por favor escribe origen y destino.", "R.U.T.A.",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var rng = new Random();
            double latOrigen = 28.6353 + (rng.NextDouble() - 0.5) * 0.06;
            double lngOrigen = -106.0889 + (rng.NextDouble() - 0.5) * 0.06;
            double latDestino = 28.6353 + (rng.NextDouble() - 0.5) * 0.06;
            double lngDestino = -106.0889 + (rng.NextDouble() - 0.5) * 0.06;

            // Trazar marcadores y línea en el mapa
            string scriptRuta = string.Format(
                CultureInfo.InvariantCulture,
                "trazarRuta({0},{1},'{2}',{3},{4},'{5}')",
                latOrigen, lngOrigen, origen,
                latDestino, lngDestino, destino);

            await MapaWebView.CoreWebView2.ExecuteScriptAsync(scriptRuta);

            // Mostrar tarjetas
            PlaceholderCard.Visibility = Visibility.Collapsed;
            Ruta1Card.Visibility = Visibility.Visible;
            Ruta2Card.Visibility = Visibility.Visible;
            Ruta3Card.Visibility = Visibility.Visible;

            LblHeader.Text = $"{origen}  →  {destino}";
            LblConteoRutas.Text = "3 rutas encontradas";
            LblTiempo.Text = "28 min";
            LblTransbordos.Text = "1";
            LblCaminata.Text = "350 m";
            LblBus.Text = "L-2 / L-7";

            // Consultar Overpass desde C# y enviar datos al mapa
            await CargarParadasDesdeCS(latOrigen, lngOrigen, latDestino, lngDestino);
        }

        // ── Consulta Overpass desde C# ────────────────────────────────────────
        private async Task CargarParadasDesdeCS(double latO, double lngO, double latD, double lngD)
        {
            try
            {
                double s = Math.Min(latO, latD) - 0.005;
                double o = Math.Min(lngO, lngD) - 0.005;
                double n = Math.Max(latO, latD) + 0.005;
                double e = Math.Max(lngO, lngD) + 0.005;

                string query = string.Format(CultureInfo.InvariantCulture,
                    "[out:json][timeout:25];" +
                    "(" +
                    "node[highway=bus_stop]({0},{1},{2},{3});" +
                    "node[amenity=bus_station]({0},{1},{2},{3});" +
                    "node[amenity=pharmacy]({0},{1},{2},{3});" +
                    "node[amenity=hospital]({0},{1},{2},{3});" +
                    "node[amenity=school]({0},{1},{2},{3});" +
                    "node[shop=supermarket]({0},{1},{2},{3});" +
                    ");out body;",
                    s, o, n, e);

                var content = new StringContent(query);
                var response = await _http.PostAsync(
                    "https://overpass-api.de/api/interpreter", content);
                string json = await response.Content.ReadAsStringAsync();

                var puntos = ParsearOverpass(json);

                // Construir array JS con los datos
                var sb = new StringBuilder();
                sb.Append("cargarPuntos([");
                foreach (var p in puntos)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "{{lat:{0},lng:{1},nombre:'{2}',tipo:'{3}'}},",
                        p.Lat, p.Lng,
                        p.Nombre.Replace("'", "\\'"),
                        p.Tipo);
                }
                sb.Append("]);");

                await MapaWebView.CoreWebView2.ExecuteScriptAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Overpass error: " + ex.Message);
                // Quitar el panel de carga aunque haya fallado
                await MapaWebView.CoreWebView2.ExecuteScriptAsync(
                    "document.getElementById('panel-carga').classList.remove('visible');");
            }
        }

        // ── Parser JSON de Overpass sin dependencias externas ─────────────────
        private List<PuntoMapa> ParsearOverpass(string json)
        {
            var lista = new List<PuntoMapa>();
            try
            {
                var bloques = json.Split(
                    new[] { "{\"type\":\"node\"" },
                    StringSplitOptions.RemoveEmptyEntries);

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
                    if (bloque.Contains("\"highway\":\"bus_stop\"") ||
                        bloque.Contains("\"amenity\":\"bus_station\""))
                        tipo = "parada";
                    else if (bloque.Contains("\"amenity\":\"pharmacy\""))
                        tipo = "farmacia";
                    else if (bloque.Contains("\"amenity\":\"hospital\""))
                        tipo = "hospital";
                    else if (bloque.Contains("\"amenity\":\"school\""))
                        tipo = "escuela";
                    else if (bloque.Contains("\"shop\":\"supermarket\""))
                        tipo = "supermercado";
                    else
                        continue;

                    lista.Add(new PuntoMapa
                    {
                        Lat = lat,
                        Lng = lng,
                        Nombre = nombre,
                        Tipo = tipo
                    });
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
    }
}