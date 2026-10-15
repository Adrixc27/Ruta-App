using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ruta_App
{
    // ─────────────────────────────────────────────────────────────
    //  Infraestructura pequeña
    // ─────────────────────────────────────────────────────────────

    /// <summary>Las vistas que quieran enterarse cuando se navega hacia ellas implementan esto.</summary>
    public interface INavegable
    {
        void AlNavegar(object parametro);
    }

    /// <summary>Convierte el nombre de un recurso ("IcoBus") en la Geometry correspondiente.</summary>
    public class IconoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clave = value as string;
            if (clave == null) return null;
            return Application.Current.TryFindResource(clave);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propiedad)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(propiedad));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Modelos
    // ─────────────────────────────────────────────────────────────

    public class Ruta : Observable
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = "";
        public string Modo { get; set; } = "Autobús";        // para los filtros: Autobús / Exprés / Metro
        public string ModoTexto { get; set; } = "Autobús";   // lo que se muestra: "Metro + Bus"
        public int FrecuenciaMin { get; set; } = 10;
        public decimal Tarifa { get; set; } = 2.75m;
        public bool Desvio { get; set; }
        public string Horario { get; set; } = "5:30 - 23:00";
        public int Paradas { get; set; } = 20;
        public int Minutos { get; set; } = 20;
        public int Transbordos { get; set; }
        public string Descripcion { get; set; } = "";         // texto de la tabla routes
        public string Lineas { get; set; } = "";             // chip de la pantalla Comparar
        public string Etiqueta { get; set; } = "";           // Recomendado / Más rápido / Económico

        public string OrigenNombre { get; set; } = "";
        public string DestinoNombre { get; set; } = "";
        public double LatO { get; set; }
        public double LngO { get; set; }
        public double LatD { get; set; }
        public double LngD { get; set; }

        // Textos ya formateados para los bindings
        public string NumeroTexto { get { return Numero.ToString(); } }
        public string TarifaTexto { get { return "$" + Tarifa.ToString("0.00", CultureInfo.InvariantCulture); } }
        public string MinutosTexto { get { return Minutos + " min"; } }
        public string Meta { get { return ModoTexto + " · " + MinutosTexto + " · " + TarifaTexto; } }
        public string Subtitulo { get { return string.IsNullOrEmpty(Descripcion) ? ModoTexto + " · " + Horario : Descripcion; } }
        public string FrecuenciaTexto
        {
            get { return string.IsNullOrEmpty(Resumen) ? "Cada " + FrecuenciaMin + " min desde parada central" : Resumen; }
        }

        // Cuando la ruta es una opción de viaje (camina → camión → camina) hacia un destino concreto
        public bool EsViaje { get; set; }
        public string Resumen { get; set; } = "";        // "Camina 4 min · Bus 12 min · Camina 3 min"
        public double ViajeLatO { get; set; } = double.NaN;  // dónde está el usuario
        public double ViajeLngO { get; set; }
        public double ViajeLatD { get; set; } = double.NaN;  // a dónde quiere llegar
        public double ViajeLngD { get; set; }
        public int CaminataMetros { get; set; }
        public List<double[]> Trazo { get; set; }        // recorrido de la ruta [lat, lng] en orden (por calles)
        public List<double[]> Waypoints { get; set; }    // puntos clave por donde pasa (para calcular el Trazo)
        public List<Tramo> Tramos { get; set; }          // solo en opciones de viaje: caminar / bus / caminar...
        public List<Sugerencia> Escalas { get; set; }    // paradas intermedias del viaje (A → B → C)
        public string EtiquetaMayus { get { return Etiqueta.ToUpperInvariant(); } }
        public string TransbordosTexto
        {
            get
            {
                if (Transbordos == 0) return "Sin transbordos";
                return Transbordos == 1 ? "1 transbordo" : Transbordos + " transbordos";
            }
        }

        private bool _guardada;
        public bool Guardada
        {
            get { return _guardada; }
            set { if (_guardada == value) return; _guardada = value; Notify("Guardada"); }
        }

        private bool _seleccionada;
        public bool Seleccionada
        {
            get { return _seleccionada; }
            set { if (_seleccionada == value) return; _seleccionada = value; Notify("Seleccionada"); }
        }
    }

    public class Reciente
    {
        public Ruta Ruta { get; set; }
        public string Texto { get; set; } = "";
    }

    public class Trayecto : Observable
    {
        public string Alias { get; set; } = "";
        public string Recorrido { get; set; } = "";   // "Kennedy > BOW"
        public string Detalle { get; set; } = "";     // "18 min, de lunes a viernes"

        private bool _favorito;
        public bool Favorito
        {
            get { return _favorito; }
            set { if (_favorito == value) return; _favorito = value; Notify("Favorito"); }
        }
    }

    public class Etiqueta
    {
        public string Texto { get; set; } = "";
        public bool EsRuta { get; set; }
        public string Icono { get { return EsRuta ? "IcoBus" : "IcoPin"; } }
    }

    public class Publicacion : Observable
    {
        public string Tipo { get; set; } = "aviso";   // aviso | pregunta | propuesta
        public string Autor { get; set; } = "";
        public bool Oficial { get; set; }
        public string Tiempo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Cuerpo { get; set; } = "";
        public List<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
        public int Comentarios { get; set; }
        public int Orden { get; set; }        // mayor = más reciente
        public int RutaNumero { get; set; }   // 0 = no está ligada a una ruta

        private int _likes;
        public int Likes
        {
            get { return _likes; }
            set { if (_likes == value) return; _likes = value; Notify("Likes"); }
        }

        private bool _meGusta;
        public bool MeGusta
        {
            get { return _meGusta; }
            set { if (_meGusta == value) return; _meGusta = value; Notify("MeGusta"); }
        }

        public string Inicial { get { return Autor.Length > 0 ? Autor.Substring(0, 1).ToUpperInvariant() : "?"; } }
        public string TipoTexto { get { return Tipo.ToUpperInvariant(); } }
        public string IconoTipo { get { return Tipo == "aviso" ? "IcoAlert" : (Tipo == "pregunta" ? "IcoHelp" : "IcoBulb"); } }
        public string Cta { get { return Tipo == "pregunta" ? "Responder" : "Ver en el mapa"; } }
    }

    // ─────────────────────────────────────────────────────────────
    //  Utilidades de mapa
    // ─────────────────────────────────────────────────────────────

    /// <summary>Un tramo del viaje: caminando (punteado) o en camión (línea azul).</summary>
    public class Tramo
    {
        public bool EsBus { get; set; }
        public List<double[]> Puntos { get; set; } = new List<double[]>();   // [lat, lng]
        public string TextoInicio { get; set; }   // etiqueta de la parada donde empieza (solo camión)
        public string TextoFin { get; set; }      // etiqueta de la parada donde termina (solo camión)
    }

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
                                                          double dLat, double dLng)
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

            await AgregarRutasEjemploAsync(lineas, origen, destino, O, D, aM);

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
                                                           double[] O, double[] D, Func<double[], double[]> aM)
        {
            var tareas = new List<Task<Linea>>();

            if (lineas.Count == 0)
            {
                tareas.Add(CrearEjemploAsync(901, origen, destino, aM));
            }
            else
            {
                double[] cercaO, cercaD;
                double mO = ParadaMasCercana(lineas, O, out cercaO);
                double mD = ParadaMasCercana(lineas, D, out cercaD);
                if (mO > CaminataParaRutaEjemplo) tareas.Add(CrearEjemploAsync(901, origen, cercaO, aM));
                if (mD > CaminataParaRutaEjemplo) tareas.Add(CrearEjemploAsync(902, cercaD, destino, aM));
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
        private static async Task<Linea> CrearEjemploAsync(int numero, double[] a, double[] b, Func<double[], double[]> aM)
        {
            if (Dist(aM(a), aM(b)) < ViajeMinimoMetros) return null;

            string coords = a[1].ToString("R", CultureInfo.InvariantCulture) + "," + a[0].ToString("R", CultureInfo.InvariantCulture) + ";" +
                            b[1].ToString("R", CultureInfo.InvariantCulture) + "," + b[0].ToString("R", CultureInfo.InvariantCulture);
            var trazo = await Estado.TrazoOsrmAsync(coords) ?? new List<double[]> { a, b };

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
                Guardada = a.Guardada,
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

    public static class Geo
    {
        public const double CentroLat = 28.6353;
        public const double CentroLng = -106.0889;

        /// <summary>
        /// Camino "escalonado" entre dos puntos (como el dibujo de Figma).
        /// Es solo demostrativo: cuando tengas rutas reales, reemplázalo por las coordenadas verdaderas.
        /// </summary>
        public static List<double[]> Camino(double latO, double lngO, double latD, double lngD)
        {
            double dLat = latD - latO;
            double dLng = lngD - lngO;
            var p1 = new[] { latO + dLat * 0.45, lngO };
            var p2 = new[] { p1[0], lngO + dLng * 0.55 };
            var p3 = new[] { latD, p2[1] };
            return new List<double[]>
            {
                new[] { latO, lngO }, p1, p2, p3, new[] { latD, lngD }
            };
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Estado compartido entre pantallas (sin base de datos: todo en memoria)
    // ─────────────────────────────────────────────────────────────

    public static class Estado
    {
        // Sesión
        public static string Nombre = "Laura Guerrero";
        public static string Correo = "laura.g@ruta.com";
        public static bool Invitado;

        // Búsqueda actual (Planificar → Comparar)
        public static string Origen = "Mi ubicación actual";
        public static string Destino = "";

        // Coordenadas de la búsqueda actual (las llena Planificar; NaN = desconocidas)
        public static double OrigenLat = double.NaN, OrigenLng, DestinoLat = double.NaN, DestinoLng;

        public static List<Sugerencia> Escalas = new List<Sugerencia>();   // paradas intermedias de la búsqueda

        /// <summary>Olvida la búsqueda actual (destino, paradas y coordenadas), por ejemplo al empezar el viaje.</summary>
        public static void NuevaBusqueda()
        {
            Destino = "";
            Escalas = new List<Sugerencia>();
            OrigenLat = double.NaN; DestinoLat = double.NaN;
        }

        public static bool HayCoordenadas
        {
            get { return !double.IsNaN(OrigenLat) && !double.IsNaN(DestinoLat); }
        }

        // Navegación: la asigna MainWindow
        public static Action<string, object> Navegar;

        // Datos
        public static List<Ruta> Rutas = DatosDemo.CrearRutas();   // ejemplo; se reemplaza con las de la base de datos si la API responde
        public static ObservableCollection<Reciente> Recientes = DatosDemo.CrearRecientes(Rutas);
        public static ObservableCollection<Trayecto> Trayectos = DatosDemo.CrearTrayectos();
        public static ObservableCollection<Publicacion> Publicaciones = DatosDemo.CrearPublicaciones();
        public static List<Publicacion> PublicacionesExtra = DatosDemo.CrearPublicacionesExtra();

        public static event Action GuardadasCambiaron;

        /// <summary>Carga las rutas de la base de datos (vía API). Devuelve false si no se pudo.</summary>
        public static async System.Threading.Tasks.Task<bool> CargarRutasRealesAsync()
        {
            var reales = await new DB.RutaRepositorio().CargarAsync();
            if (reales == null) return false;

            Rutas.Clear();
            Rutas.AddRange(reales);
            _preparando = null;

            Recientes.Clear();
            foreach (var r in reales.Take(2))
                Recientes.Add(new Reciente { Ruta = r, Texto = "Disponible" });
            return true;
        }

        // ── Recorridos de las rutas ───────────────────────────────────────────
        private static readonly System.Net.Http.HttpClient _httpTrazos =
            new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(8) };

        static Estado()
        {
            // TLS 1.2 / 1.3 para OSRM, Nominatim, etc. (en .NET Framework a veces no vienen activados)
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | (System.Net.SecurityProtocolType)12288;
            _httpTrazos.DefaultRequestHeaders.UserAgent.ParseAdd("RutaApp/1.0 (estudiante@universidad.com)");
        }
        private static readonly Dictionary<string, List<double[]>> _cacheOsrm = new Dictionary<string, List<double[]>>();
        private static System.Threading.Tasks.Task _preparando;

        /// <summary>
        /// Calcula el recorrido por calles de las rutas que tienen Waypoints (OSRM, todas a la vez) y lo guarda
        /// en disco para no volver a pedirlo. Si ya se está calculando, espera esa misma tarea.
        /// </summary>
        public static System.Threading.Tasks.Task PrepararTrazosAsync()
        {
            if (_preparando == null || _preparando.IsFaulted) _preparando = PrepararInterno();
            return _preparando;
        }

        private static async System.Threading.Tasks.Task PrepararInterno()
        {
            string dir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp");
            string archivo = System.IO.Path.Combine(dir, "trazos.json");
            var cache = new Dictionary<string, List<double[]>>();
            try
            {
                if (System.IO.File.Exists(archivo))
                    cache = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<double[]>>>(
                        System.IO.File.ReadAllText(archivo)) ?? cache;
            }
            catch { }

            var rutas = Rutas.Where(r => r.Waypoints != null && r.Waypoints.Count >= 2).ToList();
            var claves = rutas.Select(r => string.Join(";", r.Waypoints.Select(w =>
                w[1].ToString("R", CultureInfo.InvariantCulture) + "," + w[0].ToString("R", CultureInfo.InvariantCulture)))).ToList();

            // Las que faltan en el caché se piden todas al mismo tiempo
            var faltan = claves.Where(c => !cache.ContainsKey(c)).Distinct().ToList();
            var respuestas = await System.Threading.Tasks.Task.WhenAll(faltan.Select(c => TrazoOsrmAsync(c)));
            bool cambio = false;
            for (int i = 0; i < faltan.Count; i++)
                if (respuestas[i] != null) { cache[faltan[i]] = respuestas[i]; cambio = true; }

            bool completo = true;
            for (int n = 0; n < rutas.Count; n++)
            {
                var r = rutas[n];
                List<double[]> trazo;
                if (!cache.TryGetValue(claves[n], out trazo)) { completo = false; trazo = new List<double[]>(r.Waypoints); }
                r.Trazo = trazo;

                double metros = 0;
                double k = Math.Cos(trazo[0][0] * Math.PI / 180);
                for (int i = 1; i < trazo.Count; i++)
                {
                    double dx = (trazo[i][1] - trazo[i - 1][1]) * 111320 * k;
                    double dy = (trazo[i][0] - trazo[i - 1][0]) * 110574;
                    metros += Math.Sqrt(dx * dx + dy * dy);
                }
                r.Minutos = Math.Max(5, (int)Math.Round(metros / 333));
                r.Paradas = Math.Max(2, (int)(metros / 400) + 1);
            }

            if (cambio)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllText(archivo, System.Text.Json.JsonSerializer.Serialize(cache));
                }
                catch { }
            }
            if (!completo) _preparando = null;   // sin internet: se reintenta la próxima vez
        }

        /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM). null si no hay internet.</summary>
        public static async System.Threading.Tasks.Task<List<double[]>> TrazoOsrmAsync(string coordenadas)
        {
            lock (_cacheOsrm)
            {
                List<double[]> guardado;
                if (_cacheOsrm.TryGetValue(coordenadas, out guardado)) return guardado;
            }
            try
            {
                string json = await _httpTrazos.GetStringAsync(
                    "https://router.project-osrm.org/route/v1/driving/" + coordenadas + "?overview=full&geometries=geojson");
                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    var raiz = doc.RootElement;
                    if (raiz.GetProperty("code").GetString() != "Ok") return null;
                    var lista = new List<double[]>();
                    foreach (var c in raiz.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates").EnumerateArray())
                        lista.Add(new[] { c[1].GetDouble(), c[0].GetDouble() });
                    if (lista.Count < 2) return null;
                    lock (_cacheOsrm) { _cacheOsrm[coordenadas] = lista; }
                    return lista;
                }
            }
            catch { return null; }
        }

        public static int TotalGuardadas { get { return Rutas.Count(r => r.Guardada); } }

        public static Ruta BuscarRuta(int numero)
        {
            return Rutas.FirstOrDefault(r => r.Numero == numero);
        }

        public static void AlternarGuardada(Ruta ruta)
        {
            ruta.Guardada = !ruta.Guardada;
            var h = GuardadasCambiaron;
            if (h != null) h();
        }

        public static void AgregarReciente(Ruta ruta)
        {
            return; // la lista de Planificar muestra todas las rutas reales; no se recorta ni se reordena
            var existente = Recientes.FirstOrDefault(r => r.Ruta == ruta);
            if (existente != null) Recientes.Remove(existente);
            Recientes.Insert(0, new Reciente { Ruta = ruta, Texto = "Hace un momento" });
            while (Recientes.Count > 3) Recientes.RemoveAt(Recientes.Count - 1);
        }

        public static string Iniciales(string nombre)
        {
            var partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();
            return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Datos de demostración (los mismos textos del diseño de Figma)
    // ─────────────────────────────────────────────────────────────

    internal static class DatosDemo
    {
        // Rutas de ejemplo: cada una cruza la ciudad pasando por el Centro (Plaza Mayor), como las líneas troncales.
        // Los puntos (Waypoints) son aproximados; el recorrido por calles se calcula con OSRM (Estado.PrepararTrazosAsync).
        private static readonly double[] Centro = { 28.6357, -106.0877 };

        private static List<double[]> W(double lat1, double lng1, double lat2, double lng2)
        {
            return new List<double[]> { new[] { lat1, lng1 }, Centro, new[] { lat2, lng2 } };
        }

        public static List<Ruta> CrearRutas()
        {
            var lista = new List<Ruta>
            {
                new Ruta { Numero = 3, Nombre = "Ruta 3 - Exprés Centro", Modo = "Exprés", ModoTexto = "Exprés",
                    FrecuenciaMin = 10, Tarifa = 1.50m, Horario = "6:00 - 22:00", Lineas = "Bus Exprés", Etiqueta = "Económico",
                    OrigenNombre = "Terminal Norte", DestinoNombre = "Jardines de Oriente",
                    Waypoints = W(28.7000, -106.1100, 28.6200, -106.0250) },

                new Ruta { Numero = 4, Nombre = "Ruta 4 - Zona Centro", FrecuenciaMin = 12, Tarifa = 2.75m,
                    Desvio = true, Horario = "5:30 - 22:30", Lineas = "Bus 104",
                    OrigenNombre = "Terminal Sur", DestinoNombre = "Bosque Azul",
                    Waypoints = W(28.5900, -106.1000, 28.6900, -106.0450) },

                new Ruta { Numero = 6, Nombre = "Ruta 6 - Villas", FrecuenciaMin = 10, Tarifa = 2.75m,
                    Horario = "5:30 - 23:00", Lineas = "Bus 102", Etiqueta = "Recomendado", Guardada = true,
                    OrigenNombre = "Terminal Villas", DestinoNombre = "Colonia Avalos",
                    Waypoints = W(28.6100, -106.1450, 28.6300, -106.0400) },

                new Ruta { Numero = 7, Nombre = "Ruta 7 - Bosque Azul", Modo = "Metro", ModoTexto = "Metro + Bus",
                    FrecuenciaMin = 15, Tarifa = 3.50m, Horario = "5:00 - 22:30",
                    Lineas = "Metro L + Bus 12", Etiqueta = "Más rápido", Guardada = true,
                    OrigenNombre = "Komatsu", DestinoNombre = "Rancheria Juárez",
                    Waypoints = W(28.7100, -106.0600, 28.6150, -106.0450) },

                new Ruta { Numero = 9, Nombre = "Ruta 9 - Plan de Ayala", FrecuenciaMin = 12, Tarifa = 2.75m, Lineas = "Bus 109",
                    OrigenNombre = "Plan de Ayala", DestinoNombre = "Panamericana",
                    Waypoints = W(28.6620, -106.1200, 28.6050, -106.0600) },

                new Ruta { Numero = 12, Nombre = "Ruta 12 - Circunvalación II", FrecuenciaMin = 20, Tarifa = 2.75m,
                    Lineas = "Bus 112", Guardada = true,
                    OrigenNombre = "Circunvalación II", DestinoNombre = "Sur",
                    Waypoints = W(28.6800, -106.0950, 28.6000, -106.0850) },

                new Ruta { Numero = 15, Nombre = "Ruta 15 - Campesina", FrecuenciaMin = 18, Tarifa = 2.75m, Lineas = "Bus 115",
                    OrigenNombre = "Poniente", DestinoNombre = "Campesina",
                    Waypoints = W(28.6350, -106.1500, 28.6500, -106.0250) }
            };

            foreach (var r in lista)
            {
                var a = r.Waypoints[0]; var b = r.Waypoints[r.Waypoints.Count - 1];
                r.LatO = a[0]; r.LngO = a[1]; r.LatD = b[0]; r.LngD = b[1];
            }
            return lista;
        }

        public static ObservableCollection<Reciente> CrearRecientes(List<Ruta> rutas)
        {
            var lista = new ObservableCollection<Reciente>();
            foreach (var r in rutas) lista.Add(new Reciente { Ruta = r, Texto = r.TarifaTexto + " · " + r.MinutosTexto });
            return lista;
        }

        public static ObservableCollection<Trayecto> CrearTrayectos()
        {
            return new ObservableCollection<Trayecto>
            {
                new Trayecto { Alias = "Trabajo", Recorrido = "Kennedy > BOWI", Detalle = "18 min, de lunes a viernes" },
                new Trayecto { Alias = "Gimnasio", Recorrido = "Plan de Ayala > Komatsu", Detalle = "14 min" },
                new Trayecto { Alias = "Casa de mamá", Recorrido = "Circunvalacion II", Detalle = "25 min, domingos" }
            };
        }

        private static Etiqueta R(int n) { return new Etiqueta { Texto = "Ruta " + n, EsRuta = true }; }
        private static Etiqueta Z(string t) { return new Etiqueta { Texto = t, EsRuta = false }; }

        public static ObservableCollection<Publicacion> CrearPublicaciones()
        {
            return new ObservableCollection<Publicacion>
            {
                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 2 h",
                    Titulo = "Aviso de servicio",
                    Cuerpo = "La Ruta 4 tendrá un desvío temporal por obras en Zona Centro. Comparte alternativas con la comunidad.",
                    Etiquetas = new List<Etiqueta> { R(4), Z("Zona Centro") }, Likes = 12, Comentarios = 4, Orden = 100, RutaNumero = 4 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 5 h",
                    Titulo = "Tu experiencia importa",
                    Cuerpo = "¿Qué parada necesita mejor señalización? Cuéntanos cómo mejorar tus recorridos diarios.",
                    Etiquetas = new List<Etiqueta> { Z("Paradas") }, Likes = 19, Comentarios = 5, Orden = 90 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "ayer",
                    Titulo = "Nueva conexión",
                    Cuerpo = "Se propone extender la Ruta 7 hasta Bosque Azul. Revisa el mapa y deja tu opinión.",
                    Etiquetas = new List<Etiqueta> { R(7), Z("Bosque Azul") }, Likes = 26, Comentarios = 6, Orden = 80, RutaNumero = 7 },

                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 1 d",
                    Titulo = "Horario especial en Ruta 6",
                    Cuerpo = "Este domingo la Ruta 6 - Villas circulará cada 20 minutos por mantenimiento de unidades.",
                    Etiquetas = new List<Etiqueta> { R(6), Z("Villas") }, Likes = 8, Comentarios = 2, Orden = 70, RutaNumero = 6 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 1 d",
                    Titulo = "¿Horario de la Ruta 3 Exprés?",
                    Cuerpo = "Voy de Circunvalación II al centro y quiero saber a qué hora pasa el exprés por la mañana.",
                    Etiquetas = new List<Etiqueta> { R(3) }, Likes = 5, Comentarios = 9, Orden = 60, RutaNumero = 3 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 2 d",
                    Titulo = "Más frecuencia en Plan de Ayala",
                    Cuerpo = "Sugerimos una unidad extra en hora pico, de 7 a 9 am, para reducir el tiempo de espera.",
                    Etiquetas = new List<Etiqueta> { Z("Plan de Ayala") }, Likes = 31, Comentarios = 12, Orden = 50, RutaNumero = 9 }
            };
        }

        public static List<Publicacion> CrearPublicacionesExtra()
        {
            return new List<Publicacion>
            {
                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 3 d",
                    Titulo = "Mantenimiento en Ruta 12",
                    Cuerpo = "La Ruta 12 - Circunvalación II no circulará el sábado de 2 a 5 am por trabajos de mantenimiento.",
                    Etiquetas = new List<Etiqueta> { R(12), Z("Circunvalación II") }, Likes = 4, Comentarios = 1, Orden = 40, RutaNumero = 12 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 3 d",
                    Titulo = "¿Aceptan tarjeta?",
                    Cuerpo = "¿Las unidades de la Ruta 15 ya aceptan pago con tarjeta o solo efectivo?",
                    Etiquetas = new List<Etiqueta> { R(15), Z("Komatsu") }, Likes = 7, Comentarios = 3, Orden = 30, RutaNumero = 15 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 4 d",
                    Titulo = "Paradas con techo",
                    Cuerpo = "Proponemos instalar techo en las paradas más concurridas de Zona Centro para los días de lluvia y sol fuerte.",
                    Etiquetas = new List<Etiqueta> { Z("Paradas"), Z("Zona Centro") }, Likes = 22, Comentarios = 8, Orden = 20 }
            };
        }
    }
}
