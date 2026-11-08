namespace RutaApp.Nucleo.Modelos;

public static class Geo
{
    public const double CentroLat = 28.6353;
    public const double CentroLng = -106.0889;

    /// <summary>
    /// Punto de la línea más cercano a (lat, lng) y a qué distancia en metros queda. Sirve para que una parada
    /// desfasada se dibuje sobre el recorrido en lugar de a un lado.
    /// </summary>
    public static double[] ProyectarEnLinea(IList<double[]> linea, double lat, double lng, out double metros)
    {
        metros = double.MaxValue;
        if (linea == null || linea.Count == 0) return null;
        double kx = 111320 * Math.Cos(lat * Math.PI / 180), ky = 110574;
        double[] mejor = linea[0];
        for (int i = 0; i < linea.Count; i++)
        {
            double ax = (linea[i][1] - lng) * kx, ay = (linea[i][0] - lat) * ky;
            double t = 0, bx = ax, by = ay;
            if (i + 1 < linea.Count)
            {
                bx = (linea[i + 1][1] - lng) * kx; by = (linea[i + 1][0] - lat) * ky;
                double dx = bx - ax, dy = by - ay, largo2 = dx * dx + dy * dy;
                if (largo2 > 0) t = Math.Max(0, Math.Min(1, -(ax * dx + ay * dy) / largo2));
            }
            double px = ax + (bx - ax) * t, py = ay + (by - ay) * t;
            double d = Math.Sqrt(px * px + py * py);
            if (d < metros)
            {
                metros = d;
                mejor = i + 1 < linea.Count
                    ? new[] { linea[i][0] + (linea[i + 1][0] - linea[i][0]) * t, linea[i][1] + (linea[i + 1][1] - linea[i][1]) * t }
                    : linea[i];
            }
        }
        return mejor;
    }
}
