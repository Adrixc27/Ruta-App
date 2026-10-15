using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ruta_App
{
    public partial class CompararView : UserControl, INavegable
    {
        private List<Ruta> _opciones = new List<Ruta>();
        private Ruta _seleccion;
        private int _version;   // para descartar cálculos viejos si el usuario cambia la búsqueda

        public CompararView()
        {
            InitializeComponent();
        }

        /// <summary>Cada vez que llegas desde Planificar se recalcula la comparación.</summary>
        public async void AlNavegar(object parametro)
        {
            int version = ++_version;

            TxtOrigenResumen.Text = Estado.Origen;
            TxtDestinoResumen.Text = Estado.Destino;
            if (Estado.Escalas.Count > 0)
            {
                TxtEscalasResumen.Text = "Pasando por: " + string.Join(" → ", Estado.Escalas.Select(s => s.Nombre));
                TxtEscalasResumen.Visibility = Visibility.Visible;
            }
            else TxtEscalasResumen.Visibility = Visibility.Collapsed;

            TxtConteo.Text = "Calculando las mejores rutas…";
            await Estado.PrepararTrazosAsync();   // recorridos de las rutas (por calles)

            if (Estado.HayCoordenadas)
                _opciones = await CalcularViajeAsync();
            else
                _opciones = new List<Ruta>(Estado.Rutas);

            if (version != _version) return;   // llegó otra búsqueda mientras se calculaba

            ListaOpciones.ItemsSource = null;
            ListaOpciones.ItemsSource = _opciones;
            TxtConteo.Text = _opciones.Count == 0
                ? "No hay rutas de camión a menos de 1 km de tus puntos, o la distancia es demasiado grande"
                : _opciones.Count + (_opciones.Count == 1 ? " opción encontrada" : " opciones encontradas") + " · elige una para continuar";

            if (_opciones.Count > 0) Seleccionar(_opciones[0]);
            else { _seleccion = null; Mapa.LimpiarRuta(); Mapa.OcultarBanner(); }
        }

        /// <summary>
        /// Calcula cómo ir de A a B, de B a C, etc. (origen → paradas intermedias → destino final).
        /// La opción N del viaje junta la opción N de cada tramo.
        /// </summary>
        private async Task<List<Ruta>> CalcularViajeAsync()
        {
            var puntos = new List<double[]> { new[] { Estado.OrigenLat, Estado.OrigenLng } };
            foreach (var e in Estado.Escalas) puntos.Add(new[] { e.Lat, e.Lng });
            puntos.Add(new[] { Estado.DestinoLat, Estado.DestinoLng });

            var tareas = new List<Task<List<Ruta>>>();
            for (int i = 0; i + 1 < puntos.Count; i++)
                tareas.Add(Planificador.GenerarAsync(Estado.Rutas, puntos[i][0], puntos[i][1], puntos[i + 1][0], puntos[i + 1][1]));
            var tramos = await Task.WhenAll(tareas);

            if (tramos.Any(t => t.Count == 0)) return new List<Ruta>();
            if (tramos.Length == 1) return tramos[0];

            var resultado = new List<Ruta>();
            int cuantas = tramos.Max(t => t.Count);
            for (int n = 0; n < cuantas; n++)
            {
                var partes = tramos.Select(t => t[n < t.Count ? n : 0]).ToList();
                resultado.Add(Unir(partes));
            }
            Planificador.Etiquetar(resultado);
            return resultado;
        }

        private static Ruta Unir(List<Ruta> partes)
        {
            var tramos = new List<Tramo>();
            foreach (var p in partes) tramos.AddRange(p.Tramos);

            return new Ruta
            {
                Numero = partes[0].Numero,
                Nombre = "Viaje en " + partes.Count + " tramos",
                ModoTexto = "Autobús",
                Lineas = string.Join(" | ", partes.Select(p => p.Lineas)),
                Tarifa = partes.Sum(p => p.Tarifa),
                Minutos = partes.Sum(p => p.Minutos),
                Transbordos = partes.Sum(p => p.Transbordos),
                CaminataMetros = partes.Sum(p => p.CaminataMetros),
                Guardada = false,
                EsViaje = true,
                Resumen = string.Join("  |  ", partes.Select((p, i) => "Tramo " + (i + 1) + ": " + p.Minutos + " min")),
                Tramos = tramos,
                Escalas = Estado.Escalas,
                LatO = tramos[0].Puntos[0][0], LngO = tramos[0].Puntos[0][1]
            };
        }

        private void Seleccionar(Ruta r)
        {
            foreach (var x in _opciones) x.Seleccionada = (x == r);
            foreach (var x in Estado.Rutas) x.Seleccionada = false;
            _seleccion = r;

            TxtComenzar.Text = r.EsViaje ? "Comenzar viaje" : "Comenzar viaje (Ruta " + r.Numero + ")";
            if (r.EsViaje && r.Escalas == null) r.Escalas = Estado.Escalas;
            Mapa.DibujarRuta(r);
            Mapa.BannerViaje(r.Nombre, r.EsViaje ? r.MinutosTexto + " · " + r.TarifaTexto : "Vista previa en el mapa · " + r.MinutosTexto);
            if (!r.EsViaje) _ = Mapa.CargarParadasAsync(r.LatO, r.LngO, r.LatD, r.LngD);
        }

        private void Opcion_Click(object sender, MouseButtonEventArgs e)
        {
            var r = ((FrameworkElement)sender).DataContext as Ruta;
            if (r != null) Seleccionar(r);
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var nav = Estado.Navegar;
            if (nav != null) nav("planificar", null);
        }

        private void BtnComenzar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccion == null) return;

            MessageBox.Show(
                "¡Buen viaje!\n\nSeguirás: " + _seleccion.Nombre + " (" + _seleccion.MinutosTexto + ", " +
                _seleccion.TarifaTexto + ").",
                "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);

            Estado.AgregarReciente(_seleccion);

            // El viaje ya empezó: se olvida la búsqueda (destino, paradas y marcadores) y se vacía el mapa
            Estado.NuevaBusqueda();
            _version++;
            _opciones = new List<Ruta>();
            _seleccion = null;
            ListaOpciones.ItemsSource = null;
            Mapa.LimpiarRuta();
            Mapa.OcultarBanner();
            Mapa.OcultarTarjeta();

            var nav = Estado.Navegar;
            if (nav != null) nav("planificar", null);
        }
    }
}
