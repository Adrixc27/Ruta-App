using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Ruta_App.Pruebas
{
    public class PruebasMotorRutas
    {
        // ── Ayudas ───────────────────────────────────────────────────────────────

        private static Func<Conexion, double> Tiempo { get { return c => c.TiempoMin; } }

        private static Conexion C(string origen, string destino, string ruta, double min, double metros = 100)
        {
            return new Conexion { IdOrigen = origen, IdDestino = destino, IdRuta = ruta, TiempoMin = min, DistanciaM = metros };
        }

        /// <summary>Grafo pequeño con coordenadas en (0,0): sirve para probar la lógica sin depender de la geografía.</summary>
        private static GrafoTransporte Mini(string[] paradas, string[] rutas, params Conexion[] conexiones)
        {
            var g = new GrafoTransporte();
            foreach (var id in paradas) g.AgregarParada(new Parada { Id = id, Nombre = "Parada " + id, Accesible = true });
            foreach (var id in rutas) g.AgregarRuta(new RutaAutobus { Id = id, Nombre = "Línea " + id });
            foreach (var c in conexiones) g.AgregarConexion(c);
            return g;
        }

        private static GrafoTransporte GrafoPrueba()
        {
            return ServicioDatosTransporte.ConstruirGrafo(DatosPrueba.Crear());
        }

        /// <summary>Grafo de prueba con el transbordo caminando P03 &lt;-&gt; P09 (como lo arma ServicioTransporte).</summary>
        private static GrafoTransporte GrafoPruebaConCaminata()
        {
            var g = GrafoPrueba();
            g.AgregarConexion(C("P03", "P09", null, 1.2, 91));
            g.AgregarConexion(C("P09", "P03", null, 1.2, 91));
            return g;
        }

        private static Preferencias Estrechas(Prioridad p = Prioridad.Equilibrado)
        {
            var x = Preferencias.Para(p);
            x.RadioBusquedaMetros = 200;   // solo la parada más cercana a cada extremo
            return x;
        }

        private static ServicioTransporte Servicio() { return new ServicioTransporte(DatosPrueba.Crear()); }

        private static Recorrido Recorrer(GrafoTransporte g, string de, string a)
        {
            var r = new AlgoritmoDijkstra(g).CaminoMasCorto(de, a, Tiempo);
            Assert.True(r.Alcanzable, r.Motivo);
            return r.Recorrido;
        }

        private static Recorrido SoloPorRuta(GrafoTransporte g, string de, string a, string idRuta)
        {
            var r = new AlgoritmoDijkstra(g).CaminoMasCorto(de, a, c => c.IdRuta == idRuta ? c.TiempoMin : double.PositiveInfinity);
            Assert.True(r.Alcanzable, r.Motivo);
            return r.Recorrido;
        }

        // ── Datos de prueba ──────────────────────────────────────────────────────

        [Fact]
        public void DatosPrueba_CumplenLosMinimosYSonValidos()
        {
            var datos = DatosPrueba.Crear();
            Assert.True(datos.Paradas.Count >= 8);
            Assert.True(datos.Rutas.Count >= 3);
            Assert.Contains(datos.Rutas, r => r.Sentido == SentidoRecorrido.Unidireccional);
            Assert.Contains(datos.Rutas, r => r.Sentido == SentidoRecorrido.Bidireccional);
            var v = ServicioDatosTransporte.Validar(datos);
            Assert.True(v.EsValido, string.Join("; ", v.Errores));
        }

        // ── Dijkstra ─────────────────────────────────────────────────────────────

        [Fact]
        public void Dijkstra_ReconstruyeElCaminoMasRapidoYSusMetricas()
        {
            var r = Recorrer(GrafoPrueba(), "P01", "P05");   // R3 exprés (14 min) gana a R1 (24 min)
            Assert.Equal(new[] { "P01", "P09", "P05" }, r.Paradas.Select(p => p.Id).ToArray());
            Assert.Equal(new[] { "R3" }, r.Autobuses.Select(a => a.Id).ToArray());
            Assert.Equal(14, r.TiempoTotalMin, 6);
            Assert.Equal(14, r.TiempoAutobusMin, 6);
            Assert.Equal(0, r.TiempoCaminataMin, 6);
            Assert.Equal(0, r.Transbordos);
            Assert.Equal(r.Conexiones.Sum(c => c.DistanciaM), r.DistanciaTotalM, 6);
        }

        [Fact]
        public void Dijkstra_RespetaElSentidoUnidireccional()
        {
            // R3 solo va P01 -> P05; de regreso hay que usar R1 (24 min)
            var r = Recorrer(GrafoPrueba(), "P05", "P01");
            Assert.Equal(new[] { "R1" }, r.Autobuses.Select(a => a.Id).ToArray());
            Assert.Equal(24, r.TiempoTotalMin, 6);
        }

        [Fact]
        public void Dijkstra_DestinoInaccesible_NoLanzaYExplicaElMotivo()
        {
            var d = new AlgoritmoDijkstra(GrafoPrueba());

            var sinServicio = d.CaminoMasCorto("P01", "P12", Tiempo);          // parada sin rutas
            Assert.False(sinServicio.Alcanzable);
            Assert.Null(sinServicio.Recorrido);
            Assert.True(double.IsPositiveInfinity(sinServicio.Costo));
            Assert.False(string.IsNullOrEmpty(sinServicio.Motivo));

            Assert.True(d.CaminoMasCorto("P08", "P11", Tiempo).Alcanzable);     // ida por el ramal
            Assert.False(d.CaminoMasCorto("P11", "P08", Tiempo).Alcanzable);    // el ramal no tiene regreso

            Assert.False(d.CaminoMasCorto("NOEXISTE", "P01", Tiempo).Alcanzable);
            Assert.False(d.CaminoMasCorto("P01", "NOEXISTE", Tiempo).Alcanzable);
            Assert.False(d.CaminoMasCorto("P01", "P01", Tiempo).Alcanzable);
        }

        [Fact]
        public void Dijkstra_RechazaCostosNegativosONoNumericos()
        {
            var d = new AlgoritmoDijkstra(GrafoPrueba());
            Assert.Throws<InvalidOperationException>(() => d.CaminoMasCorto("P01", "P05", c => -1));
            Assert.Throws<InvalidOperationException>(() => d.CaminoMasCorto("P01", "P05", c => double.NaN));
            Assert.Throws<ArgumentException>(() => d.CaminoMasCorto("P01", "P05", Tiempo, -5));
        }

        [Fact]
        public void Dijkstra_ConCiclosEnElGrafoTerminaYDevuelveCaminoSimple()
        {
            var g = Mini(new[] { "A", "B", "C" }, new[] { "X" },
                C("A", "B", "X", 5), C("B", "A", "X", 5), C("B", "C", "X", 5), C("C", "B", "X", 5));
            var r = Recorrer(g, "A", "C");
            Assert.Equal(new[] { "A", "B", "C" }, r.Paradas.Select(p => p.Id).ToArray());
            Assert.Equal(10, r.TiempoTotalMin, 6);
        }

        [Fact]
        public void Dijkstra_LaPenalizacionPorTransbordoCambiaElResultado()
        {
            var g = Mini(new[] { "A", "B", "C" }, new[] { "X", "Y", "Z" },
                C("A", "C", "X", 20), C("A", "B", "Y", 5), C("B", "C", "Z", 5));
            var d = new AlgoritmoDijkstra(g);

            var sinPenalizar = d.CaminoMasCorto("A", "C", Tiempo, 0);
            Assert.Equal(1, sinPenalizar.Recorrido.Transbordos);      // A->B->C: 10 min
            Assert.Equal(10, sinPenalizar.Costo, 6);

            var penalizado = d.CaminoMasCorto("A", "C", Tiempo, 20);
            Assert.Equal(0, penalizado.Recorrido.Transbordos);        // directo: 20 min < 10 + 20
            Assert.Equal(20, penalizado.Costo, 6);
        }

        [Fact]
        public void Dijkstra_ConSoloAccesibles_NoPuedeBajarEnUnaParadaNoAccesible()
        {
            var d = new AlgoritmoDijkstra(GrafoPrueba());
            Assert.True(d.CaminoMasCorto("P01", "P04", Tiempo, 0, false).Alcanzable);
            Assert.False(d.CaminoMasCorto("P01", "P04", Tiempo, 0, true).Alcanzable);   // P04 no es accesible
            // Pasar por P04 sin bajarse sí es posible
            Assert.True(d.CaminoMasCorto("P03", "P05", Tiempo, 0, true).Alcanzable);
        }

        [Fact]
        public void DistanciasHaciaDestino_SonCotasInferioresYOmitenLoInalcanzable()
        {
            var d = new AlgoritmoDijkstra(GrafoPrueba());
            var cotas = d.DistanciasHaciaDestino("P05", Tiempo);
            Assert.Equal(0, cotas["P05"], 6);
            Assert.Equal(14, cotas["P01"], 6);
            Assert.False(cotas.ContainsKey("P12"));
            Assert.False(cotas.ContainsKey("P11"));
        }

        // ── Grafo ────────────────────────────────────────────────────────────────

        [Fact]
        public void Grafo_RechazaConexionesYParadasInvalidas()
        {
            var g = Mini(new[] { "A", "B" }, new[] { "X" }, C("A", "B", "X", 5));
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("A", "ZZ", "X", 5)));      // destino inexistente
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("ZZ", "B", "X", 5)));      // origen inexistente
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("B", "A", "NOHAY", 5)));   // ruta inexistente
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("B", "A", "X", -1)));      // tiempo negativo
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("B", "A", "X", double.NaN)));
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("A", "A", "X", 1)));       // a sí misma
            Assert.Throws<ArgumentException>(() => g.AgregarConexion(C("A", "B", "X", 5)));       // repetida
            Assert.Throws<ArgumentException>(() => g.AgregarParada(new Parada { Id = "A" }));
            Assert.Throws<ArgumentException>(() => g.AgregarParada(new Parada { Id = " " }));
            Assert.Empty(g.Salientes("NOEXISTE"));
            Assert.Null(g.ObtenerParada("NOEXISTE"));
        }

        // ── Recorrido: métricas y transbordos ────────────────────────────────────

        [Fact]
        public void Recorrido_CuentaTransbordosYNoDuplicaTramos()
        {
            var g = GrafoPruebaConCaminata();
            var r2a = g.Salientes("P06").First(c => c.IdDestino == "P03");
            var r1a = g.Salientes("P03").First(c => c.IdRuta == "R1" && c.IdDestino == "P04");
            var r1b = g.Salientes("P04").First(c => c.IdRuta == "R1" && c.IdDestino == "P05");

            var r = g.CrearRecorrido(new[] { r2a, r1a, r1b });   // R2 y luego R1 en la misma parada
            Assert.Equal(1, r.Transbordos);
            Assert.Equal(2, r.Autobuses.Count);
            Assert.Equal(2, r.Tramos.Count);                      // R1 son dos conexiones pero un solo tramo
            Assert.Equal(7 + 6 + 6, r.TiempoAutobusMin, 6);
            Assert.Equal(r2a.DistanciaM + r1a.DistanciaM + r1b.DistanciaM, r.DistanciaAutobusM, 6);
            Assert.Equal(0, r.DistanciaCaminataM, 6);
        }

        [Fact]
        public void Recorrido_CaminarEntreAutobusesCuentaUnSoloTransbordo()
        {
            var g = GrafoPruebaConCaminata();
            var r2 = g.Salientes("P06").First(c => c.IdDestino == "P03");
            var camina = g.Salientes("P03").First(c => c.EsCaminata);
            var r3 = g.Salientes("P09").First(c => c.IdRuta == "R3" && c.IdDestino == "P05");

            var r = g.CrearRecorrido(new[] { r2, camina, r3 });
            Assert.Equal(1, r.Transbordos);
            Assert.Equal(3, r.Tramos.Count);
            Assert.Equal(14, r.TiempoAutobusMin, 6);
            Assert.Equal(1.2, r.TiempoCaminataMin, 6);
            Assert.Equal(15.2, r.TiempoTotalMin, 6);
            Assert.Equal(91, r.DistanciaCaminataM, 6);
        }

        [Fact]
        public void Recorrido_AsignarAccesosDosVecesNoDuplicaLaCaminata()
        {
            var r = Recorrer(GrafoPrueba(), "P01", "P05");
            var o = new Coordenada(28.6408, -106.0800);
            var d = new Coordenada(28.6408, -106.0600);
            r.AsignarAccesos(o, 100, 1.3, d, 50, 0.6);
            Assert.Equal(150, r.DistanciaCaminataM, 6);
            Assert.Equal(1.9, r.TiempoCaminataMin, 6);
            r.AsignarAccesos(o, 100, 1.3, d, 50, 0.6);
            Assert.Equal(150, r.DistanciaCaminataM, 6);
            Assert.Equal(1.9, r.TiempoCaminataMin, 6);
            Assert.Equal(14 + 1.9, r.TiempoTotalMin, 6);
            var trazo = r.Trazo;
            Assert.Same(o, trazo.First());
            Assert.Same(d, trazo.Last());
            Assert.Equal(5, trazo.Count);   // origen + 3 paradas + destino
        }

        [Fact]
        public void Recorrido_RechazaCiclosHuecosYTramosRepetidos()
        {
            var g = Mini(new[] { "A", "B", "C" }, new[] { "X" }, C("A", "B", "X", 5), C("B", "A", "X", 5), C("B", "C", "X", 5));
            var ab = g.Salientes("A")[0];
            var ba = g.Salientes("B").First(c => c.IdDestino == "A");
            var bc = g.Salientes("B").First(c => c.IdDestino == "C");

            Assert.Throws<ArgumentException>(() => g.CrearRecorrido(new[] { ab, ba }));        // vuelve a A: ciclo
            Assert.Throws<ArgumentException>(() => g.CrearRecorrido(new[] { ab, ab }));        // mismo tramo dos veces
            Assert.Throws<ArgumentException>(() => g.CrearRecorrido(new[] { ba, bc }));        // no son consecutivas
            Assert.Throws<ArgumentException>(() => g.CrearRecorrido(new Conexion[0]));
        }

        [Fact]
        public void Confiabilidad_SeMultiplicaSoloSiTodasSonConocidasYNuncaSeInventa()
        {
            var g = GrafoPruebaConCaminata();
            Assert.Equal(0.85, Recorrer(g, "P01", "P05").Confiabilidad.Value, 6);       // solo R3
            Assert.Null(Recorrer(g, "P06", "P08").Confiabilidad);                       // solo R2 (sin dato)

            var r1 = g.Salientes("P01").First(c => c.IdRuta == "R1");
            var r1b = g.Salientes("P02").First(c => c.IdRuta == "R1" && c.IdDestino == "P03");
            var camina = g.Salientes("P03").First(c => c.EsCaminata);
            var r3 = g.Salientes("P09").First(c => c.IdRuta == "R3");
            var mixto = g.CrearRecorrido(new[] { r1, r1b, camina, r3 });                // R1 (0.92) + R3 (0.85)
            Assert.Equal(0.92 * 0.85, mixto.Confiabilidad.Value, 6);

            Assert.Null(Recorrer(g, "P01", "P08").Confiabilidad);                       // incluye R2 sin dato
        }

        // ── Búsqueda de alternativas ─────────────────────────────────────────────

        [Fact]
        public void BuscadorRutas_EncuentraDirectosYConTransbordoOrdenadosPorTiempo()
        {
            var g = GrafoPruebaConCaminata();
            var rutas = new BuscadorRutas(g).Buscar("P01", "P05", new Preferencias());
            Assert.Equal("R3", rutas[0].FirmaDeRutas);   // directo exprés: 14 min
            Assert.Equal(14, rutas[0].TiempoTotalMin, 6);
            // Con transbordo caminando entre P03 y P09 hay dos combinaciones empatadas en 20.2 min (R1+R3 y R3+R1)
            var conTransbordo = rutas.Where(r => r.Transbordos == 1 && Math.Abs(r.TiempoTotalMin - 20.2) < 1e-6).Select(r => r.FirmaDeRutas).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "R1>R3", "R3>R1" }, conTransbordo);
            var directoLento = rutas.Single(r => r.FirmaDeRutas == "R1");   // directo lento: 24 min
            Assert.Equal(24, directoLento.TiempoTotalMin, 6);
            Assert.Equal(0, directoLento.Transbordos);
            // Salen ordenadas por tiempo
            var tiempos = rutas.Select(r => r.TiempoTotalMin).ToList();
            Assert.Equal(tiempos.OrderBy(t => t).ToList(), tiempos);
            // Sin ciclos: ninguna parada se repite dentro de un recorrido
            Assert.All(rutas, r => Assert.Equal(r.Paradas.Count, r.Paradas.Select(p => p.Id).Distinct().Count()));
        }

        [Fact]
        public void BuscadorRutas_ConTransbordo_EncuentraLasDosFormasDeCambiar()
        {
            var g = GrafoPruebaConCaminata();
            var rutas = new BuscadorRutas(g).Buscar("P01", "P08", new Preferencias());
            Assert.True(rutas.Count >= 2);
            Assert.Equal(22.2, rutas[0].TiempoTotalMin, 6);   // R3 + caminar P09->P03 + R2
            Assert.Equal(1, rutas[0].Transbordos);
            Assert.Equal(26, rutas[1].TiempoTotalMin, 6);     // R1 + R2 en la misma parada
            Assert.Equal(1, rutas[1].Transbordos);
            Assert.All(rutas, r => Assert.Equal("P08", r.ParadaFinal.Id));
            Assert.All(rutas, r => Assert.True(r.Transbordos >= 1));   // no existe un directo
            // Aun dentro del límite de tiempo, ninguno repite parada
            Assert.All(rutas, r => Assert.Equal(r.Paradas.Count, r.Paradas.Select(p => p.Id).Distinct().Count()));
        }

        [Fact]
        public void BuscadorRutas_RespetaLosLimitesConfigurables()
        {
            var g = GrafoPruebaConCaminata();
            var buscador = new BuscadorRutas(g);

            Assert.Empty(buscador.Buscar("P01", "P08", new Preferencias { MaxTransbordos = 0 }));            // no hay directo
            Assert.Single(buscador.Buscar("P01", "P05", new Preferencias { MaxAlternativasPorPar = 1 }));
            // Con un factor de tiempo estricto solo queda la más rápida (14 min; las demás tardan más de 14 x 1.05)
            Assert.Single(buscador.Buscar("P01", "P05", new Preferencias { FactorTiempoMaximo = 1.05 }));
            // Con solo una expansión se corta antes de completar ningún recorrido
            Assert.Empty(buscador.Buscar("P01", "P05", new Preferencias { MaxExpansiones = 1 }));
        }

        [Fact]
        public void BuscadorRutas_DestinoInaccesibleOParadasInvalidas_DevuelveVacio()
        {
            var buscador = new BuscadorRutas(GrafoPruebaConCaminata());
            Assert.Empty(buscador.Buscar("P01", "P12", new Preferencias()));
            Assert.Empty(buscador.Buscar("P11", "P01", new Preferencias()));
            Assert.Empty(buscador.Buscar("X", "P01", new Preferencias()));
            Assert.Empty(buscador.Buscar("P01", "P01", new Preferencias()));
        }

        // ── Multicriterio (Pareto) ───────────────────────────────────────────────

        [Fact]
        public void Multicriterio_DescartaLasDominadas()
        {
            // Directo A->B (10 min, sin transbordo) domina a A->C->B (30 min, 1 transbordo)
            var g = Mini(new[] { "A", "B", "C" }, new[] { "X", "Y", "Z" },
                C("A", "B", "X", 10), C("A", "C", "Y", 15), C("C", "B", "Z", 15));
            var prefs = new Preferencias { FactorTiempoMaximo = 5 };
            var todas = new BuscadorRutas(g).Buscar("A", "B", prefs);
            Assert.Equal(2, todas.Count);
            Assert.True(BuscadorMulticriterio.Domina(todas[0], todas[1]));
            Assert.False(BuscadorMulticriterio.Domina(todas[1], todas[0]));

            var frente = new BuscadorMulticriterio(g).Buscar("A", "B", prefs);
            Assert.Single(frente);
            Assert.Equal("X", frente[0].FirmaDeRutas);
        }

        [Fact]
        public void Multicriterio_ConservaAlternativasConVentajasDistintas()
        {
            // R3 es más rápida (14 min, 0.85) y R1 más confiable (24 min, 0.92): ninguna domina a la otra.
            // La combinación R1 + caminar + R3 (20 min, 1 transbordo, 0.78) queda dominada por R3.
            var frente = new BuscadorMulticriterio(GrafoPruebaConCaminata()).Buscar("P01", "P05", new Preferencias());
            Assert.Equal(new[] { "R1", "R3" }, frente.Select(r => r.FirmaDeRutas).OrderBy(x => x).ToArray());
        }

        [Fact]
        public void Multicriterio_LaConfiabilidadDesconocidaNoSeUsaParaDominar()
        {
            var g = new GrafoTransporte();
            foreach (var id in new[] { "A", "B" }) g.AgregarParada(new Parada { Id = id, Accesible = true });
            g.AgregarRuta(new RutaAutobus { Id = "SIN", Confiabilidad = null });
            g.AgregarRuta(new RutaAutobus { Id = "ALTA", Confiabilidad = 0.99 });
            g.AgregarRuta(new RutaAutobus { Id = "BAJA", Confiabilidad = 0.50 });
            g.AgregarRuta(new RutaAutobus { Id = "LENTA", Confiabilidad = 0.99 });
            g.AgregarConexion(C("A", "B", "SIN", 10));
            g.AgregarConexion(C("A", "B", "ALTA", 10));
            g.AgregarConexion(C("A", "B", "BAJA", 10));
            g.AgregarConexion(C("A", "B", "LENTA", 20));
            Func<string, Recorrido> por = ruta => g.CrearRecorrido(new[] { g.Salientes("A").First(c => c.IdRuta == ruta) });

            // Con el dato conocido en ambos, la más confiable domina a igualdad de lo demás
            Assert.True(BuscadorMulticriterio.Domina(por("ALTA"), por("BAJA")));
            Assert.False(BuscadorMulticriterio.Domina(por("BAJA"), por("ALTA")));
            // Si falta el dato en uno, esa dimensión no cuenta: iguales en todo lo demás, ninguna domina
            Assert.False(BuscadorMulticriterio.Domina(por("SIN"), por("ALTA")));
            Assert.False(BuscadorMulticriterio.Domina(por("ALTA"), por("SIN")));
            // Y la lenta con dato conocido sí es dominada por la rápida sin dato (el tiempo decide)
            Assert.True(BuscadorMulticriterio.Domina(por("SIN"), por("LENTA")));
            Assert.False(BuscadorMulticriterio.Domina(por("LENTA"), por("SIN")));
        }

        // ── Evaluación y preferencias ────────────────────────────────────────────

        private static List<Recorrido> DirectoContraConTransbordo(out Recorrido directo, out Recorrido rapido)
        {
            var g = Mini(new[] { "A", "B", "C" }, new[] { "X", "Y", "Z" },
                C("A", "C", "X", 30, 3000), C("A", "B", "Y", 6, 500), C("B", "C", "Z", 6, 500));
            directo = g.CrearRecorrido(new[] { g.Salientes("A").First(c => c.IdRuta == "X") });
            rapido = g.CrearRecorrido(new[] { g.Salientes("A").First(c => c.IdRuta == "Y"), g.Salientes("B").First(c => c.IdRuta == "Z") });
            return new List<Recorrido> { directo, rapido };
        }

        [Fact]
        public void Evaluador_LasPreferenciasCambianElOrden()
        {
            Recorrido directo, rapido;
            var candidatos = DirectoContraConTransbordo(out directo, out rapido);

            var porVelocidad = ClasificadorRutas.Ordenar(EvaluadorRutas.Evaluar(candidatos, Preferencias.Para(Prioridad.MasRapida)));
            Assert.Same(rapido, porVelocidad[0].Recorrido);

            var porTransbordos = ClasificadorRutas.Ordenar(EvaluadorRutas.Evaluar(candidatos, Preferencias.Para(Prioridad.MenosTransbordos)));
            Assert.Same(directo, porTransbordos[0].Recorrido);

            // Pesos personalizados: solo importa el tiempo
            var soloTiempo = new Preferencias { PesoTiempo = 1, PesoTransbordos = 0, PesoCaminata = 0, PesoConfiabilidad = 0 };
            var r = ClasificadorRutas.Ordenar(EvaluadorRutas.Evaluar(candidatos, soloTiempo));
            Assert.Same(rapido, r[0].Recorrido);
            Assert.Equal(100, r[0].Puntuacion, 6);   // la mejor en el único criterio activo
            Assert.Equal(0, r[1].Puntuacion, 6);
            Assert.Equal(new[] { 1, 2 }, r.Select(x => x.Posicion).ToArray());
        }

        [Fact]
        public void Evaluador_NormalizaCadaCriterioEntre0Y1()
        {
            Recorrido directo, rapido;
            var evaluadas = EvaluadorRutas.Evaluar(DirectoContraConTransbordo(out directo, out rapido), Preferencias.Para(Prioridad.Equilibrado));
            Assert.All(evaluadas, e => { Assert.InRange(e.Costo, 0, 1); Assert.InRange(e.Puntuacion, 0, 100); });
        }

        [Fact]
        public void Evaluador_SinDatosDeConfiabilidadNoLosInventaYAdvierte()
        {
            Recorrido directo, rapido;
            var advertencias = new List<string>();
            var evaluadas = EvaluadorRutas.Evaluar(DirectoContraConTransbordo(out directo, out rapido), Preferencias.Para(Prioridad.MasConfiable), advertencias);
            Assert.Single(advertencias);
            Assert.Contains("confiabilidad", advertencias[0]);
            Assert.All(evaluadas, e => Assert.Null(e.Recorrido.Confiabilidad));
            // Sin datos de confiabilidad se comparan los otros tres criterios con el mismo peso
            Assert.Contains(evaluadas, e => e.Puntuacion > 0);
        }

        [Fact]
        public void Evaluador_ConAlgunaConfiabilidadDesconocidaLaOmiteDeTodas()
        {
            var g = GrafoPruebaConCaminata();
            var candidatos = new List<Recorrido> { Recorrer(g, "P01", "P05"), Recorrer(g, "P06", "P08") };   // 0.85 y desconocida
            var advertencias = new List<string>();
            EvaluadorRutas.Evaluar(candidatos, Preferencias.Para(Prioridad.MasConfiable), advertencias);
            Assert.Single(advertencias);

            var sinAviso = new List<string>();   // si el peso de confiabilidad es 0 no hay nada que avisar
            EvaluadorRutas.Evaluar(candidatos, new Preferencias { PesoConfiabilidad = 0 }, sinAviso);
            Assert.Empty(sinAviso);
        }

        [Fact]
        public void Evaluador_RechazaPreferenciasInvalidas()
        {
            Recorrido directo, rapido;
            var c = DirectoContraConTransbordo(out directo, out rapido);
            Assert.Throws<ArgumentException>(() => EvaluadorRutas.Evaluar(c, new Preferencias { PesoTiempo = -1 }));
            Assert.Throws<ArgumentNullException>(() => EvaluadorRutas.Evaluar(null, new Preferencias()));
        }

        [Fact]
        public void Preferencias_ValidacionDetectaProblemas()
        {
            Assert.Empty(new Preferencias().Validar());
            Assert.NotEmpty(new Preferencias { PesoTiempo = -1 }.Validar());
            Assert.NotEmpty(new Preferencias { PesoTiempo = 0, PesoTransbordos = 0, PesoCaminata = 0, PesoConfiabilidad = 0 }.Validar());
            Assert.NotEmpty(new Preferencias { PesoTiempo = double.NaN }.Validar());
            Assert.NotEmpty(new Preferencias { RadioBusquedaMetros = 0 }.Validar());
            Assert.NotEmpty(new Preferencias { FactorRodeoCaminata = 0.5 }.Validar());   // caminando no puede ser menos que en línea recta
            Assert.NotEmpty(new Preferencias { VelocidadCaminataKmh = 0 }.Validar());
            Assert.NotEmpty(new Preferencias { MaxTransbordos = -1 }.Validar());

            var rapida = Preferencias.Para(Prioridad.MasRapida);
            Assert.True(rapida.PesoTiempo > rapida.PesoCaminata);
            var confiable = Preferencias.Para(Prioridad.MasConfiable);
            Assert.True(confiable.PesoConfiabilidad > confiable.PesoTiempo);
        }

        // ── Clasificación y duplicados ───────────────────────────────────────────

        [Fact]
        public void Clasificador_EliminaDuplicadosExactosYPorSecuenciaDeRutas()
        {
            var g = GrafoPrueba();
            var a = Recorrer(g, "P01", "P05");
            var copiaDeA = Recorrer(g, "P01", "P05");
            var r1Largo = SoloPorRuta(g, "P01", "P05", "R1");
            var r1Corto = SoloPorRuta(g, "P01", "P03", "R1");

            Assert.NotSame(a, copiaDeA);
            Assert.Single(ClasificadorRutas.EliminarDuplicados(new[] { a, copiaDeA }));

            // Misma línea R1, distinta parada de bajada: distintas en modo exacto, una sola por secuencia de rutas
            Assert.Equal(2, ClasificadorRutas.EliminarDuplicados(new[] { r1Largo, r1Corto }).Count);
            var porSecuencia = ClasificadorRutas.EliminarDuplicados(new[] { r1Largo, r1Corto }, true);
            Assert.Single(porSecuencia);
            Assert.Same(r1Corto, porSecuencia[0]);   // se queda la más rápida
        }

        [Fact]
        public void Clasificador_OrdenaPorPuntuacionYDesempataDeFormaDeterminista()
        {
            var g = GrafoPrueba();
            var rapido = Recorrer(g, "P01", "P05");
            var lento = SoloPorRuta(g, "P01", "P05", "R1");
            var lista = new List<RutaRecomendada>
            {
                new RutaRecomendada { Recorrido = lento, Puntuacion = 80 },
                new RutaRecomendada { Recorrido = rapido, Puntuacion = 80 },
                new RutaRecomendada { Recorrido = lento, Puntuacion = 95 }
            };
            var orden = ClasificadorRutas.Ordenar(lista);
            Assert.Equal(new[] { 95.0, 80.0, 80.0 }, orden.Select(o => o.Puntuacion).ToArray());
            Assert.Same(rapido, orden[1].Recorrido);   // empate de puntuación: gana el de menor tiempo
            Assert.Equal(new[] { 1, 2, 3 }, orden.Select(o => o.Posicion).ToArray());
            Assert.Equal(2, ClasificadorRutas.SeleccionarMejores(lista, 2).Count);
            Assert.Empty(ClasificadorRutas.SeleccionarMejores(lista, 0));
        }

        // ── Ubicaciones ──────────────────────────────────────────────────────────

        [Fact]
        public void Ubicaciones_DistanciaGeograficaYCaminataEstimada()
        {
            double grado = ServicioUbicaciones.DistanciaLineaRectaM(0, 0, 1, 0);
            Assert.InRange(grado, 111000, 111400);   // un grado de latitud ≈ 111.2 km
            Assert.Equal(0, ServicioUbicaciones.DistanciaLineaRectaM(10, 20, 10, 20), 6);
            Assert.Equal(grado, ServicioUbicaciones.DistanciaLineaRectaM(new Coordenada(0, 0), new Coordenada(1, 0)), 6);

            var ubic = new ServicioUbicaciones(DatosPrueba.Crear().Paradas);
            var prefs = new Preferencias { RadioBusquedaMetros = 600, FactorRodeoCaminata = 1.5 };
            var cercanas = ubic.BuscarParadasCercanas(DatosPrueba.CercaDe("P01"), prefs);
            Assert.Equal(new[] { "P01", "P02" }, cercanas.Select(c => c.Parada.Id).ToArray());   // P02 está a ~500 m
            Assert.All(cercanas, c => Assert.True(c.DistanciaLineaRectaM <= 600));
            // La caminata NO es la línea recta: es la estimación con el factor de rodeo
            Assert.All(cercanas, c => Assert.Equal(c.DistanciaLineaRectaM * 1.5, c.DistanciaCaminandoM, 6));
            Assert.Equal(cercanas[0].DistanciaCaminandoM / (4.5 * 1000 / 60), cercanas[0].TiempoCaminandoMin, 6);
        }

        [Fact]
        public void Ubicaciones_RadioMaximoUbicacionesInvalidasYAccesibilidad()
        {
            var ubic = new ServicioUbicaciones(DatosPrueba.Crear().Paradas);
            var prefs = new Preferencias { RadioBusquedaMetros = 800 };
            Assert.Single(ubic.BuscarParadasCercanas(DatosPrueba.CercaDe("P01"), prefs, 1));
            Assert.Empty(ubic.BuscarParadasCercanas(new Coordenada(10, 10), prefs));              // lejos de todo
            Assert.Empty(ubic.BuscarParadasCercanas(new Coordenada(double.NaN, 0), prefs));       // inválida
            Assert.Empty(ubic.BuscarParadasCercanas(new Coordenada(91, 0), prefs));
            Assert.Empty(ubic.BuscarParadasCercanas(null, prefs));

            var soloAccesibles = new Preferencias { RadioBusquedaMetros = 200, SoloParadasAccesibles = true };
            Assert.Empty(ubic.BuscarParadasCercanas(DatosPrueba.CercaDe("P04"), soloAccesibles));   // P04 no es accesible
            Assert.Single(ubic.BuscarParadasCercanas(DatosPrueba.CercaDe("P04"), new Preferencias { RadioBusquedaMetros = 200 }));
        }

        // ── Datos: validación y carga ────────────────────────────────────────────

        [Fact]
        public void Datos_ValidacionDetectaDatosInvalidosOIncompletos()
        {
            Func<Action<DatosTransporte>, ResultadoValidacion> con = cambio =>
            {
                var d = DatosPrueba.Crear();
                cambio(d);
                return ServicioDatosTransporte.Validar(d);
            };

            Assert.False(ServicioDatosTransporte.Validar(null).EsValido);
            Assert.False(con(d => d.Paradas.Add(new Parada { Id = "P01", Nombre = "Repetida" })).EsValido);        // id repetido
            Assert.False(con(d => d.Paradas.Add(new Parada { Id = "", Nombre = "Sin id" })).EsValido);
            Assert.False(con(d => d.Paradas[0].Latitud = 123).EsValido);                                           // coordenada imposible
            Assert.False(con(d => d.Paradas[0].Longitud = double.NaN).EsValido);
            Assert.False(con(d => d.Rutas[0].IdsParadas.Add("NOEXISTE")).EsValido);                                // ruta con parada inexistente
            Assert.False(con(d => d.Rutas[0].IdsParadas = new List<string> { "P01" }).EsValido);                   // ruta de una sola parada
            Assert.False(con(d => d.Rutas[0].IdsParadas.Add("P01")).EsValido);                                     // parada repetida en la ruta
            Assert.False(con(d => d.Rutas[0].Confiabilidad = 1.5).EsValido);
            Assert.False(con(d => d.Rutas[0].Confiabilidad = -0.1).EsValido);
            Assert.False(con(d => d.Rutas.Add(new RutaAutobus { Id = "R1" })).EsValido);                           // ruta repetida
            Assert.False(con(d => d.Conexiones[0].TiempoMin = -3).EsValido);
            Assert.False(con(d => d.Conexiones[0].TiempoMin = 0).EsValido);
            Assert.False(con(d => d.Conexiones[0].DistanciaM = double.NaN).EsValido);
            Assert.False(con(d => d.Conexiones[0].IdDestino = "NOEXISTE").EsValido);                               // conexión a parada inexistente
            Assert.False(con(d => d.Conexiones[0].IdRuta = "NOEXISTE").EsValido);
            Assert.False(con(d => d.Conexiones[0].IdRuta = null).EsValido);
            Assert.False(con(d => d.Conexiones.RemoveAt(0)).EsValido);                                             // falta una conexión que exige la ruta
            Assert.False(con(d => d.Conexiones.Add(C("P05", "P01", "R3", 5))).EsValido);                           // contra el sentido de una ruta unidireccional
            Assert.False(con(d => d.Conexiones.Add(d.Conexiones[0])).EsValido);                                    // conexión duplicada
            Assert.False(con(d => d.Conexiones = null).EsValido);                                                  // datos incompletos
        }

        [Fact]
        public void Datos_AdvertenciasParaParadasSinServicioYRutasSinConfiabilidad()
        {
            var v = ServicioDatosTransporte.Validar(DatosPrueba.Crear());
            Assert.True(v.EsValido);
            Assert.Contains(v.Advertencias, a => a.Contains("P12"));   // parada sin ruta
            Assert.Contains(v.Advertencias, a => a.Contains("R2"));    // sin confiabilidad
        }

        [Fact]
        public void Datos_ConstruirGrafoConDatosInvalidosLanzaConTodosLosErrores()
        {
            var d = DatosPrueba.Crear();
            d.Conexiones[0].TiempoMin = -1;
            d.Rutas[1].Confiabilidad = 7;
            var ex = Assert.Throws<InvalidDataException>(() => ServicioDatosTransporte.ConstruirGrafo(d));
            Assert.Contains("R2", ex.Message);
            Assert.Throws<InvalidDataException>(() => new ServicioTransporte(d));
            Assert.Throws<InvalidDataException>(() => ServicioDatosTransporte.ConstruirGrafo(null));
        }

        [Fact]
        public void Datos_JsonIdaYVuelta()
        {
            var original = DatosPrueba.Crear();
            string json = ServicioDatosTransporte.AJson(original);
            var leido = ServicioDatosTransporte.DesdeJson(json);

            Assert.Contains("Unidireccional", json);   // los sentidos se guardan con su nombre, no con un número
            Assert.Equal(original.Paradas.Count, leido.Paradas.Count);
            Assert.Equal(original.Rutas.Count, leido.Rutas.Count);
            Assert.Equal(original.Conexiones.Count, leido.Conexiones.Count);
            Assert.Null(leido.Rutas.First(r => r.Id == "R2").Confiabilidad);           // lo desconocido sigue desconocido
            Assert.Equal(0.92, leido.Rutas.First(r => r.Id == "R1").Confiabilidad.Value, 6);
            Assert.Equal(SentidoRecorrido.Unidireccional, leido.Rutas.First(r => r.Id == "R3").Sentido);
            Assert.False(leido.Paradas.First(p => p.Id == "P04").Accesible);
            Assert.True(ServicioDatosTransporte.Validar(leido).EsValido);

            string archivo = Path.Combine(Path.GetTempPath(), "ruta_datos_" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                File.WriteAllText(archivo, json);
                Assert.Equal(original.Paradas.Count, ServicioDatosTransporte.DesdeArchivo(archivo).Paradas.Count);
            }
            finally { File.Delete(archivo); }
        }

        [Fact]
        public void Datos_JsonYArchivoInvalidosLanzanErroresClaros()
        {
            Assert.Throws<InvalidDataException>(() => ServicioDatosTransporte.DesdeJson(""));
            Assert.Throws<InvalidDataException>(() => ServicioDatosTransporte.DesdeJson("{ esto no es json"));
            Assert.Throws<InvalidDataException>(() => ServicioDatosTransporte.DesdeJson("null"));
            Assert.Throws<FileNotFoundException>(() => ServicioDatosTransporte.DesdeArchivo(Path.Combine(Path.GetTempPath(), "no_existe_" + Guid.NewGuid().ToString("N") + ".json")));
            Assert.Throws<ArgumentException>(() => ServicioDatosTransporte.DesdeArchivo(" "));
        }

        [Fact]
        public void Datos_RutaCircularExigeLaConexionDeCierreYSePuedeRecorrer()
        {
            Func<bool, DatosTransporte> circular = cerrada =>
            {
                var d = new DatosTransporte();
                foreach (var id in new[] { "A", "B", "C" }) d.Paradas.Add(new Parada { Id = id, Nombre = id, Latitud = 28, Longitud = -106, Accesible = true });
                d.Rutas.Add(new RutaAutobus { Id = "X", Nombre = "Circular", Sentido = SentidoRecorrido.Circular, IdsParadas = new List<string> { "A", "B", "C" } });
                d.Conexiones.Add(C("A", "B", "X", 4));
                d.Conexiones.Add(C("B", "C", "X", 4));
                if (cerrada) d.Conexiones.Add(C("C", "A", "X", 4));
                return d;
            };

            Assert.False(ServicioDatosTransporte.Validar(circular(false)).EsValido);
            Assert.True(ServicioDatosTransporte.Validar(circular(true)).EsValido);

            var g = ServicioDatosTransporte.ConstruirGrafo(circular(true));
            var r = Recorrer(g, "C", "B");   // C -> A -> B por el cierre del círculo
            Assert.Equal(new[] { "C", "A", "B" }, r.Paradas.Select(p => p.Id).ToArray());
            Assert.Equal(0, r.Transbordos);
        }

        // ── Servicio completo ────────────────────────────────────────────────────

        [Fact]
        public void Servicio_DevuelveAlternativasOrdenadasYExplicadas()
        {
            var origen = DatosPrueba.CercaDe("P01");
            var destino = DatosPrueba.CercaDe("P05");
            var res = Servicio().BuscarMejoresRutas(origen, destino, Estrechas(Prioridad.MasRapida), 3);

            Assert.True(res.Exito, res.Mensaje);
            Assert.Equal(TipoError.Ninguno, res.Error);
            Assert.Equal(2, res.Recomendaciones.Count);   // R3 y R1; la combinada está dominada
            Assert.Equal(new[] { 1, 2 }, res.Recomendaciones.Select(r => r.Posicion).ToArray());
            Assert.True(res.Recomendaciones[0].Puntuacion >= res.Recomendaciones[1].Puntuacion);
            Assert.Equal("R3", res.Recomendaciones[0].Recorrido.FirmaDeRutas);
            Assert.All(res.Recomendaciones, r => Assert.False(string.IsNullOrWhiteSpace(r.Explicacion)));

            var primera = res.Recomendaciones[0];
            Assert.Contains(primera.Ventajas, v => v.Contains("más rápida"));
            Assert.Contains(res.Recomendaciones[1].Desventajas, d => d.Contains("Tarda 10 min más"));
            Assert.Contains(res.Recomendaciones[1].Ventajas, v => v.Contains("confiabilidad") && v.Contains("92%"));

            // Listo para dibujar en Leaflet: el trazo va del origen al destino reales
            var trazo = primera.Recorrido.Trazo;
            Assert.Same(origen, trazo.First());
            Assert.Same(destino, trazo.Last());
            Assert.Equal("P01", primera.Recorrido.ParadaInicial.Id);
        }

        [Fact]
        public void Servicio_LaPrioridadElegidaCambiaLaMejorRecomendacion()
        {
            var servicio = Servicio();
            var origen = DatosPrueba.CercaDe("P01");
            var destino = DatosPrueba.CercaDe("P05");

            var rapida = servicio.BuscarMejoresRutas(origen, destino, Estrechas(Prioridad.MasRapida), 2);
            var confiable = servicio.BuscarMejoresRutas(origen, destino, Estrechas(Prioridad.MasConfiable), 2);
            Assert.Equal("R3", rapida.Recomendaciones[0].Recorrido.FirmaDeRutas);      // 14 min
            Assert.Equal("R1", confiable.Recomendaciones[0].Recorrido.FirmaDeRutas);   // 0.92 contra 0.85
        }

        [Fact]
        public void Servicio_CaminataInicialYFinalSonEstimacionesMayoresQueLaLineaRecta()
        {
            var origen = DatosPrueba.CercaDe("P01");
            var destino = DatosPrueba.CercaDe("P05");
            var prefs = Estrechas();
            var r = Servicio().BuscarMejoresRutas(origen, destino, prefs, 1).Recomendaciones[0].Recorrido;

            double recta = ServicioUbicaciones.DistanciaLineaRectaM(origen, new Coordenada(28.6400, -106.0800));
            Assert.Equal(recta * prefs.FactorRodeoCaminata, r.AccesoM, 6);
            Assert.True(r.AccesoM > recta);
            Assert.Equal(r.AccesoM + r.EgresoM, r.DistanciaCaminataM, 6);
            Assert.Equal(r.TiempoAutobusMin + r.AccesoMin + r.EgresoMin, r.TiempoTotalMin, 6);
        }

        [Fact]
        public void Servicio_ConTransbordoCombinaVariasLineas()
        {
            var res = Servicio().BuscarMejoresRutas(DatosPrueba.CercaDe("P06"), DatosPrueba.CercaDe("P11"), Estrechas(), 5);
            Assert.True(res.Exito, res.Mensaje);
            var mejor = res.Recomendaciones[0].Recorrido;
            Assert.Equal("R2>R4", mejor.FirmaDeRutas);
            Assert.Equal(1, mejor.Transbordos);
            Assert.Null(mejor.Confiabilidad);   // R2 y R4 no tienen datos: no se inventa un valor
        }

        [Fact]
        public void Servicio_ConfiabilidadDesconocidaGeneraAdvertenciaYNoRompeElOrden()
        {
            var res = Servicio().BuscarMejoresRutas(DatosPrueba.CercaDe("P06"), DatosPrueba.CercaDe("P08"), Estrechas(Prioridad.MasConfiable), 3);
            Assert.True(res.Exito, res.Mensaje);
            Assert.Contains(res.Advertencias, a => a.Contains("confiabilidad"));
            Assert.All(res.Recomendaciones, r => Assert.Null(r.Recorrido.Confiabilidad));
            Assert.Contains(res.Recomendaciones[0].Desventajas, d => d.Contains("confiabilidad"));
        }

        [Fact]
        public void Servicio_UbicacionesInvalidas()
        {
            var s = Servicio();
            var ok = DatosPrueba.CercaDe("P01");
            Assert.Equal(TipoError.UbicacionInvalida, s.BuscarMejoresRutas(null, ok, null, 1).Error);
            Assert.Equal(TipoError.UbicacionInvalida, s.BuscarMejoresRutas(ok, null, null, 1).Error);
            Assert.Equal(TipoError.UbicacionInvalida, s.BuscarMejoresRutas(new Coordenada(95, 0), ok, null, 1).Error);
            Assert.Equal(TipoError.UbicacionInvalida, s.BuscarMejoresRutas(ok, new Coordenada(0, 200), null, 1).Error);
            Assert.Equal(TipoError.UbicacionInvalida, s.BuscarMejoresRutas(new Coordenada(double.NaN, double.NaN), ok, null, 1).Error);
            var fallo = s.BuscarMejoresRutas(null, ok, null, 1);
            Assert.False(fallo.Exito);
            Assert.Empty(fallo.Recomendaciones);
            Assert.False(string.IsNullOrWhiteSpace(fallo.Mensaje));
        }

        [Fact]
        public void Servicio_PreferenciasYCantidadInvalidas()
        {
            var s = Servicio();
            var o = DatosPrueba.CercaDe("P01");
            var d = DatosPrueba.CercaDe("P05");
            Assert.Equal(TipoError.PreferenciasInvalidas, s.BuscarMejoresRutas(o, d, new Preferencias { PesoTiempo = -2 }, 1).Error);
            Assert.Equal(TipoError.CantidadInvalida, s.BuscarMejoresRutas(o, d, null, 0).Error);
            Assert.Equal(TipoError.CantidadInvalida, s.BuscarMejoresRutas(o, d, null, -3).Error);
            Assert.True(s.BuscarMejoresRutas(o, d, null, 1).Exito);   // preferencias null = valores por defecto
        }

        [Fact]
        public void Servicio_SinParadasCercanas()
        {
            var s = Servicio();
            var lejos = new Coordenada(10, 10);
            Assert.Equal(TipoError.SinParadasCercanasAlOrigen, s.BuscarMejoresRutas(lejos, DatosPrueba.CercaDe("P01"), Estrechas(), 1).Error);
            Assert.Equal(TipoError.SinParadasCercanasAlDestino, s.BuscarMejoresRutas(DatosPrueba.CercaDe("P01"), lejos, Estrechas(), 1).Error);
        }

        [Fact]
        public void Servicio_DestinosInaccesibles()
        {
            var s = Servicio();
            // P12 existe pero no tiene ninguna ruta
            var aSinServicio = s.BuscarMejoresRutas(DatosPrueba.CercaDe("P01"), DatosPrueba.CercaDe("P12"), Estrechas(), 3);
            Assert.False(aSinServicio.Exito);
            Assert.Equal(TipoError.DestinoInaccesible, aSinServicio.Error);

            // P11 es el final de un ramal de una sola vía: no se puede salir de ahí
            var desdeCallejon = s.BuscarMejoresRutas(DatosPrueba.CercaDe("P11"), DatosPrueba.CercaDe("P01"), Estrechas(), 3);
            Assert.Equal(TipoError.DestinoInaccesible, desdeCallejon.Error);

            // Con el límite de transbordos en 0 no se llega de la colonia al ramal (hace falta cambiar de línea)
            var sinTransbordos = s.BuscarMejoresRutas(DatosPrueba.CercaDe("P06"), DatosPrueba.CercaDe("P11"), new Preferencias { RadioBusquedaMetros = 200, MaxTransbordos = 0 }, 3);
            Assert.Equal(TipoError.DestinoInaccesible, sinTransbordos.Error);
        }

        [Fact]
        public void Servicio_OrigenYDestinoJuntoALaMismaParada()
        {
            // Dos puntos distintos (~90 m) que solo tienen a P01 como parada cercana: no hay viaje en autobús
            var res = Servicio().BuscarMejoresRutas(DatosPrueba.CercaDe("P01", 0.0008), DatosPrueba.CercaDe("P01", -0.0008), Estrechas(), 3);
            Assert.False(res.Exito);
            Assert.Equal(TipoError.MismaParada, res.Error);

            // El mismo punto como origen y destino ni siquiera se busca
            var punto = DatosPrueba.CercaDe("P03");
            Assert.Equal(TipoError.OrigenIgualDestino, Servicio().BuscarMejoresRutas(punto, punto, Estrechas(), 3).Error);
        }

        [Fact]
        public void Servicio_SoloParadasAccesibles()
        {
            var s = Servicio();
            // Con radio corto la única parada junto a P04 no es accesible
            var sinParadas = s.BuscarMejoresRutas(DatosPrueba.CercaDe("P01"), DatosPrueba.CercaDe("P04"),
                new Preferencias { RadioBusquedaMetros = 200, SoloParadasAccesibles = true }, 3);
            Assert.Equal(TipoError.SinParadasCercanasAlDestino, sinParadas.Error);

            // Con radio mayor se baja en una parada accesible vecina (P03 o P05)
            var res = s.BuscarMejoresRutas(DatosPrueba.CercaDe("P01"), DatosPrueba.CercaDe("P04"),
                new Preferencias { RadioBusquedaMetros = 700, SoloParadasAccesibles = true }, 3);
            Assert.True(res.Exito, res.Mensaje);
            Assert.All(res.Recomendaciones, r => Assert.True(r.Recorrido.TodasParadasAccesibles));
            Assert.All(res.Recomendaciones, r => Assert.NotEqual("P04", r.Recorrido.ParadaFinal.Id));
        }

        [Fact]
        public void Servicio_NoDevuelveAlternativasDuplicadasNiDominadas()
        {
            var res = Servicio().BuscarMejoresRutas(DatosPrueba.CercaDe("P01", 0.0008, 0.002), DatosPrueba.CercaDe("P08"), new Preferencias(), 10);
            Assert.True(res.Exito, res.Mensaje);
            Assert.Equal(res.Recomendaciones.Count, res.Recomendaciones.Select(r => r.Recorrido.FirmaDeRutas).Distinct().Count());
            Assert.Equal(res.Recomendaciones.Count, res.Recomendaciones.Select(r => r.Recorrido.Firma).Distinct().Count());
            foreach (var a in res.Recomendaciones)
                foreach (var b in res.Recomendaciones)
                    if (!ReferenceEquals(a, b)) Assert.False(BuscadorMulticriterio.Domina(a.Recorrido, b.Recorrido));
        }

        [Fact]
        public void Servicio_CantidadPedidaMayorQueLasAlternativasAvisa()
        {
            var res = Servicio().BuscarMejoresRutas(DatosPrueba.CercaDe("P06"), DatosPrueba.CercaDe("P08"), Estrechas(), 5);
            Assert.True(res.Exito);
            Assert.Contains(res.Advertencias, a => a.Contains("Solo hay"));
            Assert.True(res.Recomendaciones.Count < 5);
        }

        [Fact]
        public void Servicio_ReutilizaElGrafoYLoReconstruyeSiCambiaLaCaminata()
        {
            var s = Servicio();
            var o = DatosPrueba.CercaDe("P06");
            var d = DatosPrueba.CercaDe("P05");
            var lenta = Estrechas(); lenta.VelocidadCaminataKmh = 2;
            var normal = Estrechas();

            var a = s.BuscarMejoresRutas(o, d, lenta, 3);
            var b = s.BuscarMejoresRutas(o, d, normal, 3);
            var c = s.BuscarMejoresRutas(o, d, lenta, 3);
            Assert.True(a.Exito && b.Exito && c.Exito);
            Assert.Equal(a.Recomendaciones[0].Recorrido.TiempoTotalMin, c.Recomendaciones[0].Recorrido.TiempoTotalMin, 9);
            // Caminar más despacio nunca acorta el viaje
            Assert.True(a.Recomendaciones.Min(r => r.Recorrido.TiempoTotalMin) >= b.Recomendaciones.Min(r => r.Recorrido.TiempoTotalMin));
        }
    }
}
