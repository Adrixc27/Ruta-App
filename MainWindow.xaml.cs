using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ruta_App
{
    public partial class MainWindow : Window
    {
        // Las pantallas se crean la primera vez que se visitan y luego se reutilizan
        // (no se quitan del árbol visual: así los mapas WebView2 no se reinician).
        private readonly Dictionary<string, UserControl> _vistas = new Dictionary<string, UserControl>();
        private bool _colapsado;

        public MainWindow()
        {
            InitializeComponent();

            Estado.Navegar = Navegar;                    // las pantallas navegan con Estado.Navegar("clave", parametro)
            Estado.GuardadasCambiaron += ActualizarBadge; // el número junto a "Rutas"
            Auth.Ingreso += Auth_Ingreso;

            ActualizarBadge();
        }

        // ── Sesión ────────────────────────────────────────────────────────────
        private void Auth_Ingreso()
        {
            Auth.Visibility = Visibility.Collapsed;
            AppShell.Visibility = Visibility.Visible;
            Navegar("planificar", null);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Auth.Reiniciar();
            AppShell.Visibility = Visibility.Collapsed;
            Auth.Visibility = Visibility.Visible;
        }

        // ── Navegación ────────────────────────────────────────────────────────
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            switch (((FrameworkElement)sender).Name)
            {
                case "NavPlanificar": Navegar("planificar", null); break;
                case "NavRutas": Navegar("rutas", null); break;
                case "NavComunidad": Navegar("comunidad", null); break;
                case "NavPerfil": Navegar("perfil", null); break;
                case "NavConfig": Navegar("config", null); break;
            }
        }

        /// <summary>
        /// Claves: planificar, comparar, rutas, comunidad, perfil, config.
        /// "parametro" es opcional (por ejemplo una Ruta para abrirla en el mapa).
        /// </summary>
        public void Navegar(string clave, object parametro)
        {
            UserControl vista;
            if (!_vistas.TryGetValue(clave, out vista))
            {
                vista = Crear(clave);
                if (vista == null) return;
                _vistas[clave] = vista;
                Contenido.Children.Add(vista);
            }

            foreach (var par in _vistas)
                par.Value.Visibility = par.Key == clave ? Visibility.Visible : Visibility.Collapsed;

            MarcarActivo(clave);

            var navegable = vista as INavegable;
            if (navegable != null) navegable.AlNavegar(parametro);
        }

        private static UserControl Crear(string clave)
        {
            switch (clave)
            {
                case "planificar": return new PlanificarView();
                case "comparar": return new CompararView();
                case "rutas": return new RutasView();
                case "comunidad": return new ComunidadView();
                case "perfil": return new PerfilView();
                case "config": return new ConfiguracionView();
                default: return null;
            }
        }

        /// <summary>Pinta de azul el botón del sidebar de la pantalla actual (Comparar cuenta como Planificar).</summary>
        private void MarcarActivo(string clave)
        {
            string activo = clave == "comparar" ? "planificar" : clave;
            NavPlanificar.Tag = activo == "planificar" ? "Activo" : "Normal";
            NavRutas.Tag = activo == "rutas" ? "Activo" : "Normal";
            NavComunidad.Tag = activo == "comunidad" ? "Activo" : "Normal";
            NavPerfil.Tag = activo == "perfil" ? "Activo" : "Normal";
            NavConfig.Tag = activo == "config" ? "Activo" : "Normal";
        }

        private void ActualizarBadge()
        {
            int n = Estado.TotalGuardadas;
            TxtBadgeRutas.Text = n.ToString();
            BadgeRutas.Visibility = n > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Sidebar colapsable ────────────────────────────────────────────────
        private void BtnColapsar_Click(object sender, RoutedEventArgs e)
        {
            _colapsado = !_colapsado;
            AplicarColapso(_colapsado);
        }

        private void AplicarColapso(bool colapsar)
        {
            // Al contraer se ocultan las etiquetas de inmediato; al expandir, cuando termina la animación
            if (colapsar) EtiquetasVisibles(false);

            var animacion = new DoubleAnimation(colapsar ? 100 : 260, TimeSpan.FromMilliseconds(220))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            if (!colapsar)
                animacion.Completed += (s, e) => EtiquetasVisibles(true);
            Sidebar.BeginAnimation(WidthProperty, animacion);

            IcoColapsar.Data = (Geometry)FindResource(colapsar ? "IcoChevronRight" : "IcoChevronLeft");
            BtnColapsar.ToolTip = colapsar ? "Expandir menú" : "Contraer menú";

            // La insignia de "Rutas" pasa a la esquina del icono cuando no hay texto
            if (colapsar)
            {
                Grid.SetColumn(BadgeRutas, 0);
                BadgeRutas.HorizontalAlignment = HorizontalAlignment.Left;
                BadgeRutas.VerticalAlignment = VerticalAlignment.Top;
                BadgeRutas.Margin = new Thickness(12, -10, 0, 0);
            }
            else
            {
                Grid.SetColumn(BadgeRutas, 2);
                BadgeRutas.HorizontalAlignment = HorizontalAlignment.Right;
                BadgeRutas.VerticalAlignment = VerticalAlignment.Center;
                BadgeRutas.Margin = new Thickness(0);
            }
        }

        private void EtiquetasVisibles(bool visibles)
        {
            var v = visibles ? Visibility.Visible : Visibility.Collapsed;
            PanelMarca.Visibility = v;
            LblMenu.Visibility = v;
            TxtNavPlanificar.Visibility = v;
            TxtNavRutas.Visibility = v;
            TxtNavComunidad.Visibility = v;
            TxtNavPerfil.Visibility = v;
            TxtNavConfig.Visibility = v;
            TxtColapsar.Visibility = v;
            TxtLogout.Visibility = v;
        }
    }
}
