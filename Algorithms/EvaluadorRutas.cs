using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ruta_App
{
    /// <summary>
    /// Puntúa recorridos según las Preferencias: normaliza cada criterio entre las alternativas (0 = la mejor, 1 = la peor),
    /// pondera con los pesos y explica por qué una opción es mejor o peor que las demás.
    /// </summary>
    public static class EvaluadorRutas
    {
        /// <summary>
        /// Calcula Costo (0-1, menor es mejor) y Puntuacion (0-100) de cada candidato. No ordena: eso lo hace ClasificadorRutas.
        /// La confiabilidad solo participa si TODOS los candidatos tienen el dato; si falta en alguno se omite
        /// (no se inventan valores) y se agrega una advertencia.
        /// </summary>
        public static List<RutaRecomendada> Evaluar(IList<Recorrido> candidatos, Preferencias preferencias, IList<string> advertencias = null)
        {
            if (candidatos == null) throw new ArgumentNullException("candidatos");
            if (preferencias == null) throw new ArgumentNullException("preferencias");
            var errores = preferencias.Validar();
            if (errores.Count > 0) throw new ArgumentException("Preferencias inválidas: " + string.Join(" ", errores));

            var lista = candidatos.Select(r => new RutaRecomendada { Recorrido = r }).ToList();
            if (lista.Count == 0) return lista;

            bool confiabilidadCompleta = candidatos.All(r => r.Confiabilidad.HasValue);
            double wT = preferencias.PesoTiempo, wX = preferencias.PesoTransbordos, wC = preferencias.PesoCaminata, wR = preferencias.PesoConfiabilidad;

            if (wR > 0 && !confiabilidadCompleta)
            {
                if (advertencias != null)
                    advertencias.Add("No hay datos de confiabilidad suficientes para todas las alternativas; ese criterio no se usó en la comparación.");
                wR = 0;
            }
            if (wT + wX + wC + wR <= 0)
            {
                // Solo se había pedido confiabilidad y no hay datos: se comparan los demás criterios con el mismo peso.
                wT = wX = wC = 1;
                if (advertencias != null) advertencias.Add("Sin datos de confiabilidad: se compararon tiempo, transbordos y caminata con el mismo peso.");
            }
            double suma = wT + wX + wC + wR;
            wT /= suma; wX /= suma; wC /= suma; wR /= suma;

            var tiempo = Normalizar(candidatos.Select(r => r.TiempoTotalMin).ToList());
            var transbordos = Normalizar(candidatos.Select(r => (double)r.Transbordos).ToList());
            var caminata = Normalizar(candidatos.Select(r => r.DistanciaCaminataM).ToList());
            var inconfiabilidad = confiabilidadCompleta
                ? Normalizar(candidatos.Select(r => 1 - r.Confiabilidad.Value).ToList())
                : new double[candidatos.Count];

            for (int i = 0; i < lista.Count; i++)
            {
                double costo = wT * tiempo[i] + wX * transbordos[i] + wC * caminata[i] + wR * inconfiabilidad[i];
                lista[i].Costo = costo;
                lista[i].Puntuacion = Math.Round((1 - costo) * 100, 1);
            }
            return lista;
        }

        /// <summary>Normalización mínimo-máximo; si todos son iguales el criterio no distingue y vale 0 para todos.</summary>
        private static double[] Normalizar(IList<double> valores)
        {
            double min = valores.Min(), max = valores.Max();
            var resultado = new double[valores.Count];
            if (max - min < 1e-9) return resultado;
            for (int i = 0; i < valores.Count; i++) resultado[i] = (valores[i] - min) / (max - min);
            return resultado;
        }

        /// <summary>Llena Ventajas, Desventajas y Explicacion comparando cada opción con las demás de la lista.</summary>
        public static void Explicar(IList<RutaRecomendada> alternativas)
        {
            if (alternativas == null) throw new ArgumentNullException("alternativas");
            if (alternativas.Count == 0) return;

            var recorridos = alternativas.Select(a => a.Recorrido).ToList();
            double minTiempo = recorridos.Min(r => r.TiempoTotalMin);
            int minTransbordos = recorridos.Min(r => r.Transbordos);
            double minCaminata = recorridos.Min(r => r.DistanciaCaminataM);
            var conocidas = recorridos.Where(r => r.Confiabilidad.HasValue).Select(r => r.Confiabilidad.Value).ToList();
            double maxConfiabilidad = conocidas.Count > 0 ? conocidas.Max() : 0;
            bool hayComparacion = alternativas.Count > 1;

            foreach (var alt in alternativas)
            {
                var r = alt.Recorrido;
                alt.Ventajas = new List<string>();
                alt.Desventajas = new List<string>();

                if (r.Transbordos == 0) alt.Ventajas.Add("Es directa: sin transbordos");
                else if (hayComparacion && r.Transbordos == minTransbordos && recorridos.Any(o => o.Transbordos > minTransbordos))
                    alt.Ventajas.Add("Menos transbordos (" + Plural(r.Transbordos, "transbordo", "transbordos") + ")");

                if (hayComparacion && r.TiempoTotalMin - minTiempo < 0.5 && recorridos.Any(o => o.TiempoTotalMin - minTiempo >= 0.5))
                    alt.Ventajas.Add("La más rápida: " + Min(r.TiempoTotalMin));

                if (hayComparacion && r.DistanciaCaminataM - minCaminata < 25 && recorridos.Any(o => o.DistanciaCaminataM - minCaminata >= 25))
                    alt.Ventajas.Add("Menor caminata: " + Metros(r.DistanciaCaminataM));

                if (r.Confiabilidad.HasValue && hayComparacion && maxConfiabilidad - r.Confiabilidad.Value < 0.005
                    && recorridos.Any(o => !o.Confiabilidad.HasValue || maxConfiabilidad - o.Confiabilidad.Value >= 0.005))
                    alt.Ventajas.Add("Mayor confiabilidad conocida: " + (r.Confiabilidad.Value * 100).ToString("0", CultureInfo.InvariantCulture) + "%");

                if (r.TodasParadasAccesibles) alt.Ventajas.Add("Todas las paradas donde sube y baja son accesibles");

                if (r.TiempoTotalMin - minTiempo >= 1)
                    alt.Desventajas.Add("Tarda " + Min(r.TiempoTotalMin - minTiempo) + " más que la más rápida");
                if (r.Transbordos > minTransbordos)
                    alt.Desventajas.Add("Requiere " + Plural(r.Transbordos, "transbordo", "transbordos"));
                if (r.DistanciaCaminataM - minCaminata >= 50)
                    alt.Desventajas.Add("Camina " + Metros(r.DistanciaCaminataM) + " (" + Metros(r.DistanciaCaminataM - minCaminata) + " más que la que menos camina)");
                if (!r.Confiabilidad.HasValue)
                    alt.Desventajas.Add("Sin datos suficientes para valorar su confiabilidad");
                if (!r.TodasParadasAccesibles)
                    alt.Desventajas.Add("Alguna parada donde sube o baja no está marcada como accesible");

                alt.Explicacion = ConstruirExplicacion(alt);
            }
        }

        /// <summary>Descripción corta: "Línea 1 (Terminal Norte → Plaza) → camina 120 m → Línea 2 (...)".</summary>
        public static string Resumir(Recorrido r)
        {
            var partes = new List<string>();
            foreach (var t in r.Tramos)
            {
                var ini = t.Paradas[0].Nombre;
                var fin = t.Paradas[t.Paradas.Count - 1].Nombre;
                if (t.EsCaminata) partes.Add("camina " + Metros(t.DistanciaM) + " hasta " + fin);
                else partes.Add((string.IsNullOrEmpty(t.NombreRuta) ? t.IdRuta : t.NombreRuta) + " (" + ini + " → " + fin + ")");
            }
            return string.Join(" → ", partes);
        }

        private static string ConstruirExplicacion(RutaRecomendada alt)
        {
            var r = alt.Recorrido;
            var texto = new List<string>();
            texto.Add(Resumir(r) + ". Total: " + Min(r.TiempoTotalMin) + ", " + Metros(r.DistanciaCaminataM) + " caminando, "
                      + Plural(r.Transbordos, "transbordo", "transbordos") + ".");
            if (alt.Ventajas.Count > 0) texto.Add("Ventajas: " + string.Join("; ", alt.Ventajas) + ".");
            if (alt.Desventajas.Count > 0) texto.Add("A tener en cuenta: " + string.Join("; ", alt.Desventajas) + ".");
            return string.Join(" ", texto);
        }

        private static string Min(double minutos)
        {
            return Math.Round(minutos).ToString("0", CultureInfo.InvariantCulture) + " min";
        }

        private static string Metros(double metros)
        {
            return Math.Round(metros).ToString("0", CultureInfo.InvariantCulture) + " m";
        }

        private static string Plural(int n, string singular, string plural)
        {
            return n.ToString(CultureInfo.InvariantCulture) + " " + (n == 1 ? singular : plural);
        }
    }

    /// <summary>Ordena alternativas y elimina las repetidas.</summary>
    public static class ClasificadorRutas
    {
        /// <summary>
        /// Quita recorridos repetidos conservando el más rápido de cada grupo. Por defecto son iguales si usan los mismos
        /// tramos de autobús (misma ruta, misma parada de subida y bajada); con porSecuenciaDeRutas también se
        /// consideran iguales los que usan las mismas líneas en el mismo orden aunque suban o bajen en otra parada.
        /// </summary>
        public static List<Recorrido> EliminarDuplicados(IEnumerable<Recorrido> recorridos, bool porSecuenciaDeRutas = false)
        {
            if (recorridos == null) throw new ArgumentNullException("recorridos");
            var mejores = new Dictionary<string, Recorrido>();
            var orden = new List<string>();
            foreach (var r in recorridos)
            {
                if (r == null) continue;
                string clave = porSecuenciaDeRutas ? r.FirmaDeRutas : r.Firma;
                Recorrido actual;
                if (!mejores.TryGetValue(clave, out actual)) { mejores[clave] = r; orden.Add(clave); }
                else if (CompararRecorridos(r, actual) < 0) mejores[clave] = r;
            }
            return orden.Select(k => mejores[k]).ToList();
        }

        /// <summary>Igual que el anterior pero para alternativas ya puntuadas: conserva la de mayor puntuación de cada grupo.</summary>
        public static List<RutaRecomendada> EliminarDuplicados(IEnumerable<RutaRecomendada> alternativas, bool porSecuenciaDeRutas = false)
        {
            if (alternativas == null) throw new ArgumentNullException("alternativas");
            var mejores = new Dictionary<string, RutaRecomendada>();
            var orden = new List<string>();
            foreach (var a in alternativas)
            {
                if (a == null || a.Recorrido == null) continue;
                string clave = porSecuenciaDeRutas ? a.Recorrido.FirmaDeRutas : a.Recorrido.Firma;
                RutaRecomendada actual;
                if (!mejores.TryGetValue(clave, out actual)) { mejores[clave] = a; orden.Add(clave); }
                else if (Comparar(a, actual) < 0) mejores[clave] = a;
            }
            return orden.Select(k => mejores[k]).ToList();
        }

        /// <summary>Ordena de mejor a peor (puntuación, luego tiempo, transbordos, caminata y firma) y numera la Posicion desde 1.</summary>
        public static List<RutaRecomendada> Ordenar(IEnumerable<RutaRecomendada> alternativas)
        {
            if (alternativas == null) throw new ArgumentNullException("alternativas");
            var lista = alternativas.Where(a => a != null).ToList();
            lista.Sort(Comparar);
            for (int i = 0; i < lista.Count; i++) lista[i].Posicion = i + 1;
            return lista;
        }

        public static List<RutaRecomendada> SeleccionarMejores(IEnumerable<RutaRecomendada> alternativas, int cantidad)
        {
            return Ordenar(alternativas).Take(Math.Max(0, cantidad)).ToList();
        }

        // Negativo si "a" es mejor que "b". Desempata siempre igual para que el orden sea determinista.
        private static int Comparar(RutaRecomendada a, RutaRecomendada b)
        {
            int c = b.Puntuacion.CompareTo(a.Puntuacion);
            return c != 0 ? c : CompararRecorridos(a.Recorrido, b.Recorrido);
        }

        private static int CompararRecorridos(Recorrido a, Recorrido b)
        {
            int c = a.TiempoTotalMin.CompareTo(b.TiempoTotalMin);
            if (c != 0) return c;
            c = a.Transbordos.CompareTo(b.Transbordos);
            if (c != 0) return c;
            c = a.DistanciaCaminataM.CompareTo(b.DistanciaCaminataM);
            if (c != 0) return c;
            return string.CompareOrdinal(a.Firma, b.Firma);
        }
    }
}
