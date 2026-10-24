using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruta_App
{
    /// <summary>
    /// Parada cercana a una ubicación. Distingue la distancia en línea recta (la que se usa para filtrar por radio)
    /// de la distancia estimada caminando (línea recta × factor de rodeo), que es la que cuenta como caminata del viaje.
    /// </summary>
    public class ParadaCercana
    {
        public Parada Parada { get; set; }
        public double DistanciaLineaRectaM { get; set; }
        /// <summary>Estimación: no sigue calles reales hasta que se integre un servicio de rutas peatonales.</summary>
        public double DistanciaCaminandoM { get; set; }
        public double TiempoCaminandoMin { get; set; }
    }

    public class ServicioUbicaciones
    {
        private const double RadioTierraM = 6371008.8;
        private readonly List<Parada> _paradas;

        public ServicioUbicaciones(IEnumerable<Parada> paradas)
        {
            if (paradas == null) throw new ArgumentNullException("paradas");
            _paradas = paradas.Where(p => p != null).ToList();
        }

        /// <summary>Distancia en línea recta sobre la esfera terrestre (fórmula de Haversine), en metros.</summary>
        public static double DistanciaLineaRectaM(Coordenada a, Coordenada b)
        {
            if (a == null || b == null) throw new ArgumentNullException(a == null ? "a" : "b");
            return DistanciaLineaRectaM(a.Latitud, a.Longitud, b.Latitud, b.Longitud);
        }

        public static double DistanciaLineaRectaM(double lat1, double lon1, double lat2, double lon2)
        {
            double f1 = lat1 * Math.PI / 180, f2 = lat2 * Math.PI / 180;
            double df = (lat2 - lat1) * Math.PI / 180, dl = (lon2 - lon1) * Math.PI / 180;
            double h = Math.Sin(df / 2) * Math.Sin(df / 2) + Math.Cos(f1) * Math.Cos(f2) * Math.Sin(dl / 2) * Math.Sin(dl / 2);
            return 2 * RadioTierraM * Math.Asin(Math.Min(1, Math.Sqrt(h)));
        }

        /// <summary>Estima la distancia caminando a partir de la línea recta.</summary>
        public static double EstimarDistanciaCaminandoM(double lineaRectaM, double factorRodeo)
        {
            return lineaRectaM * factorRodeo;
        }

        public static double TiempoCaminandoMin(double distanciaM, double velocidadKmh)
        {
            return distanciaM / (velocidadKmh * 1000.0 / 60.0);
        }

        /// <summary>
        /// Paradas dentro del radio (en línea recta) de una ubicación, de la más cercana a la más lejana.
        /// Devuelve lista vacía si la ubicación es inválida o no hay paradas en el radio.
        /// </summary>
        public List<ParadaCercana> BuscarParadasCercanas(Coordenada ubicacion, Preferencias preferencias, int maximo = int.MaxValue)
        {
            var resultado = new List<ParadaCercana>();
            if (ubicacion == null || !ubicacion.EsValida || preferencias == null) return resultado;

            foreach (var p in _paradas)
            {
                if (preferencias.SoloParadasAccesibles && !p.Accesible) continue;
                double recta = DistanciaLineaRectaM(ubicacion.Latitud, ubicacion.Longitud, p.Latitud, p.Longitud);
                if (recta > preferencias.RadioBusquedaMetros) continue;
                resultado.Add(Crear(p, recta, preferencias));
            }
            return resultado.OrderBy(c => c.DistanciaLineaRectaM).ThenBy(c => c.Parada.Id, StringComparer.Ordinal).Take(Math.Max(0, maximo)).ToList();
        }

        /// <summary>Pares de paradas a menos de radioM (en línea recta) para crear los transbordos caminando.</summary>
        public IEnumerable<KeyValuePair<Parada, ParadaCercana>> ParesCercanos(double radioM, Preferencias preferencias)
        {
            for (int i = 0; i < _paradas.Count; i++)
                for (int j = 0; j < _paradas.Count; j++)
                {
                    if (i == j) continue;
                    double recta = DistanciaLineaRectaM(_paradas[i].Latitud, _paradas[i].Longitud, _paradas[j].Latitud, _paradas[j].Longitud);
                    if (recta <= radioM) yield return new KeyValuePair<Parada, ParadaCercana>(_paradas[i], Crear(_paradas[j], recta, preferencias));
                }
        }

        public ParadaCercana Crear(Parada parada, double lineaRectaM, Preferencias preferencias)
        {
            double caminando = EstimarDistanciaCaminandoM(lineaRectaM, preferencias.FactorRodeoCaminata);
            return new ParadaCercana
            {
                Parada = parada,
                DistanciaLineaRectaM = lineaRectaM,
                DistanciaCaminandoM = caminando,
                TiempoCaminandoMin = TiempoCaminandoMin(caminando, preferencias.VelocidadCaminataKmh)
            };
        }
    }
}
