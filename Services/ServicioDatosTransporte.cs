using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ruta_App
{
    /// <summary>Conjunto de datos de transporte tal como llega de la fuente (JSON, base de datos, GTFS convertido, etc.).</summary>
    public class DatosTransporte
    {
        public List<Parada> Paradas { get; set; } = new List<Parada>();
        public List<RutaAutobus> Rutas { get; set; } = new List<RutaAutobus>();
        /// <summary>Tramos de autobús dirigidos con su tiempo y distancia. Se espera uno por cada par de paradas consecutivas según el sentido de la ruta.</summary>
        public List<Conexion> Conexiones { get; set; } = new List<Conexion>();
    }

    public class ResultadoValidacion
    {
        public List<string> Errores { get; } = new List<string>();
        public List<string> Advertencias { get; } = new List<string>();
        public bool EsValido { get { return Errores.Count == 0; } }
    }

    /// <summary>Carga, valida y convierte datos de transporte en un GrafoTransporte.</summary>
    public static class ServicioDatosTransporte
    {
        private static readonly JsonSerializerOptions OpcionesJson = CrearOpcionesJson();

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            var o = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
            o.Converters.Add(new JsonStringEnumConverter());   // SentidoRecorrido como texto, más fácil de editar a mano
            return o;
        }

        /// <summary>
        /// Revisa los datos sin modificarlos. Errores = el grafo no se puede construir con seguridad;
        /// Advertencias = datos raros pero utilizables (p. ej. paradas sin servicio).
        /// </summary>
        public static ResultadoValidacion Validar(DatosTransporte datos)
        {
            var v = new ResultadoValidacion();
            if (datos == null) { v.Errores.Add("No se recibieron datos de transporte."); return v; }
            if (datos.Paradas == null || datos.Rutas == null || datos.Conexiones == null)
            {
                v.Errores.Add("Las listas de paradas, rutas y conexiones no pueden ser nulas.");
                return v;
            }

            // Paradas
            var paradas = new Dictionary<string, Parada>();
            foreach (var p in datos.Paradas)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.Id)) { v.Errores.Add("Hay una parada sin identificador."); continue; }
                if (paradas.ContainsKey(p.Id)) { v.Errores.Add("La parada '" + p.Id + "' está repetida."); continue; }
                paradas.Add(p.Id, p);
                if (string.IsNullOrWhiteSpace(p.Nombre)) v.Advertencias.Add("La parada '" + p.Id + "' no tiene nombre.");
                if (!p.Posicion.EsValida) v.Errores.Add("La parada '" + p.Id + "' tiene coordenadas inválidas (" + p.Latitud + ", " + p.Longitud + ").");
            }

            // Rutas
            var rutas = new Dictionary<string, RutaAutobus>();
            foreach (var r in datos.Rutas)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.Id)) { v.Errores.Add("Hay una ruta sin identificador."); continue; }
                if (rutas.ContainsKey(r.Id)) { v.Errores.Add("La ruta '" + r.Id + "' está repetida."); continue; }
                rutas.Add(r.Id, r);

                if (r.IdsParadas == null || r.IdsParadas.Count < 2) { v.Errores.Add("La ruta '" + r.Id + "' necesita al menos dos paradas."); continue; }
                foreach (var id in r.IdsParadas.Where(id => id == null || !paradas.ContainsKey(id)))
                    v.Errores.Add("La ruta '" + r.Id + "' usa la parada '" + id + "' que no existe.");
                foreach (var g in r.IdsParadas.Where(id => id != null).GroupBy(id => id).Where(g => g.Count() > 1))
                    v.Errores.Add("La ruta '" + r.Id + "' repite la parada '" + g.Key + "' (no se permiten ciclos dentro de una ruta).");
                if (r.Confiabilidad.HasValue && (double.IsNaN(r.Confiabilidad.Value) || r.Confiabilidad.Value < 0 || r.Confiabilidad.Value > 1))
                    v.Errores.Add("La confiabilidad de la ruta '" + r.Id + "' debe estar entre 0 y 1 (o ser null si se desconoce).");
            }

            // Conexiones
            var existentes = new HashSet<string>();
            foreach (var c in datos.Conexiones)
            {
                if (c == null) { v.Errores.Add("Hay una conexión nula."); continue; }
                string nombre = c.ToString();
                if (c.IdOrigen == null || !paradas.ContainsKey(c.IdOrigen)) { v.Errores.Add("La conexión " + nombre + " parte de una parada que no existe."); continue; }
                if (c.IdDestino == null || !paradas.ContainsKey(c.IdDestino)) { v.Errores.Add("La conexión " + nombre + " llega a una parada que no existe."); continue; }
                if (c.IdOrigen == c.IdDestino) { v.Errores.Add("La conexión " + nombre + " va de una parada a sí misma."); continue; }
                if (c.IdRuta == null) { v.Errores.Add("La conexión " + nombre + " no indica ruta (las caminatas se generan automáticamente)."); continue; }
                if (!rutas.ContainsKey(c.IdRuta)) { v.Errores.Add("La conexión " + nombre + " usa una ruta que no existe."); continue; }
                if (double.IsNaN(c.TiempoMin) || double.IsInfinity(c.TiempoMin) || c.TiempoMin <= 0)
                    v.Errores.Add("La conexión " + nombre + " debe tener un tiempo mayor que cero.");
                if (double.IsNaN(c.DistanciaM) || double.IsInfinity(c.DistanciaM) || c.DistanciaM < 0)
                    v.Errores.Add("La conexión " + nombre + " tiene una distancia inválida.");
                if (!existentes.Add(c.IdOrigen + ">" + c.IdDestino + "@" + c.IdRuta))
                    v.Errores.Add("La conexión " + nombre + " está repetida.");
            }

            // Coherencia entre rutas y conexiones (conexiones inexistentes o contrarias al sentido de la ruta)
            foreach (var r in rutas.Values)
            {
                if (r.IdsParadas == null || r.IdsParadas.Count < 2 || r.IdsParadas.Any(id => id == null || !paradas.ContainsKey(id))) continue;
                var esperadas = TramosEsperados(r);
                foreach (var t in esperadas)
                    if (!existentes.Contains(t)) v.Errores.Add("Falta la conexión " + Descripcion(t) + " que exige el recorrido de la ruta '" + r.Id + "'.");
                foreach (var c in datos.Conexiones.Where(c => c != null && c.IdRuta == r.Id))
                    if (!esperadas.Contains(c.IdOrigen + ">" + c.IdDestino + "@" + r.Id))
                        v.Errores.Add("La conexión " + c + " no corresponde al recorrido " + r.Sentido + " de la ruta '" + r.Id + "'.");
            }

            // Paradas sin servicio: no es un error, pero conviene avisar
            var usadas = new HashSet<string>(datos.Rutas.Where(r => r != null && r.IdsParadas != null).SelectMany(r => r.IdsParadas).Where(id => id != null));
            foreach (var p in paradas.Values.Where(p => !usadas.Contains(p.Id)))
                v.Advertencias.Add("La parada '" + p.Id + "' no pertenece a ninguna ruta (no se podrá llegar a ella en autobús).");
            foreach (var r in rutas.Values.Where(r => !r.Confiabilidad.HasValue))
                v.Advertencias.Add("La ruta '" + r.Id + "' no tiene datos de confiabilidad.");

            return v;
        }

        /// <summary>Valida y construye el grafo de autobuses. Lanza InvalidDataException con todos los errores si los datos no son válidos.</summary>
        public static GrafoTransporte ConstruirGrafo(DatosTransporte datos)
        {
            var validacion = Validar(datos);
            if (!validacion.EsValido)
                throw new InvalidDataException("Datos de transporte inválidos:" + Environment.NewLine + string.Join(Environment.NewLine, validacion.Errores));

            var grafo = new GrafoTransporte();
            foreach (var p in datos.Paradas) grafo.AgregarParada(p);
            foreach (var r in datos.Rutas) grafo.AgregarRuta(r);
            foreach (var c in datos.Conexiones) grafo.AgregarConexion(c);
            return grafo;
        }

        public static DatosTransporte DesdeJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("El JSON de datos de transporte está vacío.");
            try
            {
                var datos = JsonSerializer.Deserialize<DatosTransporte>(json, OpcionesJson);
                if (datos == null) throw new InvalidDataException("El JSON no contiene datos de transporte.");
                return datos;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("El JSON de datos de transporte no tiene el formato esperado: " + ex.Message, ex);
            }
        }

        public static DatosTransporte DesdeArchivo(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo)) throw new ArgumentException("Falta la ruta del archivo.");
            if (!File.Exists(rutaArchivo)) throw new FileNotFoundException("No se encontró el archivo de datos de transporte.", rutaArchivo);
            return DesdeJson(File.ReadAllText(rutaArchivo));
        }

        public static string AJson(DatosTransporte datos)
        {
            return JsonSerializer.Serialize(datos, OpcionesJson);
        }

        // Tramos "origen>destino@ruta" que debe tener una ruta según su sentido
        private static HashSet<string> TramosEsperados(RutaAutobus r)
        {
            var t = new HashSet<string>();
            var ids = r.IdsParadas;
            for (int i = 0; i < ids.Count - 1; i++)
            {
                t.Add(ids[i] + ">" + ids[i + 1] + "@" + r.Id);
                if (r.Sentido == SentidoRecorrido.Bidireccional) t.Add(ids[i + 1] + ">" + ids[i] + "@" + r.Id);
            }
            if (r.Sentido == SentidoRecorrido.Circular && ids.Count >= 2) t.Add(ids[ids.Count - 1] + ">" + ids[0] + "@" + r.Id);
            return t;
        }

        private static string Descripcion(string tramo)
        {
            int arroba = tramo.LastIndexOf('@');
            return tramo.Substring(0, arroba).Replace(">", " -> ") + " [" + tramo.Substring(arroba + 1) + "]";
        }
    }
}
