using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ruta_App
{
    public partial class PerfilView : UserControl, INavegable
    {
        public PerfilView()
        {
            InitializeComponent();
            ListaTrayectos.ItemsSource = Estado.Trayectos;
            Estado.Trayectos.CollectionChanged += (s, e) => ActualizarVacio();
            AlNavegar(null);
        }

        /// <summary>Recarga nombre y correo cada vez que entras (pueden haber cambiado al iniciar sesión).</summary>
        public void AlNavegar(object parametro)
        {
            TxtNombre.Text = Estado.Nombre;
            TxtCorreo.Text = Estado.Correo;
            TxtIniciales.Text = Estado.Iniciales(Estado.Nombre);
            ActualizarVacio();
        }

        private void ActualizarVacio()
        {
            TxtVacio.Visibility = Estado.Trayectos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Editar nombre ─────────────────────────────────────────────────────
        private void BtnEditarNombre_Click(object sender, RoutedEventArgs e)
        {
            TxtNuevoNombre.Text = Estado.Nombre;
            ErrorNombre.Visibility = Visibility.Collapsed;
            ModalNombre.Visibility = Visibility.Visible;
            TxtNuevoNombre.Focus();
            TxtNuevoNombre.SelectAll();
        }

        private void BtnCancelarNombre_Click(object sender, RoutedEventArgs e)
        {
            ModalNombre.Visibility = Visibility.Collapsed;
        }

        private void BtnGuardarNombre_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNuevoNombre.Text.Trim();
            if (nombre.Length < 2)
            {
                ErrorNombre.Visibility = Visibility.Visible;
                return;
            }
            Estado.Nombre = nombre;
            AlNavegar(null);
            ModalNombre.Visibility = Visibility.Collapsed;
        }

        private void TxtNuevoNombre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnGuardarNombre_Click(sender, e);
            else if (e.Key == Key.Escape) BtnCancelarNombre_Click(sender, e);
        }

        // ── Cambiar foto ──────────────────────────────────────────────────────
        private void BtnCambiarFoto_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Elige tu foto de perfil",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp|Todos los archivos|*.*"
            };
            if (dialogo.ShowDialog() != true) return;

            try
            {
                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.UriSource = new Uri(dialogo.FileName);
                imagen.EndInit();

                Avatar.Background = new ImageBrush(imagen) { Stretch = Stretch.UniformToFill };
                TxtIniciales.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo cargar esa imagen. Prueba con otro archivo.",
                                "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Trayectos guardados ───────────────────────────────────────────────
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var nav = Estado.Navegar;
            if (nav != null) nav("planificar", null);
        }

        private void Favorito_Click(object sender, RoutedEventArgs e)
        {
            var t = ((FrameworkElement)sender).DataContext as Trayecto;
            if (t != null) t.Favorito = !t.Favorito;
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            var t = ((FrameworkElement)sender).DataContext as Trayecto;
            if (t == null) return;

            var respuesta = MessageBox.Show("¿Quitar \"" + t.Alias + "\" de tus rutas guardadas?",
                                            "R.U.T.A.", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta == MessageBoxResult.Yes) Estado.Trayectos.Remove(t);
        }
    }
}
