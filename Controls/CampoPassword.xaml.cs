using System;
using System.Windows;
using System.Windows.Controls;

namespace Ruta_App
{
    /// <summary>Contraseña con candado, placeholder y botón "ojo" para mostrarla.</summary>
    public partial class CampoPassword : UserControl
    {
        public event Action PasswordChanged;

        private bool _sincronizando;

        public CampoPassword()
        {
            InitializeComponent();
        }

        public string Password
        {
            get { return Oculta.Password; }
            set { Oculta.Password = value ?? ""; }
        }

        private void Oculta_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_sincronizando) return;
            _sincronizando = true;
            Visible.Text = Oculta.Password;
            _sincronizando = false;
            Notificar();
        }

        private void Visible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_sincronizando) return;
            _sincronizando = true;
            Oculta.Password = Visible.Text;
            _sincronizando = false;
            Notificar();
        }

        private void Notificar()
        {
            Placeholder.Visibility = Oculta.Password.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            var h = PasswordChanged;
            if (h != null) h();
        }

        private void BtnOjo_Click(object sender, RoutedEventArgs e)
        {
            bool mostrar = Visible.Visibility != Visibility.Visible;
            Visible.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            Oculta.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
            if (mostrar) { Visible.Focus(); Visible.CaretIndex = Visible.Text.Length; }
            else Oculta.Focus();
        }
    }
}
