namespace RutaApp.Interfaz.Servicios;

// Destinos que se sugieren al empezar a elegir el destino.
public partial class EstadoApp
{
    // Distancia máxima de un lugar al recorrido de una ruta para recomendarlo (la misma que usa el planificador para sugerir dónde bajar).
    private const double MetrosMaxLugarRecomendado = 500;

    // Lugares conocidos a los que sí se puede dibujar un viaje con las rutas cargadas: los que quedan cerca del recorrido de alguna.
    // Si se conoce la ubicación de la persona, salen primero los más cercanos a ella.
    public List<Sugerencia> LugaresRecomendados(int max = 6)
    {
        var lista = new List<(Sugerencia Lugar, double Distancia)>();
        foreach (var lugar in LugaresSugeridos.Todos)
        {
            double mejor = double.MaxValue;
            string ruta = null;
            foreach (var r in Rutas)
            {
                foreach (var variante in r.Variantes ?? new List<RutaOficial>())
                {
                    Geo.ProyectarEnLinea(variante.Trazo, lugar.Lat, lugar.Lng, out double m);
                    if (m < mejor) { mejor = m; ruta = r.Nombre; }
                }
            }
            if (ruta == null || mejor > MetrosMaxLugarRecomendado) continue;

            var sugerencia = new Sugerencia
            {
                Numero = "LUGAR",
                Nombre = lugar.Nombre,
                Detalle = lugar.Tipo + " · ruta " + ruta + " a " + Formato.Distancia(mejor),
                Lat = lugar.Lat,
                Lng = lugar.Lng
            };
            lista.Add((sugerencia, MiPosicion == null ? 0 : MetrosEntre(MiPosicion[0], MiPosicion[1], lugar.Lat, lugar.Lng)));
        }
        return lista.OrderBy(x => x.Distancia).Select(x => x.Lugar).Take(max).ToList();
    }

    private static double MetrosEntre(double lat1, double lng1, double lat2, double lng2)
    {
        double k = Math.Cos(lat1 * Math.PI / 180);
        double dx = (lng2 - lng1) * 111320 * k, dy = (lat2 - lat1) * 110574;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
