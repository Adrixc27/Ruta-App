using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ruta_App
{
    public partial class CompararView : UserControl, INavegable
    {
        private List<Ruta> _opciones = new List<Ruta>();
        private Ruta _seleccion;

        public CompararView()
        {
            InitializeComponent();
        }

        /// <summary>Cada vez que llegas desde Planificar se recalcula la comparación.</summary>
        public void AlNavegar(object parametro)
        {
            TxtOrigenResumen.Text = Estado.Origen;
            TxtDestinoResumen.Text = Estado.Destino;
            TxtPrioridad.Text = "Prioridad: " + Estado.Prioridad;

            string icono = "IcoZap";
            if (Estado.Prioridad == "Económico") icono = "IcoWallet";
            else if (Estado.Prioridad == "Cómodo") icono = "IcoSofa";
            IcoPrioridad.Data = (System.Windows.Media.Geometry)FindResource(icono);

            // Demo: siempre se comparan estas 3 rutas. Aquí iría tu lógica real de búsqueda.
            _opciones = new List<Ruta>
            {
                Estado.BuscarRuta(6), Estado.BuscarRuta(7), Estado.BuscarRuta(3)
            };
            _opciones.RemoveAll(r => r == null);

            ListaOpciones.ItemsSource = null;
            ListaOpciones.ItemsSource = _opciones;
            TxtConteo.Text = _opciones.Count + " rutas encontradas · elige una para continuar";

            if (_opciones.Count > 0) Seleccionar(_opciones[0]);
        }

        private void Seleccionar(Ruta r)
        {
            foreach (var x in Estado.Rutas) x.Seleccionada = (x == r);
            _seleccion = r;

            TxtComenzar.Text = "Comenzar viaje (Ruta " + r.Numero + ")";
            Mapa.DibujarRuta(r);
            Mapa.BannerViaje(r.Nombre, "Vista previa en el mapa · " + r.MinutosTexto);
            _ = Mapa.CargarParadasAsync(r.LatO, r.LngO, r.LatD, r.LngD);
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
                "¡Buen viaje!\n\nSeguirás la " + _seleccion.Nombre + " (" + _seleccion.MinutosTexto + ", " +
                _seleccion.TarifaTexto + ").",
                "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);

            Estado.AgregarReciente(_seleccion);
            Estado.Destino = "";
            var nav = Estado.Navegar;
            if (nav != null) nav("planificar", null);
        }
    }
}
