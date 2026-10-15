using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    /// <summary>Landing page: presentación de RUTA con el mismo carrusel del inicio de sesión.</summary>
    public partial class LandingView : UserControl
    {
        public event Action IniciarSesion;
        public event Action VerSobreNosotros;
        public event Action VerRutas;
        public event Action ReproducirIntro;

        public LandingView()
        {
            InitializeComponent();

            Carrusel.AgregarSlide(new SlideMarca
            {
                Titular = "Muévete por tu ciudad sin perderte.",
                Subtitular = "Planifica, compara y viaja en transporte público con rutas claras y en tiempo real."
            });
            Carrusel.AgregarFotosDeCiudades();

            // Esquinas redondeadas también para el contenido del carrusel
            Tarjeta.SizeChanged += (s, e) =>
                Carrusel.Clip = new RectangleGeometry(new Rect(0, 0, Tarjeta.ActualWidth - 2, Tarjeta.ActualHeight - 2), 31, 31);
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) { var h = ReproducirIntro; if (h != null) h(); }
        private void BtnLogin_Click(object sender, RoutedEventArgs e) { var h = IniciarSesion; if (h != null) h(); }
        private void BtnExplorar_Click(object sender, RoutedEventArgs e) { var h = VerRutas; if (h != null) h(); }
        private void BtnSobre_Click(object sender, RoutedEventArgs e) { var h = VerSobreNosotros; if (h != null) h(); }
        private void BtnRutas_Click(object sender, RoutedEventArgs e) { var h = VerRutas; if (h != null) h(); }
    }
}
