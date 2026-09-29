using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ruta_App
{
    public partial class RutasView : UserControl, INavegable
    {
        private bool _listo;              // evita que los eventos Checked se disparen antes de terminar de construir
        private bool _soloGuardadas;
        private string _modo = "Todas";
        private Ruta _seleccion;

        public RutasView()
        {
            InitializeComponent();

            Mapa.AccionQuitar += QuitarSeleccion;
            Mapa.AccionPlanificar += PlanificarConRuta;
            Mapa.AccionGuardar += GuardarSeleccion;
            Estado.GuardadasCambiaron += Refrescar;

            _listo = true;
            Refrescar();

            // Como en el diseño de Figma, arranca mostrando la Ruta 6
            var inicial = Estado.BuscarRuta(6);
            if (inicial != null) Seleccionar(inicial);
        }

        public void AlNavegar(object parametro)
        {
            var ruta = parametro as Ruta;
            if (ruta != null)
            {
                // Llegamos desde otra pantalla pidiendo ver una ruta concreta
                _soloGuardadas = false;
                _modo = "Todas";
                TabTodas.IsChecked = true;
                ChipTodas.IsChecked = true;
                Buscar.Text = "";
                Refrescar();
                Seleccionar(ruta);
            }
            else if (_seleccion != null)
            {
                Seleccionar(_seleccion); // re-sincroniza la selección y el mapa
            }
        }

        // ── Lista ─────────────────────────────────────────────────────────────
        private void Refrescar()
        {
            if (!_listo) return;

            var todas = Estado.Rutas;
            TxtCountTodas.Text = todas.Count.ToString();
            TxtCountGuardadas.Text = Estado.TotalGuardadas.ToString();

            IEnumerable<Ruta> lista = _soloGuardadas ? todas.Where(r => r.Guardada) : todas;
            if (!_soloGuardadas && _modo != "Todas") lista = lista.Where(r => r.Modo == _modo);

            string q = Buscar.Text.Trim();
            if (q.Length > 0)
            {
                lista = lista.Where(r =>
                    r.Nombre.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    r.OrigenNombre.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    r.DestinoNombre.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            var resultado = lista.ToList();
            ListaRutas.ItemsSource = resultado;

            PanelChips.Visibility = _soloGuardadas ? Visibility.Collapsed : Visibility.Visible;
            CtaExplorar.Visibility = _soloGuardadas ? Visibility.Visible : Visibility.Collapsed;
            TxtVacio.Visibility = resultado.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            TxtVacio.Text = _soloGuardadas ? "Aún no tienes rutas guardadas." : "No se encontraron rutas.";

            TxtSubtitulo.Text = _soloGuardadas ? "Tus rutas favoritas, siempre a un toque" : "Toca una ruta para verla en el mapa";
            TxtNota.Text = _soloGuardadas ? "" : "Mostrando " + resultado.Count + " de " + todas.Count + " rutas";

            // Si la ruta que se ve en el mapa cambió de "guardada", se actualiza su tarjeta
            if (_seleccion != null) Mapa.MostrarTarjeta(_seleccion);
        }

        private void TabTodas_Checked(object sender, RoutedEventArgs e)
        {
            if (!_listo) return;
            _soloGuardadas = false;
            Refrescar();
        }

        private void TabGuardadas_Checked(object sender, RoutedEventArgs e)
        {
            if (!_listo) return;
            _soloGuardadas = true;
            Refrescar();
        }

        private void Modo_Checked(object sender, RoutedEventArgs e)
        {
            if (!_listo) return;
            var rb = sender as RadioButton;
            _modo = (rb != null && rb.Tag != null) ? rb.Tag.ToString() : "Todas";
            Refrescar();
        }

        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refrescar();
        }

        private void BtnExplorar_Click(object sender, RoutedEventArgs e)
        {
            TabTodas.IsChecked = true;
        }

        // ── Selección y mapa ──────────────────────────────────────────────────
        private void Ruta_Click(object sender, MouseButtonEventArgs e)
        {
            var r = ((FrameworkElement)sender).DataContext as Ruta;
            if (r != null) Seleccionar(r);
        }

        private void Marcador_Click(object sender, RoutedEventArgs e)
        {
            var r = ((FrameworkElement)sender).DataContext as Ruta;
            if (r != null) Estado.AlternarGuardada(r); // dispara GuardadasCambiaron → Refrescar()
        }

        private void Seleccionar(Ruta r)
        {
            foreach (var x in Estado.Rutas) x.Seleccionada = (x == r);
            _seleccion = r;

            Mapa.DibujarRuta(r);
            Mapa.BannerRuta("Mostrando " + r.Nombre, true);
            Mapa.MostrarTarjeta(r);
        }

        // Botón "Quitar" del banner del mapa
        private void QuitarSeleccion()
        {
            foreach (var x in Estado.Rutas) x.Seleccionada = false;
            _seleccion = null;
            Mapa.LimpiarRuta();
            Mapa.OcultarBanner();
            Mapa.OcultarTarjeta();
        }

        // Botón "Planificar viaje con esta ruta" de la tarjeta del mapa
        private void PlanificarConRuta()
        {
            if (_seleccion == null) return;
            Estado.Origen = _seleccion.OrigenNombre;
            Estado.Destino = _seleccion.DestinoNombre;
            var nav = Estado.Navegar;
            if (nav != null) nav("planificar", null);
        }

        // Botón "Guardada" de la tarjeta del mapa
        private void GuardarSeleccion()
        {
            if (_seleccion != null) Estado.AlternarGuardada(_seleccion);
        }
    }
}
