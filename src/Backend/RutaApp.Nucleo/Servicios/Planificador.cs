using System.Globalization;

namespace RutaApp.Nucleo.Servicios;

// Arma opciones de viaje con el camino más corto sobre la red de paradas: camina a una parada, viaja por una ruta, baja
// y transborda (2 o más camiones). Solo usa rutas reales: si el origen o el destino queda lejos de la red, no inventa ninguna.
public static class Planificador
{
    private const double MetrosPorMinCaminando = 80;      // ~4.8 km/h
    private const double MetrosPorMinBus = 333;           // ~20 km/h en ciudad
    private const double MaxCaminataMetros = 1000;        // más lejos que esto no se considera "cerca"
    private const double MaxCaminataTransbordoMetros = 350;
    private const double SeparacionParadasMetros = 400;
    private const double ViajeMinimoMetros = 250;
    private const double PenalizacionTransbordoMin = 3;
    private const double ParadaCercaDestinoMetros = 500;   // si hay una parada real a menos de esto del destino, se recomienda bajar ahí
    private const double ParadaPegadaAlRecorridoMetros = 120;
    private const double PesoCaminata = 2.0;                // un minuto caminando cuenta como 2: se prefiere la parada más cercana y caminar menos
    private const double ZonaVentajaMetros = 150;           // si el recorrido pasa a 150 m o más cerca que la parada, se recomienda esa zona
    private const double DistanciaMaximaMetros = 40000;   // más lejos que esto no se calcula

    private class Linea
    {
        public Ruta R;
        public List<double[]> P;      // trazo [lat, lng]
        public double[][] M;          // trazo en metros (x, y)
        public double[] Cum;          // distancia acumulada a lo largo del trazo
        public List<int> Paradas;     // índices del trazo que son parada
        public HashSet<int> Reales = new HashSet<int>();                 // índices del trazo que son paradas reales (base de datos)
        public Dictionary<int, string> Nombres = new Dictionary<int, string>();
        public HashSet<int> Zonas = new HashSet<int>();                  // puntos del recorrido por donde pasa el camión muy cerca del origen o destino
        public bool TieneReales { get { return Reales.Count > 0; } }
        public List<bool> Original;   // true = el punto viene del trazo original (no fue agregado al densificar)
        public string Corto { get { return string.IsNullOrWhiteSpace(R.Nombre) ? "Ruta " + R.Numero : R.Nombre; } }
    }

    private class Arista { public int A, B; public double Costo; public double Real; public bool Viaje; }   // Costo: con el peso de caminar; Real: minutos de verdad

    private class Nodo
    {
        public Linea L;
        public int Idx;
        public double[] M;
        public bool Real;          // parada real de la base de datos
        public string Nombre;      // nombre de la parada real
        public bool Zona;          // zona del recorrido (no es una parada) donde el camión pasa cerca
        public List<Arista> Sal = new List<Arista>();
    }

    private class Opcion
    {
        public double Total, CaminaM;
        public List<Tramo> Tramos = new List<Tramo>();
        public string Resumen = "";
        public List<Linea> Lineas = new List<Linea>();
    }

    public static List<Ruta> Generar(IEnumerable<Ruta> rutas, double oLat, double oLng, double dLat, double dLng)
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
            var l = Construir(r, aM);
            if (l != null) lineas.Add(l);
        }

        // El recorrido pasa a veces más cerca del usuario o del destino que cualquier parada: esos puntos también cuentan
        foreach (var l in lineas) { AgregarZonas(l, O); AgregarZonas(l, D); }

        var nodos = ConstruirGrafo(lineas);
        HashSet<int> inicioPermitido, finPermitido;
        ElegirExtremos(nodos, O, D, out inicioPermitido, out finPermitido);

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
            var o = Buscar(nodos, vetadas, O, D, origen, destino, inicioPermitido, finPermitido);
            if (o == null) continue;
            if (!firmas.Add(string.Join("+", o.Lineas.Select(l => l.R.Numero)))) continue;
            opciones.Add(o);
            foreach (var l in o.Lineas)
                pendientes.Enqueue(new HashSet<Linea>(vetadas) { l });
        }

        // El algoritmo decide: menos tiempo, poca caminata, pocos transbordos y tarifa baja
        Func<Opcion, double> puntaje = o => o.Total + (PesoCaminata - 1) * MinCam(o.CaminaM) + 2 * (o.Lineas.Count - 1) + (double)o.Lineas.Sum(l => l.R.Tarifa);

        var resultado = opciones.OrderBy(puntaje).Take(4).Select(Convertir).ToList();
        Etiquetar(resultado);
        return resultado;
    }

    // Pone las etiquetas Recomendado / Más rápido / Económico / Menos caminata a una lista de opciones.
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

    private static Linea Construir(Ruta r, Func<double[], double[]> aM)
    {
        var bruto = r.Trazo != null && r.Trazo.Count >= 2
            ? r.Trazo
            : new List<double[]> { new[] { r.LatO, r.LngO }, new[] { r.LatD, r.LngD } };

        var original = new List<bool>();
        var p = Densificar(bruto, aM, 100, original);
        var m = p.Select(aM).ToArray();
        var cum = new double[p.Count];
        for (int i = 1; i < p.Count; i++) cum[i] = cum[i - 1] + Dist(m[i - 1], m[i]);
        if (cum[cum.Length - 1] < ViajeMinimoMetros) return null;

        // Paradas reales de la ruta: cada una se ubica en el punto más cercano del trazo
        var linea = new Linea { R = r, P = p, M = m, Cum = cum, Original = original };
        if (r.Oficial != null)
        {
            foreach (var parada in r.Oficial.Paradas)
            {
                var pm = aM(new[] { parada.Latitud, parada.Longitud });
                int mejor = -1;
                double dMin = double.MaxValue;
                for (int i = 0; i < p.Count; i++)
                {
                    double d = Dist(pm, m[i]);
                    if (d < dMin) { dMin = d; mejor = i; }
                }
                if (mejor < 0 || dMin > ParadaPegadaAlRecorridoMetros) continue;   // parada que no cae sobre el recorrido: se ignora
                linea.Reales.Add(mejor);
                if (!string.IsNullOrWhiteSpace(parada.Nombre)) linea.Nombres[mejor] = parada.Nombre.Replace(", Chihuahua, Chihuahua", "").Trim();
            }
        }

        // Además de las reales hay puntos cada ~400 m: sirven si no hay una parada real cerca
        var paradas = new List<int>(linea.Reales);
        double ultima = 0;
        for (int i = 0; i < p.Count; i++)
        {
            bool extremo = i == 0 || i == p.Count - 1;
            if (!extremo && cum[i] - ultima < SeparacionParadasMetros) continue;
            ultima = cum[i];
            if (linea.Reales.Any(q => Math.Abs(cum[q] - cum[i]) < 150)) continue;   // ya hay una parada real ahí
            paradas.Add(i);
        }
        paradas.Sort();
        linea.Paradas = paradas.Distinct().ToList();
        return linea;
    }

    // Agrega puntos intermedios para que ninguna parada quede lejos del trazo real
    private static List<double[]> Densificar(List<double[]> pts, Func<double[], double[]> aM, double paso, List<bool> original)
    {
        var res = new List<double[]> { pts[0] };
        original.Add(true);
        for (int i = 1; i < pts.Count; i++)
        {
            double d = Dist(aM(pts[i - 1]), aM(pts[i]));
            int n = (int)Math.Floor(d / paso);
            for (int s = 1; s <= n; s++)
            {
                double f = (double)s / (n + 1);
                res.Add(new[] { pts[i - 1][0] + (pts[i][0] - pts[i - 1][0]) * f, pts[i - 1][1] + (pts[i][1] - pts[i - 1][1]) * f });
                original.Add(false);
            }
            res.Add(pts[i]);
            original.Add(true);
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

    // Grafo: cada parada de cada ruta es un nodo
    private static List<Nodo> ConstruirGrafo(List<Linea> lineas)
    {
        var nodos = new List<Nodo>();
        var primero = new Dictionary<Linea, int>();
        foreach (var l in lineas)
        {
            primero[l] = nodos.Count;
            foreach (int i in l.Paradas)
            {
                bool real = l.Reales.Contains(i);
                nodos.Add(new Nodo { L = l, Idx = i, M = l.M[i], Real = real, Zona = !real && l.Zonas.Contains(i),
                                     Nombre = real && l.Nombres.ContainsKey(i) ? l.Nombres[i] : null });
            }
        }

        // El camión solo avanza en el sentido del recorrido (son circuitos de ida y vuelta): si ya pasó por una zona,
        // para volver ahí hay que dar toda la vuelta. Al llegar al final del circuito sigue desde el principio.
        foreach (var l in lineas)
        {
            int b = primero[l];
            int total = l.Paradas.Count;
            for (int s = 0; s + 1 < total; s++)
            {
                double min = (l.Cum[l.Paradas[s + 1]] - l.Cum[l.Paradas[s]]) / MetrosPorMinBus;
                nodos[b + s].Sal.Add(new Arista { A = b + s, B = b + s + 1, Costo = min, Real = min, Viaje = true });
            }
            if (total > 1 && Dist(l.M[0], l.M[l.M.Length - 1]) < 100)
                nodos[b + total - 1].Sal.Add(new Arista { A = b + total - 1, B = b, Costo = 0, Real = 0, Viaje = true });
        }

        // Transbordo: caminar a una parada de otra ruta que quede cerca
        for (int a = 0; a < nodos.Count; a++)
            for (int b = 0; b < nodos.Count; b++)
            {
                if (nodos[a].L == nodos[b].L || nodos[a].L.R.Numero == nodos[b].L.R.Numero) continue;
                if (!Elegible(nodos[a]) || !Elegible(nodos[b])) continue;   // en rutas con paradas reales solo se transborda en paradas
                double c = Dist(nodos[a].M, nodos[b].M);
                if (c > MaxCaminataTransbordoMetros) continue;
                nodos[a].Sal.Add(new Arista
                {
                    A = a, B = b, Viaje = false,
                    Costo = MinCam(c) * PesoCaminata + Espera(nodos[b].L) + PenalizacionTransbordoMin,
                    Real = MinCam(c) + Espera(nodos[b].L) + PenalizacionTransbordoMin
                });
            }
        return nodos;
    }

    // En una ruta con paradas reales solo esas paradas sirven para transbordar; si no las tiene, cualquier punto
    private static bool Elegible(Nodo n) { return !n.L.TieneReales || n.Real; }

    // Puntos del recorrido donde el camión pasa más cerca de "punto" (uno por cada pasada: ida y vuelta pueden estar en calles distintas)
    private static void AgregarZonas(Linea l, double[] punto)
    {
        int n = l.P.Count;
        var d = new double[n];
        for (int i = 0; i < n; i++) d[i] = Dist(punto, l.M[i]);

        var minimos = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if (d[i] > MaxCaminataMetros) continue;
            bool izquierda = i == 0 || d[i] <= d[i - 1];
            bool derecha = i == n - 1 || d[i] < d[i + 1];
            if (izquierda && derecha) minimos.Add(i);
        }
        foreach (int i in minimos.OrderBy(x => d[x]).Take(4))
        {
            if (l.Reales.Contains(i)) continue;   // ahí ya hay una parada real
            l.Zonas.Add(i);
            if (!l.Paradas.Contains(i)) l.Paradas.Add(i);
        }
        l.Paradas.Sort();
    }

    // Dónde se puede subir y bajar en cada pasada de cada ruta:
    //  - la parada real más cercana (al usuario; al destino solo si queda a menos de ParadaCercaDestinoMetros);
    //  - pero si el recorrido pasa bastante más cerca que esa parada (ZonaVentajaMetros), se recomienda esa zona;
    //  - si no hay parada cerca, se usa la zona por donde pasa el camión.
    private static void ElegirExtremos(List<Nodo> nodos, double[] O, double[] D, out HashSet<int> inicio, out HashSet<int> fin)
    {
        inicio = new HashSet<int>();
        fin = new HashSet<int>();
        foreach (var grupo in Enumerable.Range(0, nodos.Count).GroupBy(i => nodos[i].L))
        {
            var ids = grupo.OrderBy(i => nodos[i].Idx).ToList();
            ElegirEn(nodos, ids, O, MaxCaminataMetros, inicio);
            ElegirEn(nodos, ids, D, ParadaCercaDestinoMetros, fin);
        }
    }

    private static void ElegirEn(List<Nodo> nodos, List<int> ids, double[] punto, double maxParada, HashSet<int> resultado)
    {
        var l = nodos[ids[0]].L;
        var reales = MinimosLocales(nodos, ids.Where(i => nodos[i].Real).ToList(), punto, maxParada);
        var zonas = MinimosLocales(nodos, ids.Where(i => nodos[i].Zona).ToList(), punto, MaxCaminataMetros);

        var usadas = new HashSet<int>();
        foreach (int z in zonas)
        {
            int r = -1;
            double dr = double.MaxValue;
            foreach (int x in reales)
            {
                if (Math.Abs(l.Cum[nodos[x].Idx] - l.Cum[nodos[z].Idx]) >= 800) continue;   // otra pasada de la ruta
                double d = Dist(punto, nodos[x].M);
                if (d < dr) { dr = d; r = x; }
            }
            if (r >= 0) usadas.Add(r);
            if (r < 0 || Dist(punto, nodos[z].M) + ZonaVentajaMetros < dr) resultado.Add(z);
            else resultado.Add(r);
        }
        foreach (int r in reales) if (!usadas.Contains(r)) resultado.Add(r);
    }

    // De una lista de nodos en orden del recorrido, los que están más cerca de "punto" que sus vecinos (uno por pasada)
    private static List<int> MinimosLocales(List<Nodo> nodos, List<int> ids, double[] punto, double maxMetros)
    {
        var res = new List<int>();
        for (int k = 0; k < ids.Count; k++)
        {
            double d = Dist(punto, nodos[ids[k]].M);
            if (d > maxMetros) continue;
            bool izquierda = k == 0 || d <= Dist(punto, nodos[ids[k - 1]].M);
            bool derecha = k == ids.Count - 1 || d < Dist(punto, nodos[ids[k + 1]].M);
            if (izquierda && derecha) res.Add(ids[k]);
        }
        return res;
    }

    private static Opcion Buscar(List<Nodo> nodos, HashSet<Linea> vetadas, double[] O, double[] D,
                                 double[] origen, double[] destino, HashSet<int> inicioOk, HashSet<int> finOk)
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
            if (inicioOk.Contains(i)) costo[i] = MinCam(w) * PesoCaminata + Espera(nodos[i].L);   // empezar caminando hasta la parada
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
            if (!finOk.Contains(i)) continue;
            double w = Dist(D, nodos[i].M);
            double t = costo[i] + MinCam(w) * PesoCaminata;
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
            if (Avance(l, nodos[t[0]].Idx, nodos[t[1]].Idx) < ViajeMinimoMetros) return null;
        }

        // Minutos reales del viaje (sin el peso extra de caminar): caminar, esperar, ir en camión y transbordar
        double minutosReales = MinCam(Dist(O, nodos[camino[0]].M)) + Espera(nodos[camino[0]].L);
        for (int s = 1; s < camino.Count; s++)
            minutosReales += nodos[camino[s - 1]].Sal.First(x => x.B == camino[s]).Real;
        minutosReales += MinCam(Dist(D, nodos[camino[camino.Count - 1]].M));

        var o = new Opcion { Total = minutosReales };
        double c1 = Dist(O, nodos[tramosBus[0][0]].M);
        double c2 = Dist(D, nodos[tramosBus[tramosBus.Count - 1][1]].M);
        o.CaminaM = c1 + c2;

        var subida = nodos[tramosBus[0][0]];
        var partes = new List<string> { "Camina " + Min(c1) + " min" + (subida.Nombre != null ? " a " + subida.Nombre : subida.Zona ? " hasta donde pasa el camión" : "") };
        o.Tramos.Add(Camina(origen, nodos[tramosBus[0][0]].L.P[nodos[tramosBus[0][0]].Idx], c1));

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
                if (cam > 5) o.Tramos.Add(Camina(prev.L.P[prev.Idx], a.L.P[a.Idx], cam));
                partes.Add("Transbordo");
            }

            string aqui = a.Nombre != null ? "Sube en " + a.Nombre : a.Zona ? "Sube en esta zona" : "Sube aquí";
            string textoInicio = t == 0 ? aqui + " · " + a.L.Corto
                : "Transbordo · " + (a.Nombre != null ? "sube en " + a.Nombre : a.Zona ? "sube en esta zona" : "sube") + " · " + a.L.Corto;
            string textoFin = ultimo ? (b.Nombre != null ? "Baja en " + b.Nombre : b.Zona ? "Baja en esta zona" : "Baja aquí") : null;
            var tramoBus = Bus(a.L, a.Idx, b.Idx, textoInicio, textoFin);
            tramoBus.ParadaInicio = a.Nombre ?? (a.Zona ? "la zona señalada" : null);
            tramoBus.ParadaFin = b.Nombre ?? (b.Zona ? "la zona señalada" : null);
            tramoBus.EsperaMin = a.L.R.FrecuenciaMin;
            tramoBus.Simbolo = a.L.R.Simbolo;
            tramoBus.Patron = a.L.R.Patron;
            o.Tramos.Add(tramoBus);
            o.Lineas.Add(a.L);
            partes.Add("Bus " + (int)Math.Round(Avance(a.L, a.Idx, b.Idx) / MetrosPorMinBus) + " min");
        }

        var ult = nodos[tramosBus[tramosBus.Count - 1][1]];
        o.Tramos.Add(Camina(ult.L.P[ult.Idx], destino, c2));
        partes.Add("Camina " + Min(c2) + " min");
        o.Resumen = string.Join(" · ", partes);
        return o;
    }

    // Metros que recorre el camión de la parada i a la j, siempre hacia adelante (si j quedó atrás, da la vuelta al circuito)
    private static double Avance(Linea l, int i, int j)
    {
        if (j >= i) return l.Cum[j] - l.Cum[i];
        return (l.Cum[l.Cum.Length - 1] - l.Cum[i]) + l.Cum[j];
    }

    private static Tramo Bus(Linea l, int i, int j, string inicio, string fin)
    {
        var pts = new List<double[]>();
        // Se usan los puntos reales del recorrido (no los agregados en línea recta al densificar)
        Action<int> agregar = x => { if (l.Original[x] || x == i || x == j) pts.Add(l.P[x]); };
        if (i <= j) { for (int x = i; x <= j; x++) agregar(x); }
        else
        {
            for (int x = i; x < l.P.Count; x++) agregar(x);   // hasta el final del circuito...
            for (int x = 0; x <= j; x++) agregar(x);           // ...y desde el principio
        }
        return new Tramo
        {
            EsBus = true, Puntos = pts, TextoInicio = inicio, TextoFin = fin, Linea = l.Corto, Color = l.R.Color,
            Minutos = (int)Math.Round(Avance(l, i, j) / MetrosPorMinBus),
            AjustarACalles = !l.R.TrazoEnCalles,                  // si el recorrido solo une paradas, se ajusta a las calles al mostrarlo
            Reparar = l.R.TrazoEnCalles && !l.R.Reparado           // recorrido guardado: se corrigen los tramos que cortan esquinas
        };
    }

    // Tramo a pie: se dibuja recto al principio y se ajusta a las banquetas al mostrarlo
    private static Tramo Camina(double[] desde, double[] hasta, double metros)
    {
        return new Tramo { EsBus = false, Puntos = new List<double[]> { desde, hasta }, Minutos = Min(metros), Metros = (int)Math.Round(metros / 10.0) * 10, AjustarACalles = true };
    }

    // Convertir la opción en una Ruta que Comparar sabe mostrar
    private static Ruta Convertir(Opcion o)
    {
        var a = o.Lineas[0].R;
        int cambios = o.Lineas.Count - 1;
        return new Ruta
        {
            Numero = a.Numero,
            Nombre = cambios > 0 ? string.Join(" + ", o.Lineas.Select(l => l.Corto)) : a.Nombre,
            Modo = a.Modo,
            ModoTexto = a.ModoTexto,
            Lineas = string.Join(" → ", o.Lineas.Select(l => l.Corto)),
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
