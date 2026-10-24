using System.Collections.Concurrent;
using System.Globalization;

namespace RutaApp.Blazor.Services;

/// <summary>
/// Recorridos por calles (OSRM), nombre de la calle de unas coordenadas (Nominatim), paradas cercanas (Overpass)
/// y búsqueda de lugares. Es un servicio único para toda la app: los recorridos que ya se pidieron quedan en caché.
/// </summary>
public class ServicioTrazos
{
    private readonly IHttpClientFactory _fabrica;
    private readonly ILogger<ServicioTrazos> _log;
    private readonly ConcurrentDictionary<string, List<double[]>> _cacheOsrm = new();

    public ServicioTrazos(IHttpClientFactory fabrica, ILogger<ServicioTrazos> log)
    {
        _fabrica = fabrica;
        _log = log;
    }

    private HttpClient Http => _fabrica.CreateClient("externo");

    // Texto "lng,lat;lng,lat;..." que usa OSRM
    public static string ClaveOsrm(IEnumerable<double[]> puntosLatLng)
    {
        return string.Join(";", puntosLatLng.Select(w =>
            w[1].ToString("R", CultureInfo.InvariantCulture) + "," + w[0].ToString("R", CultureInfo.InvariantCulture)));
    }

    /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM). null si no hay internet.</summary>
    public async Task<List<double[]>> TrazoOsrmAsync(string coordenadas)
    {
        if (_cacheOsrm.TryGetValue(coordenadas, out var guardado)) return guardado;
        try
        {
            string json = await Http.GetStringAsync(
                "https://router.project-osrm.org/route/v1/driving/" + coordenadas + "?overview=full&geometries=geojson");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var raiz = doc.RootElement;
            if (raiz.GetProperty("code").GetString() != "Ok") return null;
            var lista = new List<double[]>();
            foreach (var c in raiz.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates").EnumerateArray())
                lista.Add(new[] { c[1].GetDouble(), c[0].GetDouble() });   // Leaflet usa [lat, lng]
            if (lista.Count < 2) return null;
            _cacheOsrm[coordenadas] = lista;
            return lista;
        }
        catch (Exception ex)
        {
            _log.LogDebug("OSRM: {Mensaje}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Calcula el recorrido por calles de las rutas que tienen Waypoints (OSRM, todas a la vez) y lo deja en Ruta.Trazo.
    /// Devuelve false si alguna no se pudo calcular (sin internet): se usan los puntos clave y se reintenta la próxima vez.
    /// </summary>
    public async Task<bool> PrepararAsync(IEnumerable<Ruta> todas)
    {
        var rutas = todas.Where(r => r.Waypoints != null && r.Waypoints.Count >= 2).ToList();
        var claves = rutas.Select(r => ClaveOsrm(r.Waypoints)).ToList();

        // Las que faltan se piden todas al mismo tiempo
        var faltan = claves.Where(c => !_cacheOsrm.ContainsKey(c)).Distinct().ToList();
        await Task.WhenAll(faltan.Select(c => TrazoOsrmAsync(c)));

        bool completo = true;
        for (int n = 0; n < rutas.Count; n++)
        {
            var r = rutas[n];
            if (!_cacheOsrm.TryGetValue(claves[n], out var trazo))
            {
                completo = false;
                trazo = new List<double[]>(r.Waypoints);
            }
            r.Trazo = trazo;

            double metros = 0;
            double k = Math.Cos(trazo[0][0] * Math.PI / 180);
            for (int i = 1; i < trazo.Count; i++)
            {
                double dx = (trazo[i][1] - trazo[i - 1][1]) * 111320 * k;
                double dy = (trazo[i][0] - trazo[i - 1][0]) * 110574;
                metros += Math.Sqrt(dx * dx + dy * dy);
            }
            if (!r.DatosReales) r.Minutos = Math.Max(5, (int)Math.Round(metros / 333));   // los minutos de la BD no se pisan
            r.Paradas = Math.Max(2, (int)(metros / 400) + 1);
        }
        return completo;
    }

    /// <summary>Nombre de la calle en esas coordenadas (OpenStreetMap / Nominatim), o null si no hay internet o no se encontró.</summary>
    public async Task<string> CalleDeAsync(double lat, double lng)
    {
        try
        {
            string url = "https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=18&accept-language=es&lat=" +
                         lat.ToString("R", CultureInfo.InvariantCulture) + "&lon=" + lng.ToString("R", CultureInfo.InvariantCulture);
            string json = await Http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("address", out var dir)) return null;
            foreach (var clave in new[] { "road", "pedestrian", "residential", "footway", "path", "neighbourhood", "suburb" })
            {
                if (dir.TryGetProperty(clave, out var valor) && !string.IsNullOrWhiteSpace(valor.GetString()))
                    return valor.GetString();
            }
            return null;
        }
        catch { return null; }
    }

    /// <summary>Opción de viaje (OSRM) entre dos puntos para dibujar una ruta que aún no tiene recorrido propio.</summary>
    public Task<List<double[]>> TrazoEntreAsync(double latO, double lngO, double latD, double lngD)
    {
        return TrazoOsrmAsync(ClaveOsrm(new[] { new[] { latO, lngO }, new[] { latD, lngD } }));
    }

    // ── Buscador de lugares (Photon + Nominatim) ──────────────────────────
    public Task<List<Sugerencia>> BuscarLugaresAsync(string texto) => BuscadorLugares.BuscarAsync(Http, texto);

    // ── Paradas y puntos de referencia (Overpass / OpenStreetMap) ─────────
    public class PuntoMapa
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
    }

    public async Task<List<PuntoMapa>> ParadasEntreAsync(double latO, double lngO, double latD, double lngD)
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

            var contenido = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
            var respuesta = await Http.PostAsync("https://overpass-api.de/api/interpreter", contenido);
            string json = await respuesta.Content.ReadAsStringAsync();

            return ParsearOverpass(json).Take(80).ToList();   // pocos puntos: mapa limpio
        }
        catch (Exception ex)
        {
            _log.LogDebug("Overpass: {Mensaje}", ex.Message);
            return new List<PuntoMapa>();
        }
    }

    // Parser JSON de Overpass sin dependencias externas
    private static List<PuntoMapa> ParsearOverpass(string json)
    {
        var lista = new List<PuntoMapa>();
        var bloques = json.Split(new[] { "{\"type\":\"node\"" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var bloque in bloques)
        {
            var mLat = System.Text.RegularExpressions.Regex.Match(bloque, "\"lat\":([0-9.\\-]+)");
            var mLng = System.Text.RegularExpressions.Regex.Match(bloque, "\"lon\":([0-9.\\-]+)");
            if (!mLat.Success || !mLng.Success) continue;

            double lat = double.Parse(mLat.Groups[1].Value, CultureInfo.InvariantCulture);
            double lng = double.Parse(mLng.Groups[1].Value, CultureInfo.InvariantCulture);
            var mName = System.Text.RegularExpressions.Regex.Match(bloque, "\"name\":\"([^\"]+)\"");
            string nombre = mName.Success ? mName.Groups[1].Value : "";

            string tipo;
            if (bloque.Contains("\"highway\":\"bus_stop\"") || bloque.Contains("\"amenity\":\"bus_station\"")) tipo = "parada";
            else if (bloque.Contains("\"amenity\":\"pharmacy\"")) tipo = "farmacia";
            else if (bloque.Contains("\"amenity\":\"hospital\"")) tipo = "hospital";
            else if (bloque.Contains("\"amenity\":\"school\"")) tipo = "escuela";
            else if (bloque.Contains("\"shop\":\"supermarket\"")) tipo = "supermercado";
            else continue;

            lista.Add(new PuntoMapa { Lat = lat, Lng = lng, Nombre = nombre, Tipo = tipo });
        }
        return lista;
    }
}
