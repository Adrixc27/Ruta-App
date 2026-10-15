using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruta_App.DB
{
    /// <summary>
    /// Trae las rutas de la API (tabla routes) y los nombres de los camiones (tabla bus) y arma las Ruta de la app.
    /// </summary>
    internal class RutaRepositorio
    {
        /// <summary>Devuelve las rutas reales, o null si la API no responde o no hay datos.</summary>
        public async Task<List<Ruta>> CargarAsync()
        {
            try
            {
                var api = new ApiService();
                var buses = await api.ObtenerBusesAsync() ?? new List<BusApi>();
                var rutas = await api.ObtenerRutasAsync() ?? new List<RutaApi>();

                var resultado = new List<Ruta>();
                foreach (var fila in rutas)
                {
                    var bus = buses.FirstOrDefault(b => b.id == fila.idBus);
                    resultado.Add(Armar(fila, bus));
                }
                return resultado.Count > 0 ? resultado : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RutaRepositorio: " + ex.Message);
                return null;
            }
        }

        private static Ruta Armar(RutaApi fila, BusApi bus)
        {
            string nombre = string.IsNullOrWhiteSpace(fila.name) ? (bus != null ? bus.name : "Ruta " + fila.id) : fila.name.Trim();

            var r = new Ruta
            {
                Numero = fila.id,
                Nombre = nombre,
                Lineas = bus != null ? bus.name : nombre,
                Tarifa = fila.price,
                Minutos = (int)Math.Round(fila.travelMinutes),
                Descripcion = fila.description ?? "",
                OrigenNombre = nombre,
                DestinoNombre = "Centro"
            };

            // La base de datos aún no guarda coordenadas: se separan alrededor del centro para que el mapa funcione
            double desp = 0.004 * (fila.id % 10);
            r.LatO = Geo.CentroLat + 0.03 + desp; r.LngO = Geo.CentroLng - 0.03 - desp;
            r.LatD = Geo.CentroLat; r.LngD = Geo.CentroLng;
            return r;
        }
    }
}
