using System.Globalization;

namespace RutaApp.Nucleo.Modelos;

public static class Formato
{
    // true = mostrar millas y pies (se cambia en Configuración → Unidades de distancia).
    public static bool UsaMillas { get; set; }

    // true = textos como "5 min ago" en lugar de "hace 5 min" (se cambia en Configuración → Idioma).
    public static bool Ingles { get; set; }

    // "350 m" / "1.2 km", o "1,150 ft" / "0.7 mi" si se eligió millas.
    public static string Distancia(double metros)
    {
        var inv = CultureInfo.InvariantCulture;
        if (UsaMillas)
        {
            double pies = metros * 3.28084;
            return pies < 1000 ? (int)(Math.Round(pies / 10) * 10) + " ft" : (metros / 1609.344).ToString("0.#", inv) + " mi";
        }
        return metros >= 1000 ? (metros / 1000).ToString("0.#", inv) + " km" : Math.Round(metros).ToString("0", inv) + " m";
    }

    // Convierte los "(120 m)" de un texto de instrucciones a la unidad elegida.
    public static string ConvertirTexto(string texto)
    {
        if (!UsaMillas || texto == null) return texto;
        return System.Text.RegularExpressions.Regex.Replace(texto, @"\((\d+) m\)",
            m => "(" + Distancia(double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)) + ")");
    }

    private static readonly CompareInfo Comparador = new CultureInfo("es-MX").CompareInfo;

    // Nombre de la parada sin el ", Chihuahua, Chihuahua" del final.
    public static string NombreParada(string nombre) =>
        string.IsNullOrWhiteSpace(nombre) ? "" : nombre.Replace(", Chihuahua, Chihuahua", "").Trim();

    // ¿El texto contiene lo buscado? Sin importar mayúsculas ni acentos.
    public static bool Coincide(string texto, string buscado) =>
        !string.IsNullOrEmpty(texto) && Comparador.IndexOf(texto, buscado, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    // "ahora", "hace 5 min", "hace 3 h", "ayer", "hace 4 d" o la fecha si pasó más de una semana.
    public static string Hace(DateTime fecha)
    {
        var t = DateTime.Now - fecha;
        if (Ingles)
        {
            if (t.TotalMinutes < 1) return "now";
            if (t.TotalMinutes < 60) return (int)t.TotalMinutes + " min ago";
            if (t.TotalHours < 24) return (int)t.TotalHours + " h ago";
            if (t.TotalDays < 2) return "yesterday";
            if (t.TotalDays < 8) return (int)t.TotalDays + " d ago";
            return fecha.ToString("MMM d", new CultureInfo("en-US"));
        }
        if (t.TotalMinutes < 1) return "ahora";
        if (t.TotalMinutes < 60) return "hace " + (int)t.TotalMinutes + " min";
        if (t.TotalHours < 24) return "hace " + (int)t.TotalHours + " h";
        if (t.TotalDays < 2) return "ayer";
        if (t.TotalDays < 8) return "hace " + (int)t.TotalDays + " d";
        return fecha.ToString("d 'de' MMM", new CultureInfo("es-MX"));
    }
}
