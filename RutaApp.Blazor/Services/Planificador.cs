using System.Globalization;

namespace RutaApp.Blazor.Services;

/// <summary>
/// Arma opciones de viaje en camión con una búsqueda del camino más corto sobre toda la red de paradas:
/// caminar a una parada, viajar por el recorrido de una ruta, bajar, caminar a otra ruta (transbordo)...
/// puede combinar 2, 3 o más camiones.
///
/// Cada Ruta aporta su recorrido en Ruta.Trazo (puntos en orden, siguiendo calles). Las paradas se toman
/// cada ~400 m. Con rutas reales basta con llenar Ruta.Trazo (p. ej. con la tabla de paradas ordenadas).
///
/// Si el origen o el destino queda lejos de toda la red, se crean rutas de EJEMPLO que siguen calles
/// (calculadas con OSRM) para conectarlos con las paradas más cercanas.
/// </summary>
public static class Planificador
{
    private const double MetrosPorMinCaminando = 80;      // ~4.8 km/h
    private const double MetrosPorMinBus = 333;           // ~20 km/h en ciudad
    private const double MaxCaminataMetros = 1000;        // más lejos que esto no se considera "cerca"
    private const double CaminataParaRutaEjemplo = 700;   // si la parada más cercana está más lejos, se crea una ruta de ejemplo
    private const double MaxCaminataTransbordoMetros = 350;
    private const double SeparacionParadasMetros = 400;
    private const double ViajeMinimoMetros = 250;
    private const double PenalizacionTransbordoMin = 3;
    private const double DistanciaMaximaMetros = 40000;   // más lejos que esto no se calcula

    private class Linea
    {
        public Ruta R;
        public bool Ejemplo;          // ruta creada al vuelo para conectar un punto lejano
        public List<double[]> P;      // trazo [lat, lng]
        public double[][] M;          // trazo en metros (x, y)
        public double[] Cum;          // distancia acumulada a lo largo del trazo
        public List<int> Paradas;     // índices del trazo que son parada
        public string Corto { get { return Ejemplo ? "Ruta ejemplo " + (R.Numero - 900) : "Ruta " + R.Numero; } }
    }

    private class Arista { public int A, B; public double Costo; public bool Viaje; }

    private class Nodo
    {
        public Linea L;
        public int Idx;
        public double[] M;
        public List<Arista> Sal = new List<Arista>();
    }

    private class Opcion
    {
        public double Total, CaminaM;
        public List<Tramo> Tramos = new List<Tramo>();
        public string Resumen = "";
        public List<Linea> Lineas = new List<Linea>();
    }

    public static async Task<List<Ruta>> GenerarAsync(IEnumerable<Ruta> rutas, double oLat, double oLng,
                                                      double dLat, double dLng, Func<string, Task<List<double[]>>> trazoOsrm)
    {
        double k = Math.Cos(oLat * Math.PI / 180);
        Func<double[], double[]> aM = p => new[] { p[1] * 111320 * k, p[0] * 110574 };
        var origen = new[] { oLat, oLng };
        var destino = new[] { dLat, dLng };
        var O = aM(origen);
        var D = aM(destino);

        if (Dist(O, D) > DistanciaMaximaMetros) return new List<Ruta>();

        var lineas = new List<Linea>();
        foreach (var r in rutas)
        {
            var l = Construir(r, aM, false);
            if (l != null) lineas.Add(l);
        }

        await AgregarRutasEjemploAsync(lineas, origen, destino, O, D, aM, trazoOsrm);

        var nodos = ConstruirGrafo(lineas);

        // Mejor camino y variantes: se vuelve a buscar prohibiendo, una por una, las rutas del mejor resultado
        var opciones = new List<Opcion>();
        var firmas = new HashSet<string>();
        var pendientes = new Queue<HashSet<Linea>>();
        pendientes.Enqueue(new HashSet<Linea>());
        int corridas = 0;
        while (pendientes.Count > 0 && corridas < 12)
        {
            var vetadas = pendientes.Dequeue();
            corridas++;
            var o = Buscar(nodos, vetadas, O, D, origen, destino);
            if (o == null) continue;
            if (!firmas.Add(string.Join("+", o.Lineas.Select(l => l.R.Numero)))) continue;
            opciones.Add(o);
            foreach (var l in o.Lineas.Where(x => !x.Ejemplo))
                pendientes.Enqueue(new HashSet<Linea>(vetadas) { l });
        }

        // El algoritmo decide: menos tiempo, poca caminata, pocos transbordos y tarifa baja
        Func<Opcion, double> puntaje = o => o.Total + 0.5 * MinCam(o.CaminaM) + 2 * (o.Lineas.Count - 1) + (double)o.Lineas.Sum(l => l.R.Tarifa);

        var resultado = opciones.OrderBy(puntaje).Take(4).Select(Convertir).ToList();
        Etiquetar(resultado);
        return resultado;
    }

    /// <summary>Pone las etiquetas Recomendado / Más rápido / Económico / Menos caminata a una lista de opciones.</summary>
    public static void Etiquetar(List<Ruta> lista)
    {
        if (lista.Count == 0) return;
        foreach (var r in lista) r.Etiqueta = "";

        lista[0].Etiqueta = "Recomendado";
        Action<Ruta, string> poner = (r, texto) => { if (string.IsNullOrEmpty(r.Etiqueta)) r.Etiqueta = texto; };
        poner(lista.OrderBy(c => c.Minutos).First(), "Más rápido");
        poner(lista.OrderBy(c => c.Tarifa).ThenBy(c => c.Minutos).First(), "Económico");
        poner(lista.OrderBy(c => c.CaminataMetros).First(), "Menos caminata");
    }

    // ── Rutas de ejemplo para puntos que quedan lejos de la red ───────────
    private static async Task AgregarRutasEjemploAsync(List<Linea> lineas, double[] origen, double[] destino,
                                                       double[] O, double[] D, Func<double[], double[]> aM,
                                                       Func<string, Task<List<double[]>>> trazoOsrm)
    {
        var tareas = new List<Task<Linea>>();

        if (lineas.Count == 0)
        {
            tareas.Add(CrearEjemploAsync(901, origen, destino, aM, trazoOsrm));
        }
        else
        {
            double[] cercaO, cercaD;
            double mO = ParadaMasCercana(lineas, O, out cercaO);
            double mD = ParadaMasCercana(lineas, D, out cercaD);
            if (mO > CaminataParaRutaEjemplo) tareas.Add(CrearEjemploAsync(901, origen, cercaO, aM, trazoOsrm));
            if (mD > CaminataParaRutaEjemplo) tareas.Add(CrearEjemploAsync(902, cercaD, destino, aM, trazoOsrm));
        }

        foreach (var l in await Task.WhenAll(tareas))
            if (l != null) lineas.Add(l);
    }

    private static double ParadaMasCercana(List<Linea> lineas, double[] punto, out double[] latLng)
    {
        double mejor = double.MaxValue;
        latLng = null;
        foreach (var l in lineas)
            foreach (int i in l.Paradas)
            {
                double d = Dist(punto, l.M[i]);
                if (d < mejor) { mejor = d; latLng = l.P[i]; }
            }
        return mejor;
    }

    // Ruta de ejemplo entre dos puntos, siguiendo las calles (OSRM; sin internet, línea recta)
    private static async Task<Linea> CrearEjemploAsync(int numero, double[] a, double[] b, Func<double[], double[]> aM,
                                                          Func<string, Task<List<double[]>>> trazoOsrm)
    {
        if (Dist(aM(a), aM(b)) < ViajeMinimoMetros) return null;

        string coords = a[1].ToString("R", CultureInfo.InvariantCulture) + "," + a[0].ToString("R", CultureInfo.InvariantCulture) + ";" +
                        b[1].ToString("R", CultureInfo.InvariantCulture) + "," + b[0].ToString("R", CultureInfo.InvariantCulture);
        var trazo = await trazoOsrm(coords) ?? new List<double[]> { a, b };

        var r = new Ruta
        {
            Numero = numero,
            Nombre = "Ruta ejemplo " + (numero - 900),
            Lineas = "Bus ejemplo " + (numero - 900),
            ModoTexto = "Autobús (ejemplo)",
            Tarifa = 2.75m,
            FrecuenciaMin = 12,
            Trazo = trazo
        };
        return Construir(r, aM, true);
    }

    // ── Preparar el recorrido de una ruta ─────────────────────────────────
    private static Linea Construir(Ruta r, Func<double[], double[]> aM, bool ejemplo)
    {
        var bruto = r.Trazo != null && r.Trazo.Count >= 2
            ? r.Trazo
            : new List<double[]> { new[] { r.LatO, r.LngO }, new[] { r.LatD, r.LngD } };

        var p = Densificar(bruto, aM, 100);
        var m = p.Select(aM).ToArray();
        var cum = new double[p.Count];
        for (int i = 1; i < p.Count; i++) cum[i] = cum[i - 1] + Dist(m[i - 1], m[i]);
        if (cum[cum.Length - 1] < ViajeMinimoMetros) return null;

        var paradas = new List<int> { 0 };
        double ultima = 0;
        for (int i = 1; i < p.Count - 1; i++)
        {
            if (cum[i] - ultima >= SeparacionParadasMetros) { paradas.Add(i); ultima = cum[i]; }
        }
        paradas.Add(p.Count - 1);

        return new Linea { R = r, Ejemplo = ejemplo, P = p, M = m, Cum = cum, Paradas = paradas };
    }

    // Agrega puntos intermedios para que ninguna parada quede lejos del trazo real
    private static List<double[]> Densificar(List<double[]> pts, Func<double[], double[]> aM, double paso)
    {
        var res = new List<double[]> { pts[0] };
        for (int i = 1; i < pts.Count; i++)
        {
            double d = Dist(aM(pts[i - 1]), aM(pts[i]));
            int n = (int)Math.Floor(d / paso);
            for (int s = 1; s <= n; s++)
            {
                double f = (double)s / (n + 1);
                res.Add(new[] { pts[i - 1][0] + (pts[i][0] - pts[i - 1][0]) * f, pts[i - 1][1] + (pts[i][1] - pts[i - 1][1]) * f });
            }
            res.Add(pts[i]);
        }
        return res;
    }

    private static double Dist(double[] a, double[] b)
    {
        double dx = a[0] - b[0], dy = a[1] - b[1];
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double MinCam(double metros) { return metros / MetrosPorMinCaminando; }
    private static double Espera(Linea l) { return Math.Max(2, l.R.FrecuenciaMin / 2.0); }
    private static int Min(double metros) { return (int)Math.Round(metros / MetrosPorMinCaminando); }

    // ── Grafo: cada parada de cada ruta es un nodo ────────────────────────
    private static List<Nodo> ConstruirGrafo(List<Linea> lineas)
    {
        var nodos = new List<Nodo>();
        var primero = new Dictionary<Linea, int>();
        foreach (var l in lineas)
        {
            primero[l] = nodos.Count;
            foreach (int i in l.Paradas) nodos.Add(new Nodo { L = l, Idx = i, M = l.M[i] });
        }

        // Viajar en la misma ruta, en cualquier sentido, entre paradas consecutivas
        foreach (var l in lineas)
        {
            int b = primero[l];
            for (int s = 0; s + 1 < l.Paradas.Count; s++)
            {
                double min = (l.Cum[l.Paradas[s + 1]] - l.Cum[l.Paradas[s]]) / MetrosPorMinBus;
                nodos[b + s].Sal.Add(new Arista { A = b + s, B = b + s + 1, Costo = min, Viaje = true });
                nodos[b + s + 1].Sal.Add(new Arista { A = b + s + 1, B = b + s, Costo = min, Viaje = true });
            }
        }

        // Transbordo: caminar a una parada de otra ruta que quede cerca
        for (int a = 0; a < nodos.Count; a++)
            for (int b = 0; b < nodos.Count; b++)
            {
                if (nodos[a].L == nodos[b].L || nodos[a].L.R.Numero == nodos[b].L.R.Numero) continue;
                double c = Dist(nodos[a].M, nodos[b].M);
                if (c > MaxCaminataTransbordoMetros) continue;
                nodos[a].Sal.Add(new Arista
                {
                    A = a, B = b, Viaje = false,
                    Costo = MinCam(c) + Espera(nodos[b].L) + PenalizacionTransbordoMin
                });
            }
        return nodos;
    }

    // ── Camino más corto (Dijkstra) ───────────────────────────────────────
    private static Opcion Buscar(List<Nodo> nodos, HashSet<Linea> vetadas, double[] O, double[] D,
                                 double[] origen, double[] destino)
    {
        int n = nodos.Count;
        var costo = new double[n];
        var previo = new int[n];
        var hecho = new bool[n];
        for (int i = 0; i < n; i++)
        {
            costo[i] = double.MaxValue;
            previo[i] = -1;
            if (vetadas.Contains(nodos[i].L)) { hecho[i] = true; continue; }
            double w = Dist(O, nodos[i].M);
            if (w <= MaxCaminataMetros) costo[i] = MinCam(w) + Espera(nodos[i].L);   // empezar caminando hasta esta parada
        }

        while (true)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
                if (!hecho[i] && costo[i] < double.MaxValue && (u < 0 || costo[i] < costo[u])) u = i;
            if (u < 0) break;
            hecho[u] = true;

            foreach (var e in nodos[u].Sal)
            {
                if (hecho[e.B]) continue;
                double c = costo[u] + e.Costo;
                if (c < costo[e.B]) { costo[e.B] = c; previo[e.B] = u; }
            }
        }

        int fin = -1;
        double mejor = double.MaxValue;
        for (int i = 0; i < n; i++)
        {
            if (costo[i] == double.MaxValue || vetadas.Contains(nodos[i].L)) continue;
            double w = Dist(D, nodos[i].M);
            if (w > MaxCaminataMetros) continue;
            double t = costo[i] + MinCam(w);
            if (t < mejor) { mejor = t; fin = i; }
        }
        if (fin < 0) return null;

        // Reconstruir: lista de nodos desde la parada donde subes hasta donde bajas
        var camino = new List<int>();
        for (int v = fin; v >= 0; v = previo[v]) camino.Add(v);
        camino.Reverse();

        // Agrupar en tramos de camión (una ruta cada uno)
        var tramosBus = new List<int[]>();   // {nodoInicio, nodoFin}
        int ini = camino[0];
        for (int s = 1; s < camino.Count; s++)
        {
            if (nodos[camino[s]].L != nodos[camino[s - 1]].L)
            {
                tramosBus.Add(new[] { ini, camino[s - 1] });
                ini = camino[s];
            }
        }
        tramosBus.Add(new[] { ini, camino[camino.Count - 1] });

        foreach (var t in tramosBus)
        {
            var l = nodos[t[0]].L;
            if (Math.Abs(l.Cum[nodos[t[1]].Idx] - l.Cum[nodos[t[0]].Idx]) < ViajeMinimoMetros) return null;
        }

        var o = new Opcion { Total = mejor };
        double c1 = Dist(O, nodos[tramosBus[0][0]].M);
        double c2 = Dist(D, nodos[tramosBus[tramosBus.Count - 1][1]].M);
        o.CaminaM = c1 + c2;

        var partes = new List<string> { "Camina " + Min(c1) + " min" };
        o.Tramos.Add(new Tramo { EsBus = false, Puntos = new List<double[]> { origen, nodos[tramosBus[0][0]].L.P[nodos[tramosBus[0][0]].Idx] } });

        for (int t = 0; t < tramosBus.Count; t++)
        {
            var a = nodos[tramosBus[t][0]];
            var b = nodos[tramosBus[t][1]];
            bool ultimo = t == tramosBus.Count - 1;

            if (t > 0)
            {
                var prev = nodos[tramosBus[t - 1][1]];
                double cam = Dist(prev.M, a.M);
                o.CaminaM += cam;
                if (cam > 5) o.Tramos.Add(new Tramo { EsBus = false, Puntos = new List<double[]> { prev.L.P[prev.Idx], a.L.P[a.Idx] } });
                partes.Add("Transbordo");
            }

            string inicio = t == 0 ? "Sube aquí · " + a.L.R.Nombre : "Transbordo · sube " + a.L.Corto;
            o.Tramos.Add(Bus(a.L, a.Idx, b.Idx, inicio, ultimo ? "Baja aquí" : null));
            o.Lineas.Add(a.L);
            partes.Add("Bus " + (int)Math.Round(Math.Abs(a.L.Cum[b.Idx] - a.L.Cum[a.Idx]) / MetrosPorMinBus) + " min");
        }

        var ult = nodos[tramosBus[tramosBus.Count - 1][1]];
        o.Tramos.Add(new Tramo { EsBus = false, Puntos = new List<double[]> { ult.L.P[ult.Idx], destino } });
        partes.Add("Camina " + Min(c2) + " min");
        o.Resumen = string.Join(" · ", partes);
        return o;
    }

    private static Tramo Bus(Linea l, int i, int j, string inicio, string fin)
    {
        var pts = new List<double[]>();
        if (i <= j) for (int x = i; x <= j; x++) pts.Add(l.P[x]);
        else for (int x = i; x >= j; x--) pts.Add(l.P[x]);
        return new Tramo { EsBus = true, Puntos = pts, TextoInicio = inicio, TextoFin = fin };
    }

    // ── Convertir la opción en una Ruta que Comparar sabe mostrar ─────────
    private static Ruta Convertir(Opcion o)
    {
        var a = o.Lineas[0].R;
        int cambios = o.Lineas.Count - 1;
        return new Ruta
        {
            Numero = a.Numero,
            Nombre = cambios > 0 ? string.Join(" + ", o.Lineas.Select(l => l.Corto)) : (o.Lineas[0].Ejemplo ? o.Lineas[0].Corto : a.Nombre),
            Modo = a.Modo,
            ModoTexto = a.ModoTexto,
            Lineas = string.Join(" → ", o.Lineas.Select(l => l.Ejemplo ? "Bus ejemplo " + (l.R.Numero - 900) : "Bus " + l.R.Numero)),
            Tarifa = o.Lineas.Sum(l => l.R.Tarifa),
            FrecuenciaMin = a.FrecuenciaMin,
            Horario = a.Horario,
            Descripcion = a.Descripcion,
            Guardada = false,   // la estrella de una opción de viaje es del viaje, no de la ruta base
            Transbordos = cambios,
            Minutos = (int)Math.Round(o.Total),
            EsViaje = true,
            Resumen = o.Resumen,
            CaminataMetros = (int)Math.Round(o.CaminaM),
            Tramos = o.Tramos,
            LatO = o.Tramos[0].Puntos[0][0], LngO = o.Tramos[0].Puntos[0][1]
        };
    }
}
