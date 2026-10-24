using System;
using System.Collections.Generic;

namespace Ruta_App
{
    /// <summary>
    /// Red FICTICIA para probar el motor. Los nombres, tiempos y confiabilidades son inventados y no representan ninguna ruta real;
    /// las coordenadas solo ubican la red en un lugar plausible para verla en el mapa.
    ///
    ///   P06
    ///    |  R2 (norte-sur, ambos sentidos, confiabilidad desconocida)
    ///   P01 --R1-- P02 --R1-- P03 --R1-- P04(no accesible) --R1-- P05      R1: este-oeste, ambos sentidos, 0.92
    ///     \_____________R3 exprés (solo ida)__P09__________________/        R3: P01 -> P09 -> P05, solo ida, 0.85
    ///                       |  P09 está a ~70 m de P03 (transbordo caminando)
    ///                      P07
    ///                       |
    ///                      P08 --R4 (solo ida, desconocida)--> P10(no accesible) --> P11 (sin salida)
    ///
    ///   P12: parada sin ninguna ruta (destino inaccesible).
    /// </summary>
    public static class DatosPrueba
    {
        public static DatosTransporte Crear()
        {
            var paradas = new List<Parada>
            {
                P("P01", "Terminal Norte (prueba)",   28.6400, -106.0800, true),
                P("P02", "Mercado Viejo (prueba)",    28.6400, -106.0750, true),
                P("P03", "Plaza Central (prueba)",    28.6400, -106.0700, true),
                P("P04", "Cruce Este (prueba)",       28.6400, -106.0650, false),
                P("P05", "Estación Este (prueba)",    28.6400, -106.0600, true),
                P("P06", "Colonia Alta (prueba)",     28.6460, -106.0700, true),
                P("P07", "Parque Sur (prueba)",       28.6340, -106.0700, true),
                P("P08", "Centro Comercial (prueba)", 28.6280, -106.0700, true),
                P("P09", "Paso Exprés (prueba)",      28.6406, -106.0702, true),
                P("P10", "Fábrica Sur (prueba)",      28.6280, -106.0640, false),
                P("P11", "Final del Ramal (prueba)",  28.6280, -106.0580, true),
                P("P12", "Parada sin servicio (prueba)", 28.6100, -106.0300, true)
            };

            var rutas = new List<RutaAutobus>
            {
                new RutaAutobus { Id = "R1", Nombre = "Línea 1", Sentido = SentidoRecorrido.Bidireccional,  Confiabilidad = 0.92, IdsParadas = new List<string> { "P01", "P02", "P03", "P04", "P05" } },
                new RutaAutobus { Id = "R2", Nombre = "Línea 2", Sentido = SentidoRecorrido.Bidireccional,  Confiabilidad = null, IdsParadas = new List<string> { "P06", "P03", "P07", "P08" } },
                new RutaAutobus { Id = "R3", Nombre = "Línea 3 Exprés", Sentido = SentidoRecorrido.Unidireccional, Confiabilidad = 0.85, IdsParadas = new List<string> { "P01", "P09", "P05" } },
                new RutaAutobus { Id = "R4", Nombre = "Línea 4 Ramal", Sentido = SentidoRecorrido.Unidireccional, Confiabilidad = null, IdsParadas = new List<string> { "P08", "P10", "P11" } }
            };

            var datos = new DatosTransporte { Paradas = paradas, Rutas = rutas };
            foreach (var r in rutas)
            {
                var tiempos = TiemposPorTramo(r.Id);
                for (int i = 0; i < r.IdsParadas.Count - 1; i++)
                {
                    datos.Conexiones.Add(Tramo(paradas, r.Id, r.IdsParadas[i], r.IdsParadas[i + 1], tiempos[i]));
                    if (r.Sentido == SentidoRecorrido.Bidireccional)
                        datos.Conexiones.Add(Tramo(paradas, r.Id, r.IdsParadas[i + 1], r.IdsParadas[i], tiempos[i]));
                }
            }
            return datos;
        }

        /// <summary>Punto a unos metros de una parada de prueba (desplazamiento en grados; 0.001° ≈ 110 m de latitud).</summary>
        public static Coordenada CercaDe(string idParada, double desplazamientoLatitud = 0.0008, double desplazamientoLongitud = 0)
        {
            foreach (var p in Crear().Paradas)
                if (p.Id == idParada) return new Coordenada(p.Latitud + desplazamientoLatitud, p.Longitud + desplazamientoLongitud);
            throw new ArgumentException("No existe la parada de prueba '" + idParada + "'.");
        }

        // Minutos inventados de cada tramo, en el orden de IdsParadas
        private static double[] TiemposPorTramo(string idRuta)
        {
            switch (idRuta)
            {
                case "R1": return new double[] { 6, 6, 6, 6 };
                case "R2": return new double[] { 7, 7, 7 };
                case "R3": return new double[] { 7, 7 };
                case "R4": return new double[] { 8, 8 };
                default: throw new ArgumentException("Ruta de prueba desconocida: " + idRuta);
            }
        }

        private static Parada P(string id, string nombre, double lat, double lng, bool accesible)
        {
            return new Parada { Id = id, Nombre = nombre, Latitud = lat, Longitud = lng, Accesible = accesible };
        }

        // La distancia se calcula en línea recta entre las coordenadas de prueba (no es un dato de recorrido real).
        private static Conexion Tramo(List<Parada> paradas, string idRuta, string de, string a, double minutos)
        {
            Parada pd = paradas.Find(p => p.Id == de), pa = paradas.Find(p => p.Id == a);
            return new Conexion
            {
                IdOrigen = de,
                IdDestino = a,
                IdRuta = idRuta,
                TiempoMin = minutos,
                DistanciaM = Math.Round(ServicioUbicaciones.DistanciaLineaRectaM(pd.Latitud, pd.Longitud, pa.Latitud, pa.Longitud))
            };
        }
    }
}
