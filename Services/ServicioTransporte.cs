using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ruta_App
{
    /// <summary>
    /// Punto de entrada del motor de rutas: valida la consulta, encuentra paradas cercanas, busca alternativas
    /// (Dijkstra + A* + multicriterio), quita duplicados y dominadas, puntúa según las preferencias y explica el resultado.
    /// Es independiente de WPF; el ResultadoBusqueda se puede mostrar en la interfaz o serializar para Leaflet.
    /// </summary>
    public class ServicioTransporte
    {
        private const double DistanciaMinimaViajeM = 50;
        private readonly DatosTransporte _datos;
        private readonly ServicioUbicaciones _ubicaciones;
        private readonly object _candado = new object();
        private string _claveGrafo;
        private GrafoTransporte _grafo;

        /// <summary>Advertencias de los datos cargados (paradas sin servicio, rutas sin confiabilidad...).</summary>
        public IReadOnlyList<string> AdvertenciasDatos { get; private set; }

        /// <summary>Lanza InvalidDataException si los datos no son válidos.</summary>
        public ServicioTransporte(DatosTransporte datos)
        {
            ServicioDatosTransporte.ConstruirGrafo(datos);   // valida (lanza si hay errores)
            _datos = datos;
            _ubicaciones = new ServicioUbicaciones(datos.Paradas);
            AdvertenciasDatos = ServicioDatosTransporte.Validar(datos).Advertencias;
        }

        public ResultadoBusqueda BuscarMejoresRutas(Coordenada origen, Coordenada destino, Preferencias preferencias, int cantidadResultados)
        {
            if (origen == null || !origen.EsValida)
                return ResultadoBusqueda.Fallo(TipoError.UbicacionInvalida, "El origen no es una ubicación válida (latitud entre -90 y 90, longitud entre -180 y 180).");
            if (destino == null || !destino.EsValida)
                return ResultadoBusqueda.Fallo(TipoError.UbicacionInvalida, "El destino no es una ubicación válida (latitud entre -90 y 90, longitud entre -180 y 180).");
            if (preferencias == null) preferencias = new Preferencias();
            var problemas = preferencias.Validar();
            if (problemas.Count > 0)
                return ResultadoBusqueda.Fallo(TipoError.PreferenciasInvalidas, "Preferencias inválidas: " + string.Join(" ", problemas));
            if (cantidadResultados < 1)
                return ResultadoBusqueda.Fallo(TipoError.CantidadInvalida, "La cantidad de resultados debe ser al menos 1.");

            if (ServicioUbicaciones.DistanciaLineaRectaM(origen, destino) < DistanciaMinimaViajeM)
                return ResultadoBusqueda.Fallo(TipoError.OrigenIgualDestino, "El origen y el destino están a menos de " + DistanciaMinimaViajeM.ToString("0", CultureInfo.InvariantCulture) + " m: no hace falta autobús.");

            var grafo = ObtenerGrafo(preferencias);
            var cercanasOrigen = _ubicaciones.BuscarParadasCercanas(origen, preferencias, preferencias.MaxParadasCandidatas);
            if (cercanasOrigen.Count == 0)
                return ResultadoBusqueda.Fallo(TipoError.SinParadasCercanasAlOrigen, MensajeSinParadas("origen", preferencias));
            var cercanasDestino = _ubicaciones.BuscarParadasCercanas(destino, preferencias, preferencias.MaxParadasCandidatas);
            if (cercanasDestino.Count == 0)
                return ResultadoBusqueda.Fallo(TipoError.SinParadasCercanasAlDestino, MensajeSinParadas("destino", preferencias));

            var buscador = new BuscadorMulticriterio(grafo);
            var candidatos = new List<Recorrido>();
            bool seBuscoAlgunPar = false;
            foreach (var po in cercanasOrigen)
                foreach (var pd in cercanasDestino)
                {
                    if (po.Parada.Id == pd.Parada.Id) continue;   // sería solo caminar; no hay viaje en autobús
                    seBuscoAlgunPar = true;
                    foreach (var r in buscador.Buscar(po.Parada.Id, pd.Parada.Id, preferencias))
                    {
                        r.AsignarAccesos(origen, po.DistanciaCaminandoM, po.TiempoCaminandoMin, destino, pd.DistanciaCaminandoM, pd.TiempoCaminandoMin);
                        candidatos.Add(r);
                    }
                }

            if (candidatos.Count == 0)
            {
                if (!seBuscoAlgunPar)
                    return ResultadoBusqueda.Fallo(TipoError.MismaParada, "El origen y el destino quedan junto a la misma parada: no hace falta autobús, conviene ir caminando.");
                return ResultadoBusqueda.Fallo(TipoError.DestinoInaccesible,
                    "No hay forma de llegar en autobús desde las paradas cercanas al origen hasta las cercanas al destino"
                    + (preferencias.SoloParadasAccesibles ? " usando solo paradas accesibles" : "")
                    + " dentro de " + preferencias.MaxTransbordos.ToString(CultureInfo.InvariantCulture) + " transbordos como máximo.");
            }

            // Mismas alternativas encontradas por varios caminos -> una sola; luego se descartan las dominadas (Pareto)
            var frente = BuscadorMulticriterio.FiltrarNoDominados(ClasificadorRutas.EliminarDuplicados(candidatos));

            var advertencias = new List<string>();
            var evaluadas = EvaluadorRutas.Evaluar(frente, preferencias, advertencias);

            // Misma secuencia de líneas con otra parada de subida/bajada: se deja la mejor y se vuelve a puntuar el conjunto final
            var unicas = ClasificadorRutas.EliminarDuplicados(evaluadas, true);
            if (unicas.Count != evaluadas.Count)
            {
                advertencias.Clear();
                evaluadas = EvaluadorRutas.Evaluar(unicas.Select(u => u.Recorrido).ToList(), preferencias, advertencias);
            }

            var mejores = ClasificadorRutas.SeleccionarMejores(evaluadas, cantidadResultados);
            EvaluadorRutas.Explicar(mejores);

            if (mejores.Count < cantidadResultados)
                advertencias.Add("Solo hay " + mejores.Count.ToString(CultureInfo.InvariantCulture) + " alternativa(s) distinta(s) y no dominada(s) con estas preferencias.");

            return new ResultadoBusqueda { Recomendaciones = mejores, Advertencias = advertencias, Mensaje = "Se encontraron " + mejores.Count.ToString(CultureInfo.InvariantCulture) + " alternativa(s)." };
        }

        // El grafo depende de los parámetros de caminata de las preferencias (los transbordos a pie se calculan con ellos).
        private GrafoTransporte ObtenerGrafo(Preferencias p)
        {
            string clave = string.Join("|", new[] { p.FactorRodeoCaminata, p.VelocidadCaminataKmh, p.RadioTransbordoMetros }
                .Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
            lock (_candado)
            {
                if (_grafo != null && _claveGrafo == clave) return _grafo;

                var grafo = new GrafoTransporte();
                foreach (var parada in _datos.Paradas) grafo.AgregarParada(parada);
                foreach (var ruta in _datos.Rutas) grafo.AgregarRuta(ruta);
                foreach (var c in _datos.Conexiones) grafo.AgregarConexion(c);

                // Transbordos caminando entre paradas cercanas que tienen servicio
                var conServicio = new HashSet<string>(_datos.Rutas.SelectMany(r => r.IdsParadas));
                foreach (var par in _ubicaciones.ParesCercanos(p.RadioTransbordoMetros, p))
                {
                    if (!conServicio.Contains(par.Key.Id) || !conServicio.Contains(par.Value.Parada.Id)) continue;
                    grafo.AgregarConexion(new Conexion
                    {
                        IdOrigen = par.Key.Id,
                        IdDestino = par.Value.Parada.Id,
                        IdRuta = null,
                        TiempoMin = par.Value.TiempoCaminandoMin,
                        DistanciaM = par.Value.DistanciaCaminandoM
                    });
                }

                _grafo = grafo;
                _claveGrafo = clave;
                return grafo;
            }
        }

        private static string MensajeSinParadas(string lugar, Preferencias p)
        {
            return "No hay paradas" + (p.SoloParadasAccesibles ? " accesibles" : "") + " a menos de "
                + p.RadioBusquedaMetros.ToString("0", CultureInfo.InvariantCulture) + " m del " + lugar + ". Prueba con un radio de búsqueda mayor.";
        }
    }
}
