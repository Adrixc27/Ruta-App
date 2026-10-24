using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace Ruta_App
{
    // Modelos del motor de rutas. No dependen de WPF: se pueden usar desde la API, pruebas o la interfaz.
    // Unidades: tiempos en minutos, distancias en metros, coordenadas en grados decimales (WGS84).

    /// <summary>Punto geográfico (origen/destino que escribe o elige el usuario).</summary>
    public class Coordenada
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        public Coordenada() { }

        public Coordenada(double latitud, double longitud)
        {
            Latitud = latitud;
            Longitud = longitud;
        }

        public bool EsValida
        {
            get
            {
                return !double.IsNaN(Latitud) && !double.IsNaN(Longitud)
                    && Latitud >= -90 && Latitud <= 90
                    && Longitud >= -180 && Longitud <= 180;
            }
        }

        public override string ToString()
        {
            return Latitud.ToString("0.00000", CultureInfo.InvariantCulture) + ", " + Longitud.ToString("0.00000", CultureInfo.InvariantCulture);
        }
    }

    public class Parada
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        /// <summary>True si la parada permite abordar/descender con accesibilidad (rampa, andén, etc.).</summary>
        public bool Accesible { get; set; }

        [JsonIgnore]
        public Coordenada Posicion { get { return new Coordenada(Latitud, Longitud); } }

        public override string ToString() { return Nombre + " (" + Id + ")"; }
    }

    public enum SentidoRecorrido
    {
        /// <summary>Solo se recorre en el orden de IdsParadas.</summary>
        Unidireccional,
        /// <summary>Se recorre en ambos sentidos (ida y vuelta por las mismas paradas).</summary>
        Bidireccional,
        /// <summary>Se recorre en el orden de IdsParadas y la última parada vuelve a la primera.</summary>
        Circular
    }

    public class RutaAutobus
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        /// <summary>Ids de las paradas en orden de recorrido (sin repetir; en una ruta circular no se repite la primera al final).</summary>
        public List<string> IdsParadas { get; set; } = new List<string>();
        public SentidoRecorrido Sentido { get; set; } = SentidoRecorrido.Unidireccional;
        /// <summary>Confiabilidad entre 0 y 1 (p. ej. proporción de salidas puntuales). null = sin datos suficientes.</summary>
        public double? Confiabilidad { get; set; }
    }

    /// <summary>
    /// Tramo dirigido entre dos paradas. Con IdRuta = autobús; con IdRuta = null es un tramo caminando (transbordo a pie).
    /// </summary>
    public class Conexion
    {
        public string IdOrigen { get; set; } = "";
        public string IdDestino { get; set; } = "";
        public string IdRuta { get; set; }
        public double TiempoMin { get; set; }
        public double DistanciaM { get; set; }

        [JsonIgnore]
        public bool EsCaminata { get { return IdRuta == null; } }

        public override string ToString()
        {
            return IdOrigen + " -> " + IdDestino + " [" + (IdRuta ?? "a pie") + "] " + TiempoMin.ToString("0.#", CultureInfo.InvariantCulture) + " min";
        }
    }

    public enum Prioridad { Equilibrado, MasRapida, MenosTransbordos, MenosCaminata, MasConfiable }

    /// <summary>Prioridades, pesos de evaluación y límites de la búsqueda. Todo es configurable.</summary>
    public class Preferencias
    {
        // Pesos relativos de cada criterio (no negativos; se normalizan para que sumen 1).
        public double PesoTiempo { get; set; } = 1;
        public double PesoTransbordos { get; set; } = 1;
        public double PesoCaminata { get; set; } = 1;
        public double PesoConfiabilidad { get; set; } = 1;

        // Límites de la búsqueda
        public int MaxTransbordos { get; set; } = 3;
        public double RadioBusquedaMetros { get; set; } = 800;
        public int MaxParadasCandidatas { get; set; } = 3;
        public int MaxAlternativasPorPar { get; set; } = 15;
        public int MaxExpansiones { get; set; } = 20000;
        /// <summary>Se descartan recorridos que tarden más que (tiempo del más rápido × este factor).</summary>
        public double FactorTiempoMaximo { get; set; } = 2;
        public bool SoloParadasAccesibles { get; set; }

        // Caminata
        public double VelocidadCaminataKmh { get; set; } = 4.5;
        /// <summary>Distancia caminando estimada = línea recta × este factor (las calles no son rectas). Es una estimación, no una ruta peatonal real.</summary>
        public double FactorRodeoCaminata { get; set; } = 1.3;
        public double RadioTransbordoMetros { get; set; } = 300;
        /// <summary>Penalización (en minutos) que usa la búsqueda para preferir menos transbordos. No se suma al tiempo reportado.</summary>
        public double PenalizacionTransbordoMin { get; set; } = 5;

        /// <summary>Crea preferencias con el criterio elegido como prioridad principal.</summary>
        public static Preferencias Para(Prioridad prioridad)
        {
            var p = new Preferencias();
            double principal = 6;
            double otro = 1;
            p.PesoTiempo = p.PesoTransbordos = p.PesoCaminata = p.PesoConfiabilidad = otro;
            switch (prioridad)
            {
                case Prioridad.MasRapida: p.PesoTiempo = principal; break;
                case Prioridad.MenosTransbordos: p.PesoTransbordos = principal; break;
                case Prioridad.MenosCaminata: p.PesoCaminata = principal; break;
                case Prioridad.MasConfiable: p.PesoConfiabilidad = principal; break;
            }
            return p;
        }

        public double SumaPesos { get { return PesoTiempo + PesoTransbordos + PesoCaminata + PesoConfiabilidad; } }

        public double VelocidadCaminataMetrosPorMin { get { return VelocidadCaminataKmh * 1000.0 / 60.0; } }

        /// <summary>Devuelve los problemas encontrados (lista vacía si son válidas).</summary>
        public List<string> Validar()
        {
            var e = new List<string>();
            Func<double, bool> malo = v => double.IsNaN(v) || double.IsInfinity(v) || v < 0;
            if (malo(PesoTiempo) || malo(PesoTransbordos) || malo(PesoCaminata) || malo(PesoConfiabilidad))
                e.Add("Los pesos deben ser números mayores o iguales a cero.");
            else if (SumaPesos <= 0)
                e.Add("Al menos un peso debe ser mayor que cero.");
            if (MaxTransbordos < 0) e.Add("MaxTransbordos no puede ser negativo.");
            if (double.IsNaN(RadioBusquedaMetros) || RadioBusquedaMetros <= 0) e.Add("RadioBusquedaMetros debe ser mayor que cero.");
            if (MaxParadasCandidatas < 1) e.Add("MaxParadasCandidatas debe ser al menos 1.");
            if (MaxAlternativasPorPar < 1) e.Add("MaxAlternativasPorPar debe ser al menos 1.");
            if (MaxExpansiones < 1) e.Add("MaxExpansiones debe ser al menos 1.");
            if (double.IsNaN(FactorTiempoMaximo) || FactorTiempoMaximo < 1) e.Add("FactorTiempoMaximo debe ser al menos 1.");
            if (double.IsNaN(VelocidadCaminataKmh) || VelocidadCaminataKmh <= 0) e.Add("VelocidadCaminataKmh debe ser mayor que cero.");
            if (double.IsNaN(FactorRodeoCaminata) || FactorRodeoCaminata < 1) e.Add("FactorRodeoCaminata debe ser al menos 1 (la distancia caminando no puede ser menor que la línea recta).");
            if (double.IsNaN(RadioTransbordoMetros) || RadioTransbordoMetros < 0) e.Add("RadioTransbordoMetros no puede ser negativo.");
            if (double.IsNaN(PenalizacionTransbordoMin) || PenalizacionTransbordoMin < 0) e.Add("PenalizacionTransbordoMin no puede ser negativa.");
            return e;
        }
    }

    public class TramoRecorrido
    {
        public bool EsCaminata { get; set; }
        public string IdRuta { get; set; }
        public string NombreRuta { get; set; }
        public List<Parada> Paradas { get; set; } = new List<Parada>();
        public double Minutos { get; set; }
        public double DistanciaM { get; set; }
    }

    /// <summary>
    /// Camino entre dos paradas (más, opcionalmente, la caminata desde el origen y hasta el destino reales).
    /// Las métricas se calculan en una sola pasada sobre las conexiones, así que ningún tramo se cuenta dos veces.
    /// </summary>
    public class Recorrido
    {
        public List<Parada> Paradas { get; private set; }
        public List<Conexion> Conexiones { get; private set; }
        /// <summary>Autobuses usados, en orden y sin repetir seguidos.</summary>
        public List<RutaAutobus> Autobuses { get; private set; }
        public List<TramoRecorrido> Tramos { get; private set; }

        public Coordenada Origen { get; private set; }
        public Coordenada Destino { get; private set; }
        public double AccesoM { get; private set; }
        public double AccesoMin { get; private set; }
        public double EgresoM { get; private set; }
        public double EgresoMin { get; private set; }

        public double TiempoAutobusMin { get; private set; }
        public double TiempoCaminataMin { get; private set; }
        public double TiempoTotalMin { get { return TiempoAutobusMin + TiempoCaminataMin; } }
        public double DistanciaAutobusM { get; private set; }
        public double DistanciaCaminataM { get; private set; }
        public double DistanciaTotalM { get { return DistanciaAutobusM + DistanciaCaminataM; } }
        public int Transbordos { get; private set; }
        /// <summary>Producto de las confiabilidades de los autobuses usados (0-1). null si falta el dato en algún autobús.</summary>
        public double? Confiabilidad { get; private set; }
        /// <summary>True si todas las paradas donde se aborda, desciende o se camina entre autobuses son accesibles.</summary>
        public bool TodasParadasAccesibles { get; private set; }

        public Parada ParadaInicial { get { return Paradas[0]; } }
        public Parada ParadaFinal { get { return Paradas[Paradas.Count - 1]; } }

        /// <summary>Puntos [origen, paradas..., destino] en orden, listos para dibujar una polilínea en Leaflet.</summary>
        public List<Coordenada> Trazo
        {
            get
            {
                var puntos = new List<Coordenada>();
                if (Origen != null) puntos.Add(Origen);
                puntos.AddRange(Paradas.Select(p => p.Posicion));
                if (Destino != null) puntos.Add(Destino);
                return puntos;
            }
        }

        /// <summary>Identifica el recorrido por sus tramos de autobús: (ruta, parada de subida, parada de bajada).</summary>
        public string Firma
        {
            get
            {
                return string.Join("|", Tramos.Where(t => !t.EsCaminata)
                    .Select(t => t.IdRuta + ":" + t.Paradas[0].Id + ">" + t.Paradas[t.Paradas.Count - 1].Id));
            }
        }

        /// <summary>Identifica solo la secuencia de líneas usadas (ignora en qué parada se sube o baja).</summary>
        public string FirmaDeRutas
        {
            get { return string.Join(">", Autobuses.Select(a => a.Id)); }
        }

        /// <summary>
        /// Arma el recorrido a partir de conexiones consecutivas. Lanza ArgumentException si hay huecos,
        /// tramos repetidos, paradas repetidas (ciclos) o referencias a datos que no existen.
        /// </summary>
        public static Recorrido Crear(IList<Conexion> conexiones, IDictionary<string, Parada> paradas, IDictionary<string, RutaAutobus> rutas)
        {
            if (conexiones == null || conexiones.Count == 0) throw new ArgumentException("Un recorrido necesita al menos una conexión.");

            var r = new Recorrido
            {
                Conexiones = new List<Conexion>(conexiones),
                Paradas = new List<Parada>(),
                Autobuses = new List<RutaAutobus>(),
                Tramos = new List<TramoRecorrido>()
            };

            var vistas = new HashSet<string>();
            var tramosUsados = new HashSet<Conexion>();
            Func<string, Parada> obtener = id =>
            {
                Parada p;
                if (id == null || !paradas.TryGetValue(id, out p)) throw new ArgumentException("La parada '" + id + "' no existe.");
                return p;
            };

            var primera = obtener(conexiones[0].IdOrigen);
            r.Paradas.Add(primera);
            vistas.Add(primera.Id);

            double producto = 1;
            bool confiabilidadConocida = true;
            bool accesibles = true;
            TramoRecorrido tramo = null;
            Conexion previa = null;

            foreach (var c in conexiones)
            {
                if (!tramosUsados.Add(c)) throw new ArgumentException("El tramo " + c + " aparece más de una vez.");
                var actual = r.Paradas[r.Paradas.Count - 1];
                if (c.IdOrigen != actual.Id) throw new ArgumentException("Las conexiones no son consecutivas en la parada '" + actual.Id + "'.");
                var destino = obtener(c.IdDestino);
                if (!vistas.Add(destino.Id)) throw new ArgumentException("El recorrido pasa dos veces por la parada '" + destino.Id + "' (ciclo).");

                RutaAutobus ruta = null;
                if (!c.EsCaminata && !rutas.TryGetValue(c.IdRuta, out ruta))
                    throw new ArgumentException("La ruta '" + c.IdRuta + "' no existe.");

                // ¿Empieza un tramo nuevo? Sí al inicio, al cambiar de ruta o al pasar de/hacia caminata.
                bool nuevoTramo = previa == null || previa.EsCaminata != c.EsCaminata || previa.IdRuta != c.IdRuta;
                if (nuevoTramo)
                {
                    tramo = new TramoRecorrido
                    {
                        EsCaminata = c.EsCaminata,
                        IdRuta = c.IdRuta,
                        NombreRuta = ruta == null ? null : ruta.Nombre
                    };
                    tramo.Paradas.Add(actual);
                    r.Tramos.Add(tramo);
                    if (!c.EsCaminata)
                    {
                        r.Autobuses.Add(ruta);
                        if (ruta.Confiabilidad.HasValue) producto *= ruta.Confiabilidad.Value; else confiabilidadConocida = false;
                    }
                }

                tramo.Paradas.Add(destino);
                tramo.Minutos += c.TiempoMin;
                tramo.DistanciaM += c.DistanciaM;
                if (c.EsCaminata) { r.TiempoCaminataMin += c.TiempoMin; r.DistanciaCaminataM += c.DistanciaM; }
                else { r.TiempoAutobusMin += c.TiempoMin; r.DistanciaAutobusM += c.DistanciaM; }

                r.Paradas.Add(destino);
                previa = c;
            }

            // Transbordos = tramos de autobús - 1 (autobús -> caminata -> autobús cuenta como uno solo).
            r.Transbordos = Math.Max(0, r.Tramos.Count(t => !t.EsCaminata) - 1);

            foreach (var t in r.Tramos)
            {
                if (t.EsCaminata) continue;
                if (!t.Paradas[0].Accesible || !t.Paradas[t.Paradas.Count - 1].Accesible) accesibles = false;
            }

            r.TodasParadasAccesibles = accesibles;
            r.Confiabilidad = (confiabilidadConocida && r.Autobuses.Count > 0) ? (double?)producto : null;
            return r;
        }

        /// <summary>Agrega la caminata entre el origen real y la primera parada, y entre la última parada y el destino real.</summary>
        public void AsignarAccesos(Coordenada origen, double accesoM, double accesoMin, Coordenada destino, double egresoM, double egresoMin)
        {
            // Se restan los accesos anteriores para no duplicar caminata si se asigna dos veces.
            TiempoCaminataMin += (accesoMin + egresoMin) - (AccesoMin + EgresoMin);
            DistanciaCaminataM += (accesoM + egresoM) - (AccesoM + EgresoM);
            Origen = origen;
            Destino = destino;
            AccesoM = accesoM; AccesoMin = accesoMin;
            EgresoM = egresoM; EgresoMin = egresoMin;
        }
    }

    public class RutaRecomendada
    {
        public int Posicion { get; set; }
        public Recorrido Recorrido { get; set; }
        /// <summary>0-100, mayor es mejor. Es relativa a las alternativas de esta búsqueda (criterios normalizados entre ellas).</summary>
        public double Puntuacion { get; set; }
        /// <summary>Costo ponderado 0-1 (menor es mejor) del que sale la puntuación.</summary>
        public double Costo { get; set; }
        public List<string> Ventajas { get; set; } = new List<string>();
        public List<string> Desventajas { get; set; } = new List<string>();
        public string Explicacion { get; set; } = "";
    }

    public enum TipoError
    {
        Ninguno,
        UbicacionInvalida,
        PreferenciasInvalidas,
        CantidadInvalida,
        SinParadasCercanasAlOrigen,
        SinParadasCercanasAlDestino,
        OrigenIgualDestino,
        MismaParada,
        DestinoInaccesible,
        SinAlternativas
    }

    /// <summary>Respuesta completa de una búsqueda: lista para mostrarse en WPF o serializarse hacia Leaflet.</summary>
    public class ResultadoBusqueda
    {
        public bool Exito { get { return Error == TipoError.Ninguno && Recomendaciones.Count > 0; } }
        public TipoError Error { get; set; } = TipoError.Ninguno;
        public string Mensaje { get; set; } = "";
        public List<string> Advertencias { get; set; } = new List<string>();
        public List<RutaRecomendada> Recomendaciones { get; set; } = new List<RutaRecomendada>();

        public static ResultadoBusqueda Fallo(TipoError error, string mensaje)
        {
            return new ResultadoBusqueda { Error = error, Mensaje = mensaje };
        }
    }
}
