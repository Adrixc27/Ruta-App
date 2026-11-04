using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace RutaApp.Blazor.Services;

/// <summary>Lo que viaja dentro de un código para compartir: un lugar, un viaje personalizado o una ruta del catálogo.</summary>
public class Compartido
{
    public string Tipo { get; set; } = "";          // lugar | viaje | ruta
    public LugarGuardado Lugar { get; set; }
    public Trayecto Viaje { get; set; }
    public int RutaId { get; set; }
    public string RutaNombre { get; set; } = "";
}

/// <summary>
/// Códigos de texto ("RUTA1:...") para compartir paradas, lugares, rutas del catálogo o viajes personalizados.
/// El código se pega en un mensaje, en la comunidad o en "Importar código" del perfil de otra persona.
/// </summary>
public static class CodigoCompartir
{
    public const string Prefijo = "RUTA1:";
    private const int MaxPuntosPorTramo = 60;

    public static string Crear(Compartido contenido)
    {
        if (contenido.Viaje != null) contenido.Viaje = Reducir(contenido.Viaje);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(contenido);
        using var salida = new MemoryStream();
        using (var gz = new GZipStream(salida, CompressionLevel.SmallestSize, true))
            gz.Write(json, 0, json.Length);
        return Prefijo + Convert.ToBase64String(salida.ToArray()).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    /// <summary>Lee un código (o un texto que lo contiene). Devuelve null si no es un código válido.</summary>
    public static Compartido Leer(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        int i = texto.IndexOf(Prefijo, StringComparison.Ordinal);
        if (i < 0) return null;

        var sb = new StringBuilder();
        foreach (char c in texto.Substring(i + Prefijo.Length))
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(c);
            else break;
        }
        try
        {
            string b64 = sb.ToString().Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
            using var entrada = new MemoryStream(Convert.FromBase64String(b64));
            using var gz = new GZipStream(entrada, CompressionMode.Decompress);
            var resultado = JsonSerializer.Deserialize<Compartido>(gz);
            bool valido = resultado != null && (resultado.Tipo == "lugar" || resultado.Tipo == "viaje" || resultado.Tipo == "ruta");
            return valido ? resultado : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // El código lleva el recorrido simplificado (pocos puntos y 5 decimales) para que no sea larguísimo
    private static Trayecto Reducir(Trayecto t)
    {
        return new Trayecto
        {
            Alias = t.Alias, Recorrido = t.Recorrido, Detalle = t.Detalle, Origen = t.Origen, Destino = t.Destino,
            OrigenLat = t.OrigenLat, OrigenLng = t.OrigenLng, DestinoLat = t.DestinoLat, DestinoLng = t.DestinoLng,
            Minutos = t.Minutos, Tarifa = t.Tarifa, Lineas = t.Lineas, Escalas = t.Escalas,
            Tramos = t.Tramos.Select(x => new Tramo
            {
                EsBus = x.EsBus, TextoInicio = x.TextoInicio, TextoFin = x.TextoFin, Linea = x.Linea, Color = x.Color, Minutos = x.Minutos,
                AjustarACalles = true, Puntos = Adelgazar(x.Puntos)
            }).ToList()
        };
    }

    private static List<double[]> Adelgazar(List<double[]> pts)
    {
        if (pts.Count <= MaxPuntosPorTramo) return pts.Select(p => new[] { Math.Round(p[0], 5), Math.Round(p[1], 5) }).ToList();
        var res = new List<double[]>();
        double paso = (pts.Count - 1) / (double)(MaxPuntosPorTramo - 1);
        for (int k = 0; k < MaxPuntosPorTramo; k++)
        {
            var p = pts[(int)Math.Round(k * paso)];
            res.Add(new[] { Math.Round(p[0], 5), Math.Round(p[1], 5) });
        }
        return res;
    }
}
