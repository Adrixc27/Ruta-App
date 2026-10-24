using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RutaApp.Blazor.Services;

/// <summary>
/// Buscador de lugares limitado a la ciudad de Chihuahua. Combina Photon (busca mientras escribes, sirve para
/// nombres cortos o incompletos) con Nominatim (direcciones y calles), ambos de OpenStreetMap y sin API key.
/// Entiende abreviaturas ("av", "blvd", "col", "fracc", "uach"...) y no devuelve nada fuera de la ciudad.
/// </summary>
public static class BuscadorLugares
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
