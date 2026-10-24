using System;
using System.Collections.Generic;

namespace Ruta_App
{
    /// <summary>
    /// Busca varios recorridos (directos y con transbordos) entre dos paradas, del más rápido al más lento.
    /// Es una búsqueda A* que no descarta caminos al llegar a una parada: cada camino parcial se extiende
    /// por orden de (tiempo recorrido + cota inferior del tiempo restante). Así los recorridos completos salen
    /// ya ordenados por tiempo y la búsqueda se detiene en cuanto se llega a los límites de Preferencias.
    /// Los caminos nunca repiten una parada (sin ciclos).
    /// </summary>
    public class BuscadorRutas
    {
        private readonly GrafoTransporte _grafo;
        private readonly AlgoritmoDijkstra _dijkstra;

        public BuscadorRutas(GrafoTransporte grafo)
        {
            if (grafo == null) throw new ArgumentNullException("grafo");
            _grafo = grafo;
            _dijkstra = new AlgoritmoDijkstra(grafo);
        }

        private class Parcial
        {
            public string IdParada;
            public Conexion Llegada;
            public Parcial Padre;
            public double Tiempo;
            public int Transbordos;
        }

        /// <summary>
        /// Límites usados (de Preferencias): MaxTransbordos, MaxAlternativasPorPar, FactorTiempoMaximo,
        /// MaxExpansiones y SoloParadasAccesibles. Devuelve lista vacía si no hay camino.
        /// </summary>
        public List<Recorrido> Buscar(string idOrigen, string idDestino, Preferencias preferencias)
        {
            var resultado = new List<Recorrido>();
            if (preferencias == null) throw new ArgumentNullException("preferencias");
            if (!_grafo.ExisteParada(idOrigen) || !_grafo.ExisteParada(idDestino) || idOrigen == idDestino) return resultado;

            Func<Conexion, double> tiempo = c => c.TiempoMin;
            bool soloAcc = preferencias.SoloParadasAccesibles;

            // El camino más rápido real fija el límite de tiempo y confirma que el destino es alcanzable.
            var mejor = _dijkstra.CaminoMasCorto(idOrigen, idDestino, tiempo, 0, soloAcc);
            if (!mejor.Alcanzable) return resultado;
            double limite = mejor.Costo * preferencias.FactorTiempoMaximo + 1e-9;

            var cotas = _dijkstra.DistanciasHaciaDestino(idDestino, tiempo);
            var cola = new MonticuloMinimo<Parcial>();
            cola.Insertar(cotas[idOrigen], new Parcial { IdParada = idOrigen });

            int expansiones = 0;
            while (cola.Cantidad > 0 && resultado.Count < preferencias.MaxAlternativasPorPar && expansiones < preferencias.MaxExpansiones)
            {
                double f;
                var actual = cola.Extraer(out f);
                expansiones++;

                if (actual.IdParada == idDestino)
                {
                    if (!_grafo.PuedeTerminar(actual.Llegada, soloAcc)) continue;
                    var recorrido = Armar(actual);
                    if (recorrido != null) resultado.Add(recorrido);
                    continue;   // no se sigue desde el destino: volver a pasar por él sería un ciclo
                }

                foreach (var c in _grafo.Salientes(actual.IdParada))
                {
                    if (!_grafo.PuedeContinuar(actual.Llegada, c, soloAcc)) continue;
                    if (YaPasoPor(actual, c.IdDestino)) continue;

                    int transbordos = actual.Transbordos + (GrafoTransporte.EsTransbordo(actual.Llegada, c) ? 1 : 0);
                    if (transbordos > preferencias.MaxTransbordos) continue;

                    double cota;
                    if (!cotas.TryGetValue(c.IdDestino, out cota)) continue;   // desde ahí ya no se llega al destino
                    double t = actual.Tiempo + c.TiempoMin;
                    if (t + cota > limite) continue;

                    cola.Insertar(t + cota, new Parcial { IdParada = c.IdDestino, Llegada = c, Padre = actual, Tiempo = t, Transbordos = transbordos });
                }
            }
            return resultado;
        }

        private static bool YaPasoPor(Parcial parcial, string idParada)
        {
            for (var p = parcial; p != null; p = p.Padre)
                if (p.IdParada == idParada) return true;
            return false;
        }

        private Recorrido Armar(Parcial final)
        {
            var conexiones = new List<Conexion>();
            for (var p = final; p.Padre != null; p = p.Padre) conexiones.Add(p.Llegada);
            conexiones.Reverse();
            try { return _grafo.CrearRecorrido(conexiones); }
            catch (ArgumentException) { return null; }
        }
    }
}
