using System;
using System.Linq;
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

            // Landing page y página pública "Sobre Nosotros"
            Landing.IniciarSesion += () => MostrarAuth(false);
            Landing.VerSobreNosotros += () => { Landing.Visibility = Visibility.Collapsed; Sobre.Visibility = Visibility.Visible; };
            Landing.VerRutas += EntrarComoInvitadoARutas;
            Landing.ReproducirIntro += () => Intro.Reproducir();   // clic en el logo de la landing
            Sobre.Inicio += () => { Sobre.Visibility = Visibility.Collapsed; Landing.Visibility = Visibility.Visible; };
            Sobre.VerRutas += EntrarComoInvitadoARutas;

            ActualizarBadge();
        }

        // ── Sesión ────────────────────────────────────────────────────────────
        private string _trasIngreso = "planificar";   // pantalla a la que se va al entrar

        private void MostrarAuth(bool registro)
        {
            Landing.Visibility = Visibility.Collapsed;
            Sobre.Visibility = Visibility.Collapsed;
            Auth.Visibility = Visibility.Visible;
            if (registro) Auth.MostrarRegistro(); else Auth.MostrarLogin();
        }

        // "Rutas" de la landing: se explora la app como invitado, directo a las rutas
        private void EntrarComoInvitadoARutas()
        {
            _trasIngreso = "rutas";
            Auth.EntrarComoInvitado();
        }

        private async void Auth_Ingreso()
        {
            await Estado.CargarRutasRealesAsync();   // rutas de la base de datos; si la API no responde se quedan las de ejemplo
            var _ = Estado.PrepararTrazosAsync();   // recorridos por calles, en segundo plano
            Landing.Visibility = Visibility.Collapsed;
            Sobre.Visibility = Visibility.Collapsed;
            Auth.Visibility = Visibility.Collapsed;
            ActualizarBotonSesion();
            AppShell.Visibility = Visibility.Visible;
            string destino = _trasIngreso;
            _trasIngreso = "planificar";
            Navegar(destino, null);
        }

        // ── Mapas bajo un pop-up ──────────────────────────────────────────────
        private static IEnumerable<MapaControl> BuscarMapas(DependencyObject raiz)
        {
            int n = System.Windows.Media.VisualTreeHelper.GetChildrenCount(raiz);
            for (int i = 0; i < n; i++)
            {
                var hijo = System.Windows.Media.VisualTreeHelper.GetChild(raiz, i);
                var mapa = hijo as MapaControl;
                if (mapa != null) yield return mapa;
                foreach (var m in BuscarMapas(hijo)) yield return m;
            }
        }

        private async System.Threading.Tasks.Task CongelarMapasAsync()
        {
            foreach (var mapa in BuscarMapas(Contenido).ToList())
                await mapa.CongelarAsync();
        }

        private void DescongelarMapas()
        {
            foreach (var mapa in BuscarMapas(Contenido).ToList())
                mapa.Descongelar();
        }

        // Invitado: el botón dice "Iniciar sesión" y lleva al login. Con cuenta: "Cerrar sesión" (con confirmación).
        private void ActualizarBotonSesion()
        {
            string texto = Estado.Invitado ? "Iniciar sesión" : "Cerrar sesión";
            TxtLogout.Text = texto;
            BtnLogout.ToolTip = texto;
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (Estado.Invitado)
            {
                // No hay sesión que cerrar: se va directo a la pantalla de inicio de sesión
                Auth.Reiniciar();
                AppShell.Visibility = Visibility.Collapsed;
                Auth.Visibility = Visibility.Visible;
                return;
            }

            await CongelarMapasAsync();   // el mapa (WebView2) taparía el pop-up
            ModalCerrarSesion.Visibility = Visibility.Visible;
        }

        private void BtnCancelarCerrar_Click(object sender, RoutedEventArgs e)
        {
            ModalCerrarSesion.Visibility = Visibility.Collapsed;
            DescongelarMapas();
        }

        private void BtnConfirmarCerrar_Click(object sender, RoutedEventArgs e)
        {
            ModalCerrarSesion.Visibility = Visibility.Collapsed;
            DescongelarMapas();
            Auth.Reiniciar();
            AppShell.Visibility = Visibility.Collapsed;
            Landing.Visibility = Visibility.Visible;
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
                case "ajuste": return new AjusteDetalleView();
                default: return null;
            }
        }

        /// <summary>Pinta de azul el botón del sidebar de la pantalla actual (Comparar cuenta como Planificar).</summary>
        private void MarcarActivo(string clave)
        {
            string activo = clave == "comparar" ? "planificar" : (clave == "ajuste" ? "config" : clave);
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

        private void Auth_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
