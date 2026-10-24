using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruta_App
{
    /// <summary>
    /// Grafo dirigido de paradas. Cada arista es una Conexion (autobús o caminata entre paradas cercanas).
    /// Es independiente de WPF y de la fuente de datos: se llena con AgregarParada/AgregarRuta/AgregarConexion.
    /// </summary>
    public class GrafoTransporte
    {
        private readonly Dictionary<string, Parada> _paradas = new Dictionary<string, Parada>();
        private readonly Dictionary<string, RutaAutobus> _rutas = new Dictionary<string, RutaAutobus>();
        private readonly Dictionary<string, List<Conexion>> _salientes = new Dictionary<string, List<Conexion>>();
        private readonly Dictionary<string, List<Conexion>> _entrantes = new Dictionary<string, List<Conexion>>();
        private readonly HashSet<string> _clavesConexion = new HashSet<string>();
        private static readonly IReadOnlyList<Conexion> SinConexiones = new Conexion[0];

        public IEnumerable<Parada> Paradas { get { return _paradas.Values; } }
        public IEnumerable<RutaAutobus> Rutas { get { return _rutas.Values; } }
        public IEnumerable<Conexion> Conexiones { get { return _salientes.Values.SelectMany(l => l); } }
        public int NumeroParadas { get { return _paradas.Count; } }

        public void AgregarParada(Parada parada)
        {
            if (parada == null) throw new ArgumentNullException("parada");
            if (string.IsNullOrWhiteSpace(parada.Id)) throw new ArgumentException("La parada no tiene identificador.");
            if (_paradas.ContainsKey(parada.Id)) throw new ArgumentException("La parada '" + parada.Id + "' ya existe.");
            _paradas.Add(parada.Id, parada);
        }

        public void AgregarRuta(RutaAutobus ruta)
        {
            if (ruta == null) throw new ArgumentNullException("ruta");
            if (string.IsNullOrWhiteSpace(ruta.Id)) throw new ArgumentException("La ruta no tiene identificador.");
            if (_rutas.ContainsKey(ruta.Id)) throw new ArgumentException("La ruta '" + ruta.Id + "' ya existe.");
            _rutas.Add(ruta.Id, ruta);
        }

        /// <summary>Agrega una conexión. Rechaza paradas/rutas inexistentes, valores negativos o no numéricos y duplicados.</summary>
        public void AgregarConexion(Conexion c)
        {
            if (c == null) throw new ArgumentNullException("c");
            if (c.IdOrigen == null || !_paradas.ContainsKey(c.IdOrigen)) throw new ArgumentException("La parada de origen '" + c.IdOrigen + "' no existe.");
            if (c.IdDestino == null || !_paradas.ContainsKey(c.IdDestino)) throw new ArgumentException("La parada de destino '" + c.IdDestino + "' no existe.");
            if (c.IdOrigen == c.IdDestino) throw new ArgumentException("Una conexión no puede ir de una parada a sí misma ('" + c.IdOrigen + "').");
            if (c.IdRuta != null && !_rutas.ContainsKey(c.IdRuta)) throw new ArgumentException("La ruta '" + c.IdRuta + "' no existe.");
            if (!EsNoNegativo(c.TiempoMin) || !EsNoNegativo(c.DistanciaM)) throw new ArgumentException("La conexión " + c + " tiene tiempo o distancia negativos o no numéricos.");

            string clave = c.IdOrigen + "\u0001" + c.IdDestino + "\u0001" + (c.IdRuta ?? "\u0002");
            if (!_clavesConexion.Add(clave)) throw new ArgumentException("La conexión " + c + " ya existe.");

            Lista(_salientes, c.IdOrigen).Add(c);
            Lista(_entrantes, c.IdDestino).Add(c);
        }

        public bool ExisteParada(string id) { return id != null && _paradas.ContainsKey(id); }

        public Parada ObtenerParada(string id)
        {
            Parada p;
            return id != null && _paradas.TryGetValue(id, out p) ? p : null;
        }

        public RutaAutobus ObtenerRuta(string id)
        {
            RutaAutobus r;
            return id != null && _rutas.TryGetValue(id, out r) ? r : null;
        }

        /// <summary>Conexiones que salen de la parada (lista vacía si no existe o no tiene salidas).</summary>
        public IReadOnlyList<Conexion> Salientes(string idParada)
        {
            List<Conexion> l;
            return idParada != null && _salientes.TryGetValue(idParada, out l) ? l : SinConexiones;
        }

        /// <summary>Conexiones que llegan a la parada.</summary>
        public IReadOnlyList<Conexion> Entrantes(string idParada)
        {
            List<Conexion> l;
            return idParada != null && _entrantes.TryGetValue(idParada, out l) ? l : SinConexiones;
        }

        /// <summary>Construye un Recorrido (con métricas) a partir de conexiones consecutivas.</summary>
        public Recorrido CrearRecorrido(IList<Conexion> conexiones)
        {
            return Recorrido.Crear(conexiones, _paradas, _rutas);
        }

        // ── Reglas de transición: las usan Dijkstra y el buscador de alternativas para que ambos busquen igual ──

        /// <summary>
        /// ¿Se puede tomar "siguiente" justo después de "previa" (null = inicio del viaje)?
        /// Reglas: no se empieza ni se encadena una caminata entre paradas, y con soloAccesibles
        /// toda parada donde se sube, baja o cambia debe ser accesible.
        /// </summary>
        public bool PuedeContinuar(Conexion previa, Conexion siguiente, bool soloAccesibles)
        {
            if (previa == null)
            {
                if (siguiente.EsCaminata) return false;
                return !soloAccesibles || EsAccesible(siguiente.IdOrigen);
            }
            if (siguiente.EsCaminata)
            {
                if (previa.EsCaminata) return false;
                return !soloAccesibles || EsAccesible(siguiente.IdOrigen);   // se baja del autobús aquí
            }
            if (previa.EsCaminata) return !soloAccesibles || EsAccesible(siguiente.IdOrigen);   // se sube aquí
            if (previa.IdRuta == siguiente.IdRuta) return true;              // sigue en el mismo autobús
            return !soloAccesibles || EsAccesible(siguiente.IdOrigen);       // cambia de autobús en esta parada
        }

        /// <summary>El recorrido solo puede terminar bajando de un autobús (y en una parada accesible si se pidió).</summary>
        public bool PuedeTerminar(Conexion ultima, bool soloAccesibles)
        {
            return ultima != null && !ultima.EsCaminata && (!soloAccesibles || EsAccesible(ultima.IdDestino));
        }

        /// <summary>¿"siguiente" implica subir a un autobús nuevo (cuenta como transbordo si no es el primero)?</summary>
        public static bool EsTransbordo(Conexion previa, Conexion siguiente)
        {
            return previa != null && !siguiente.EsCaminata && (previa.EsCaminata || previa.IdRuta != siguiente.IdRuta);
        }

        private bool EsAccesible(string idParada)
        {
            Parada p;
            return _paradas.TryGetValue(idParada, out p) && p.Accesible;
        }

        private static bool EsNoNegativo(double v)
        {
            return !double.IsNaN(v) && !double.IsInfinity(v) && v >= 0;
        }

        private static List<Conexion> Lista(Dictionary<string, List<Conexion>> d, string clave)
        {
            List<Conexion> l;
            if (!d.TryGetValue(clave, out l)) { l = new List<Conexion>(); d.Add(clave, l); }
            return l;
        }
    }
}
