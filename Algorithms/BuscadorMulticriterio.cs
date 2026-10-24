using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruta_App
{
    /// <summary>
    /// Reúne alternativas con ventajas distintas (la más rápida, la de menos transbordos, la de menos caminata,
    /// la más confiable y la que combina tus pesos) y descarta las dominadas en el sentido de Pareto.
    /// </summary>
    public class BuscadorMulticriterio
    {
        private const double Tolerancia = 1e-6;
        private readonly GrafoTransporte _grafo;
        private readonly AlgoritmoDijkstra _dijkstra;
        private readonly BuscadorRutas _buscador;

        public BuscadorMulticriterio(GrafoTransporte grafo)
        {
            if (grafo == null) throw new ArgumentNullException("grafo");
            _grafo = grafo;
            _dijkstra = new AlgoritmoDijkstra(grafo);
            _buscador = new BuscadorRutas(grafo);
        }

        /// <summary>Alternativas no dominadas entre dos paradas (sin duplicados). Lista vacía si no hay camino.</summary>
        public List<Recorrido> Buscar(string idOrigen, string idDestino, Preferencias preferencias)
        {
            if (preferencias == null) throw new ArgumentNullException("preferencias");
            var candidatos = new List<Recorrido>(_buscador.Buscar(idOrigen, idDestino, preferencias));
            if (candidatos.Count == 0) return candidatos;   // inaccesible o fuera de los límites

            double limiteTiempo = candidatos.Min(c => c.TiempoTotalMin) * preferencias.FactorTiempoMaximo + 1e-9;
            foreach (var variante in Variantes(preferencias))
            {
                var r = _dijkstra.CaminoMasCorto(idOrigen, idDestino, variante.Costo, variante.PenalizacionTransbordo, preferencias.SoloParadasAccesibles);
                if (!r.Alcanzable) continue;
                if (r.Recorrido.Transbordos > preferencias.MaxTransbordos || r.Recorrido.TiempoTotalMin > limiteTiempo) continue;
                candidatos.Add(r.Recorrido);
            }

            return FiltrarNoDominados(ClasificadorRutas.EliminarDuplicados(candidatos));
        }

        /// <summary>
        /// ¿"a" domina a "b"? Sí si en tiempo, transbordos y caminata es igual o mejor y estrictamente mejor en al menos uno.
        /// La confiabilidad solo se compara cuando se conoce en ambos (un dato desconocido no se trata como bueno ni como malo).
        /// </summary>
        public static bool Domina(Recorrido a, Recorrido b)
        {
            bool mejorEnAlguno = false;
            var diferencias = new List<double>
            {
                a.TiempoTotalMin - b.TiempoTotalMin,
                a.Transbordos - b.Transbordos,
                a.DistanciaCaminataM - b.DistanciaCaminataM
            };
            if (a.Confiabilidad.HasValue && b.Confiabilidad.HasValue) diferencias.Add(b.Confiabilidad.Value - a.Confiabilidad.Value);

            foreach (var d in diferencias)
            {
                if (d > Tolerancia) return false;       // "a" es peor en este criterio
                if (d < -Tolerancia) mejorEnAlguno = true;
            }
            return mejorEnAlguno;
        }

        /// <summary>Frente de Pareto: conserva solo los recorridos que ningún otro domina.</summary>
        public static List<Recorrido> FiltrarNoDominados(IEnumerable<Recorrido> recorridos)
        {
            if (recorridos == null) throw new ArgumentNullException("recorridos");
            var lista = recorridos.Where(r => r != null).ToList();
            return lista.Where(r => !lista.Any(otro => !ReferenceEquals(otro, r) && Domina(otro, r))).ToList();
        }

        private class Variante
        {
            public Func<Conexion, double> Costo;
            public double PenalizacionTransbordo;
        }

        // Cada variante optimiza un criterio distinto con Dijkstra.
        private IEnumerable<Variante> Variantes(Preferencias p)
        {
            double suma = p.SumaPesos;
            double parteCaminata = suma > 0 ? p.PesoCaminata / suma : 0;
            double parteTransbordos = suma > 0 ? p.PesoTransbordos / suma : 0;

            // Más rápida
            yield return new Variante { Costo = c => c.TiempoMin, PenalizacionTransbordo = 0 };
            // Menos transbordos (y, entre ellos, la más rápida)
            yield return new Variante { Costo = c => c.TiempoMin, PenalizacionTransbordo = 1000 };
            // Menos caminata entre paradas
            yield return new Variante { Costo = c => c.EsCaminata ? c.TiempoMin * 20 : c.TiempoMin, PenalizacionTransbordo = p.PenalizacionTransbordoMin };
            // Equilibrada según los pesos del usuario: más peso en caminata/transbordos => se castigan más
            double factorCaminata = 1 + 4 * parteCaminata;
            yield return new Variante
            {
                Costo = c => c.EsCaminata ? c.TiempoMin * factorCaminata : c.TiempoMin,
                PenalizacionTransbordo = p.PenalizacionTransbordoMin * (1 + 4 * parteTransbordos)
            };

            // Más confiable: solo si hay datos; las rutas sin dato se excluyen de esta variante (no se les inventa un valor).
            if (_grafo.Rutas.Any(r => r.Confiabilidad.HasValue))
            {
                yield return new Variante
                {
                    Costo = c =>
                    {
                        if (c.EsCaminata) return c.TiempoMin * 0.001;
                        var ruta = _grafo.ObtenerRuta(c.IdRuta);
                        if (ruta == null || !ruta.Confiabilidad.HasValue || ruta.Confiabilidad.Value <= 0) return double.PositiveInfinity;
                        return -Math.Log(Math.Min(1.0, ruta.Confiabilidad.Value)) * 1000 + c.TiempoMin * 0.001;
                    },
                    PenalizacionTransbordo = 0
                };
            }
        }
    }
}
