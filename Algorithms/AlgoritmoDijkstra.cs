using System;
using System.Collections.Generic;

namespace Ruta_App
{
    /// <summary>Cola de prioridad mínima (montículo binario). Empata por orden de inserción para que los resultados sean deterministas.</summary>
    internal class MonticuloMinimo<T>
    {
        private struct Elemento
        {
            public double Prioridad;
            public long Orden;
            public T Valor;
        }

        private readonly List<Elemento> _datos = new List<Elemento>();
        private long _contador;

        public int Cantidad { get { return _datos.Count; } }

        public void Insertar(double prioridad, T valor)
        {
            _datos.Add(new Elemento { Prioridad = prioridad, Orden = _contador++, Valor = valor });
            int i = _datos.Count - 1;
            while (i > 0)
            {
                int padre = (i - 1) / 2;
                if (!Menor(_datos[i], _datos[padre])) break;
                Intercambiar(i, padre);
                i = padre;
            }
        }

        public T Extraer(out double prioridad)
        {
            if (_datos.Count == 0) throw new InvalidOperationException("El montículo está vacío.");
            var raiz = _datos[0];
            var ultimo = _datos[_datos.Count - 1];
            _datos.RemoveAt(_datos.Count - 1);
            if (_datos.Count > 0)
            {
                _datos[0] = ultimo;
                int i = 0;
                while (true)
                {
                    int izq = 2 * i + 1, der = izq + 1, menor = i;
                    if (izq < _datos.Count && Menor(_datos[izq], _datos[menor])) menor = izq;
                    if (der < _datos.Count && Menor(_datos[der], _datos[menor])) menor = der;
                    if (menor == i) break;
                    Intercambiar(i, menor);
                    i = menor;
                }
            }
            prioridad = raiz.Prioridad;
            return raiz.Valor;
        }

        private static bool Menor(Elemento a, Elemento b)
        {
            return a.Prioridad < b.Prioridad || (a.Prioridad == b.Prioridad && a.Orden < b.Orden);
        }

        private void Intercambiar(int i, int j)
        {
            var t = _datos[i]; _datos[i] = _datos[j]; _datos[j] = t;
        }
    }

    public class ResultadoDijkstra
    {
        public bool Alcanzable { get; set; }
        /// <summary>Costo total de búsqueda (incluye la penalización por transbordo si se pidió).</summary>
        public double Costo { get; set; }
        public List<Conexion> Conexiones { get; set; } = new List<Conexion>();
        public Recorrido Recorrido { get; set; }
        /// <summary>Si no es alcanzable, explica por qué.</summary>
        public string Motivo { get; set; } = "";
    }

    /// <summary>
    /// Dijkstra sobre el GrafoTransporte con costos no negativos.
    /// Como cambiar de autobús cuesta (penalización) y depende de cómo se llegó a la parada, el estado de la
    /// búsqueda es (parada, autobús con el que se llegó), no solo la parada.
    /// </summary>
    public class AlgoritmoDijkstra
    {
        private readonly GrafoTransporte _grafo;

        public AlgoritmoDijkstra(GrafoTransporte grafo)
        {
            if (grafo == null) throw new ArgumentNullException("grafo");
            _grafo = grafo;
        }

        private class Nodo
        {
            public string IdParada;
            public Conexion Llegada;      // conexión usada para llegar (null en el origen)
            public Nodo Padre;
            public double Costo;
        }

        /// <summary>
        /// Camino de menor costo entre dos paradas.
        /// costo(conexion) debe devolver un valor >= 0; double.PositiveInfinity descarta esa conexión; un valor
        /// negativo o NaN lanza InvalidOperationException (Dijkstra no funciona con pesos negativos).
        /// </summary>
        public ResultadoDijkstra CaminoMasCorto(string idOrigen, string idDestino, Func<Conexion, double> costo,
                                                double penalizacionTransbordo = 0, bool soloAccesibles = false)
        {
            if (costo == null) throw new ArgumentNullException("costo");
            if (double.IsNaN(penalizacionTransbordo) || penalizacionTransbordo < 0)
                throw new ArgumentException("La penalización por transbordo no puede ser negativa.");

            if (!_grafo.ExisteParada(idOrigen)) return NoAlcanzable("La parada de origen '" + idOrigen + "' no existe.");
            if (!_grafo.ExisteParada(idDestino)) return NoAlcanzable("La parada de destino '" + idDestino + "' no existe.");
            if (idOrigen == idDestino) return NoAlcanzable("El origen y el destino son la misma parada.");

            var mejores = new Dictionary<string, double>();
            var cola = new MonticuloMinimo<Nodo>();
            var inicio = new Nodo { IdParada = idOrigen, Costo = 0 };
            mejores[Clave(inicio)] = 0;
            cola.Insertar(0, inicio);

            while (cola.Cantidad > 0)
            {
                double prioridad;
                var actual = cola.Extraer(out prioridad);
                double conocido;
                if (mejores.TryGetValue(Clave(actual), out conocido) && actual.Costo > conocido) continue;   // entrada obsoleta

                if (actual.IdParada == idDestino && _grafo.PuedeTerminar(actual.Llegada, soloAccesibles))
                    return Reconstruir(actual);

                foreach (var c in _grafo.Salientes(actual.IdParada))
                {
                    if (!_grafo.PuedeContinuar(actual.Llegada, c, soloAccesibles)) continue;

                    double peso = costo(c);
                    if (double.IsNaN(peso) || peso < 0)
                        throw new InvalidOperationException("Dijkstra requiere costos no negativos; la conexión " + c + " devolvió " + peso + ".");
                    if (double.IsPositiveInfinity(peso)) continue;

                    double nuevo = actual.Costo + peso + (GrafoTransporte.EsTransbordo(actual.Llegada, c) ? penalizacionTransbordo : 0);
                    var siguiente = new Nodo { IdParada = c.IdDestino, Llegada = c, Padre = actual, Costo = nuevo };
                    string clave = Clave(siguiente);
                    double previo;
                    if (mejores.TryGetValue(clave, out previo) && previo <= nuevo) continue;
                    mejores[clave] = nuevo;
                    cola.Insertar(nuevo, siguiente);
                }
            }

            return NoAlcanzable("No existe un camino de '" + idOrigen + "' a '" + idDestino + "' con las conexiones y restricciones actuales.");
        }

        /// <summary>
        /// Costo mínimo desde cada parada hasta el destino, recorriendo las conexiones al revés y sin reglas de transbordo.
        /// Es una cota inferior válida (nunca sobreestima), útil para podar búsquedas. Las paradas que no llegan al destino no aparecen.
        /// </summary>
        public Dictionary<string, double> DistanciasHaciaDestino(string idDestino, Func<Conexion, double> costo)
        {
            if (costo == null) throw new ArgumentNullException("costo");
            var distancias = new Dictionary<string, double>();
            if (!_grafo.ExisteParada(idDestino)) return distancias;

            var cola = new MonticuloMinimo<string>();
            distancias[idDestino] = 0;
            cola.Insertar(0, idDestino);
            while (cola.Cantidad > 0)
            {
                double d;
                string id = cola.Extraer(out d);
                if (d > distancias[id]) continue;
                foreach (var c in _grafo.Entrantes(id))
                {
                    double peso = costo(c);
                    if (double.IsNaN(peso) || peso < 0)
                        throw new InvalidOperationException("Dijkstra requiere costos no negativos; la conexión " + c + " devolvió " + peso + ".");
                    if (double.IsPositiveInfinity(peso)) continue;
                    double nuevo = d + peso;
                    double previo;
                    if (distancias.TryGetValue(c.IdOrigen, out previo) && previo <= nuevo) continue;
                    distancias[c.IdOrigen] = nuevo;
                    cola.Insertar(nuevo, c.IdOrigen);
                }
            }
            return distancias;
        }

        private ResultadoDijkstra Reconstruir(Nodo final)
        {
            var conexiones = new List<Conexion>();
            for (var n = final; n.Padre != null; n = n.Padre) conexiones.Add(n.Llegada);
            conexiones.Reverse();
            try
            {
                return new ResultadoDijkstra
                {
                    Alcanzable = true,
                    Costo = final.Costo,
                    Conexiones = conexiones,
                    Recorrido = _grafo.CrearRecorrido(conexiones)
                };
            }
            catch (ArgumentException ex)
            {
                // Solo ocurre con datos defectuosos (p. ej. ciclos de costo cero).
                return NoAlcanzable("El camino encontrado no es válido: " + ex.Message);
            }
        }

        private static string Clave(Nodo n)
        {
            string via = n.Llegada == null ? "" : (n.Llegada.EsCaminata ? "#" : n.Llegada.IdRuta);
            return n.IdParada + "|" + via;
        }

        private static ResultadoDijkstra NoAlcanzable(string motivo)
        {
            return new ResultadoDijkstra { Alcanzable = false, Costo = double.PositiveInfinity, Motivo = motivo };
        }
    }
}
