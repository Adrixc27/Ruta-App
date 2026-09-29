using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Ruta_App
{
    public partial class ComunidadView : UserControl
    {
        private readonly ICollectionView _vista;
        private bool _listo;
        private string _tipo = "";            // "" = todo | aviso | pregunta | propuesta
        private bool _porPopularidad;
        private Ruta _rutaModal;              // ruta relacionada elegida en el modal (null = ninguna)

        public ComunidadView()
        {
            InitializeComponent();

            _vista = CollectionViewSource.GetDefaultView(Estado.Publicaciones);
            _vista.Filter = Coincide;
            AplicarOrden();
            Feed.ItemsSource = _vista;

            _listo = true;
            Refrescar();
            ActualizarBotonCargar();
        }

        // ── Filtro, orden y conteos ───────────────────────────────────────────
        private bool Coincide(object o)
        {
            var p = (Publicacion)o;
            if (_tipo.Length > 0 && p.Tipo != _tipo) return false;

            string q = Buscar.Text.Trim();
            if (q.Length == 0) return true;
            return Contiene(p.Titulo, q) || Contiene(p.Cuerpo, q) || Contiene(p.Autor, q) ||
                   p.Etiquetas.Any(t => Contiene(t.Texto, q));
        }

        private static bool Contiene(string texto, string q)
        {
            return texto != null && texto.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AplicarOrden()
        {
            _vista.SortDescriptions.Clear();
            _vista.SortDescriptions.Add(new SortDescription(_porPopularidad ? "Likes" : "Orden", ListSortDirection.Descending));
        }

        private void Refrescar()
        {
            if (!_listo) return;
            _vista.Refresh();

            var l = Estado.Publicaciones;
            CntTodo.Text = l.Count.ToString();
            CntAvisos.Text = l.Count(p => p.Tipo == "aviso").ToString();
            CntPreguntas.Text = l.Count(p => p.Tipo == "pregunta").ToString();
            CntPropuestas.Text = l.Count(p => p.Tipo == "propuesta").ToString();

            TxtSinResultados.Visibility = _vista.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ActualizarBotonCargar()
        {
            BtnCargarMas.Visibility = Estado.PublicacionesExtra.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Filtro_Checked(object sender, RoutedEventArgs e)
        {
            if (!_listo) return;
            var rb = sender as RadioButton;
            _tipo = (rb != null && rb.Tag != null) ? rb.Tag.ToString() : "";
            Refrescar();
        }

        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refrescar();
        }

        private void BtnOrden_Click(object sender, RoutedEventArgs e)
        {
            _porPopularidad = !_porPopularidad;
            TxtOrden.Text = _porPopularidad ? "Más populares" : "Más recientes";
            AplicarOrden();
            Refrescar();
        }

        private void BtnCargarMas_Click(object sender, RoutedEventArgs e)
        {
            foreach (var p in Estado.PublicacionesExtra) Estado.Publicaciones.Add(p);
            Estado.PublicacionesExtra.Clear();
            Refrescar();
            ActualizarBotonCargar();
        }

        // ── Acciones de cada publicación ──────────────────────────────────────
        private static Publicacion DePublicacion(object sender)
        {
            return ((FrameworkElement)sender).DataContext as Publicacion;
        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            var p = DePublicacion(sender);
            if (p == null) return;
            p.MeGusta = !p.MeGusta;
            p.Likes += p.MeGusta ? 1 : -1;
        }

        private void Comentar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Los comentarios estarán disponibles próximamente.",
                            "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cta_Click(object sender, RoutedEventArgs e)
        {
            var p = DePublicacion(sender);
            if (p == null) return;

            if (p.Tipo == "pregunta")
            {
                MessageBox.Show("Las respuestas estarán disponibles próximamente.",
                                "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Ruta ruta = p.RutaNumero > 0 ? Estado.BuscarRuta(p.RutaNumero) : null;
            var nav = Estado.Navegar;
            if (nav != null) nav("rutas", ruta);
        }

        private void BtnVerRuta_Click(object sender, RoutedEventArgs e)
        {
            var nav = Estado.Navegar;
            if (nav != null) nav("rutas", Estado.BuscarRuta(4));
        }

        // ── Modal "Nueva publicación" ─────────────────────────────────────────
        private void BtnNueva_Click(object sender, RoutedEventArgs e)
        {
            if (Estado.Invitado)
            {
                MessageBox.Show("Inicia sesión o crea una cuenta para publicar en la comunidad.",
                                "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TxtTitulo.Text = "";
            TxtMensaje.Text = "";
            TxtErrorModal.Visibility = Visibility.Collapsed;
            _rutaModal = null;
            TxtRutaSel.Text = "Sin ruta específica";
            TipoPregunta.IsChecked = true;
            Modal.Visibility = Visibility.Visible;
            TxtTitulo.Focus();
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            Modal.Visibility = Visibility.Collapsed;
        }

        private void TxtTitulo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtContador != null) TxtContador.Text = TxtTitulo.Text.Length + "/60";
        }

        private void BtnRutaSel_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu
            {
                PlacementTarget = BtnRutaSel,
                Placement = PlacementMode.Bottom,
                MinWidth = BtnRutaSel.ActualWidth
            };

            var ninguna = new MenuItem { Header = "Sin ruta específica" };
            ninguna.Click += (s, a) => { _rutaModal = null; TxtRutaSel.Text = "Sin ruta específica"; };
            menu.Items.Add(ninguna);

            foreach (var r in Estado.Rutas)
            {
                var ruta = r; // copia para el lambda
                var item = new MenuItem { Header = ruta.Nombre };
                item.Click += (s, a) => { _rutaModal = ruta; TxtRutaSel.Text = ruta.Nombre; };
                menu.Items.Add(item);
            }

            menu.IsOpen = true;
        }

        private void BtnPublicar_Click(object sender, RoutedEventArgs e)
        {
            string titulo = TxtTitulo.Text.Trim();
            string mensaje = TxtMensaje.Text.Trim();
            if (titulo.Length == 0 || mensaje.Length == 0)
            {
                TxtErrorModal.Text = "Escribe un título y un mensaje para publicar.";
                TxtErrorModal.Visibility = Visibility.Visible;
                return;
            }

            string tipo = TipoAviso.IsChecked == true ? "aviso" : (TipoPropuesta.IsChecked == true ? "propuesta" : "pregunta");
            int orden = Estado.Publicaciones.Count > 0 ? Estado.Publicaciones.Max(p => p.Orden) + 1 : 1;

            var nueva = new Publicacion
            {
                Tipo = tipo,
                Autor = Estado.Nombre,
                Tiempo = "ahora",
                Titulo = titulo,
                Cuerpo = mensaje,
                Orden = orden,
                RutaNumero = _rutaModal != null ? _rutaModal.Numero : 0
            };
            if (_rutaModal != null)
                nueva.Etiquetas.Add(new Etiqueta { Texto = "Ruta " + _rutaModal.Numero, EsRuta = true });

            Estado.Publicaciones.Add(nueva);

            Modal.Visibility = Visibility.Collapsed;
            FiltroTodo.IsChecked = true;
            Buscar.Text = "";
            Refrescar();
        }
    }
}
