using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    public partial class AuthView : UserControl
    {
        /// <summary>Se dispara cuando el usuario entra (login, registro o invitado).</summary>
        public event Action Ingreso;

        public AuthView()
        {
            InitializeComponent();
            RegPass.PasswordChanged += ActualizarFuerza;
            MostrarLogin();
            ActualizarFuerza();
        }

        /// <summary>Limpia los formularios (se llama al cerrar sesión).</summary>
        public void Reiniciar()
        {
            LoginCorreo.Text = "";
            LoginPass.Password = "";
            RegNombre.Text = "";
            RegCorreo.Text = "";
            RegPass.Password = "";
            ChkTerminos.IsChecked = false;
            LoginError.Visibility = Visibility.Collapsed;
            RegError.Visibility = Visibility.Collapsed;
            MostrarLogin();
        }

        // ── Cambiar entre login y registro ────────────────────────────────────
        private void MostrarLogin()
        {
            FormLogin.Visibility = Visibility.Visible;
            FormRegistro.Visibility = Visibility.Collapsed;
            TxtTitular.Text = "Muévete por tu ciudad sin perderte.";
            TxtSubtitular.Text = "Planifica, compara y viaja en transporte público con rutas claras y en tiempo real.";
        }

        private void MostrarRegistro()
        {
            FormLogin.Visibility = Visibility.Collapsed;
            FormRegistro.Visibility = Visibility.Visible;
            TxtTitular.Text = "Tus rutas favoritas, siempre a la mano.";
            TxtSubtitular.Text = "Guarda tus trayectos frecuentes y consulta tiempos de llegada en tiempo real.";
        }

        private void BtnIrRegistro_Click(object sender, RoutedEventArgs e) { MostrarRegistro(); }
        private void BtnIrLogin_Click(object sender, RoutedEventArgs e) { MostrarLogin(); }

        // ── Acciones ──────────────────────────────────────────────────────────
        // NOTA: no hay servidor. Cuando tengas API/base de datos, valida aquí las credenciales.
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string correo = LoginCorreo.Text.Trim();
            if (!EsCorreo(correo))
            {
                MostrarError(LoginError, "Escribe un correo válido, por ejemplo tu@correo.com.");
                return;
            }
            if (LoginPass.Password.Length == 0)
            {
                MostrarError(LoginError, "Escribe tu contraseña.");
                return;
            }

            LoginError.Visibility = Visibility.Collapsed;
            Estado.Correo = correo;
            Estado.Nombre = correo.ToLowerInvariant() == "laura.g@ruta.com" ? "Laura Guerrero" : NombreDesdeCorreo(correo);
            Estado.Invitado = false;
            Entrar();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = RegNombre.Text.Trim();
            string correo = RegCorreo.Text.Trim();

            if (nombre.Length < 2) { MostrarError(RegError, "Escribe tu nombre completo."); return; }
            if (!EsCorreo(correo)) { MostrarError(RegError, "Escribe un correo válido, por ejemplo tu@correo.com."); return; }
            if (RegPass.Password.Length < 6) { MostrarError(RegError, "La contraseña debe tener al menos 6 caracteres."); return; }
            if (ChkTerminos.IsChecked != true) { MostrarError(RegError, "Debes aceptar los términos para continuar."); return; }

            RegError.Visibility = Visibility.Collapsed;
            Estado.Nombre = nombre;
            Estado.Correo = correo;
            Estado.Invitado = false;
            Entrar();
        }

        private void BtnInvitado_Click(object sender, RoutedEventArgs e)
        {
            Estado.Nombre = "Invitado";
            Estado.Correo = "Sin cuenta";
            Estado.Invitado = true;
            Entrar();
        }

        private void BtnGoogle_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("El acceso con Google todavía no está disponible en esta versión.",
                            "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnOlvide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("La recuperación de contraseña estará disponible próximamente.",
                            "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Entrar()
        {
            var h = Ingreso;
            if (h != null) h();
        }

        private static void MostrarError(TextBlock caja, string texto)
        {
            caja.Text = texto;
            caja.Visibility = Visibility.Visible;
        }

        // ── Validaciones ──────────────────────────────────────────────────────
        private static bool EsCorreo(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Contains(" ")) return false;
            int arroba = s.IndexOf('@');
            if (arroba < 1 || arroba != s.LastIndexOf('@')) return false;
            int punto = s.LastIndexOf('.');
            return punto > arroba + 1 && punto < s.Length - 1;
        }

        private static string NombreDesdeCorreo(string correo)
        {
            string parte = correo.Substring(0, correo.IndexOf('@')).Replace('.', ' ').Replace('_', ' ').Replace('-', ' ');
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parte);
        }

        // ── Fuerza de la contraseña (4 barras) ────────────────────────────────
        private void ActualizarFuerza()
        {
            string p = RegPass.Password;
            int puntos = 0;
            if (p.Length >= 8) puntos++;
            if (p.Any(char.IsUpper) && p.Any(char.IsLower)) puntos++;
            if (p.Any(char.IsDigit)) puntos++;
            if (p.Any(c => !char.IsLetterOrDigit(c))) puntos++;
            if (p.Length == 0) puntos = 0;

            Brush color;
            string texto;
            switch (puntos)
            {
                case 1: color = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); texto = "Contraseña débil"; break;
                case 2: color = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)); texto = "Contraseña media"; break;
                case 3: color = (Brush)FindResource("BrandPrimary"); texto = "Contraseña buena"; break;
                case 4: color = (Brush)FindResource("Success"); texto = "Contraseña muy segura"; break;
                default: color = (Brush)FindResource("BorderSoft"); texto = "Usa 8+ caracteres con mayúsculas, números y símbolos"; break;
            }

            var barras = new[] { Barra1, Barra2, Barra3, Barra4 };
            for (int i = 0; i < barras.Length; i++)
                barras[i].Background = i < puntos ? color : (Brush)FindResource("BorderSoft");
            TxtFuerza.Text = texto;
        }
    }
}
