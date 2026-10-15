using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Ruta_App
{
    /// <summary>
    /// Carrusel de pantallas completas con cambio automático (fundido), puntos para elegir y pausa al pasar el mouse.
    /// Se usa en el inicio de sesión / registro y en la landing page.
    /// Uso:  carrusel.AgregarSlide(new SlideMarca());  carrusel.AgregarFotosDeCiudades();
    /// </summary>
    public class CarruselControl : UserControl
    {
        private readonly Grid _raiz = new Grid();
        private readonly Grid _slides = new Grid();
        private readonly StackPanel _puntos = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly List<UIElement> _lista = new List<UIElement>();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        private int _actual;

        public CarruselControl()
        {
            _puntos.HorizontalAlignment = HorizontalAlignment.Center;
            _puntos.VerticalAlignment = VerticalAlignment.Bottom;
            _puntos.Margin = new Thickness(0, 0, 0, 18);

            _raiz.Children.Add(_slides);
            _raiz.Children.Add(_puntos);
            Content = _raiz;
            ClipToBounds = true;

            _timer.Tick += (s, e) => Ir((_actual + 1) % Math.Max(1, _lista.Count));
            MouseEnter += (s, e) => _timer.Stop();
            MouseLeave += (s, e) => { if (_lista.Count > 1 && IsVisible) _timer.Start(); };
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible && _lista.Count > 1) _timer.Start(); else _timer.Stop();
            };
        }

        /// <summary>Agrega una pantalla completa al carrusel.</summary>
        public void AgregarSlide(UIElement slide)
        {
            slide.Opacity = _lista.Count == 0 ? 1 : 0;
            slide.IsHitTestVisible = _lista.Count == 0;
            _slides.Children.Add(slide);
            _lista.Add(slide);

            var punto = new Ellipse { Width = 8, Height = 8, Margin = new Thickness(4, 0, 4, 0), Cursor = System.Windows.Input.Cursors.Hand };
            int indice = _lista.Count - 1;
            punto.MouseLeftButtonUp += (s, e) => { Ir(indice); if (IsVisible) { _timer.Stop(); _timer.Start(); } };
            _puntos.Children.Add(punto);
            PintarPuntos();

            if (_lista.Count > 1 && IsVisible) _timer.Start();
        }

        /// <summary>Agrega las fotos de las ciudades donde está R.U.T.A. (Chihuahua, Cd. Juárez y Delicias).</summary>
        public void AgregarFotosDeCiudades()
        {
            AgregarSlide(CrearSlideFoto("chihuahua.jpg", "CHIHUAHUA"));
            AgregarSlide(CrearSlideFoto("cdjuarez.jpg", "CD. JUAREZ"));
            AgregarSlide(CrearSlideFoto("delicias.jpg", "DELICIAS"));
        }

        private static UIElement CrearSlideFoto(string archivo, string ciudad)
        {
            var imagen = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/" + Uri.EscapeDataString(typeof(CarruselControl).Assembly.GetName().Name) + ";component/Resources/Carrusel/" + archivo, UriKind.Absolute)),
                Stretch = Stretch.UniformToFill
            };
            RenderOptions.SetBitmapScalingMode(imagen, BitmapScalingMode.HighQuality);

            // Velo oscuro arriba y abajo para que el texto blanco se lea sobre cualquier foto
            var velo = new Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(150, 10, 25, 60), 0.0),
                    new GradientStop(Color.FromArgb(0, 10, 25, 60), 0.28),
                    new GradientStop(Color.FromArgb(0, 10, 25, 60), 0.72),
                    new GradientStop(Color.FromArgb(150, 10, 25, 60), 1.0)
                }, 90)
            };

            var sombra = new DropShadowEffect { BlurRadius = 8, ShadowDepth = 1, Opacity = 0.6, Color = Colors.Black };

            var titulo = new TextBlock
            {
                Text = "Ciudades donde\nesta R.U.T.A.",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 36, 0, 0),
                Effect = sombra
            };
            var nombre = new TextBlock
            {
                Text = ciudad,
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 52),
                Effect = sombra
            };

            var g = new Grid { Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x36, 0x5D)), ClipToBounds = true };
            g.Children.Add(imagen);
            g.Children.Add(velo);
            g.Children.Add(titulo);
            g.Children.Add(nombre);
            return g;
        }

        private void Ir(int indice)
        {
            if (_lista.Count == 0 || indice == _actual) return;

            var sale = _lista[_actual];
            var entra = _lista[indice];
            _actual = indice;

            entra.IsHitTestVisible = true;
            sale.IsHitTestVisible = false;
            Panel.SetZIndex(entra, 1);
            Panel.SetZIndex(sale, 0);
            entra.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(600)));
            sale.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(600)));
            PintarPuntos();
        }

        private void PintarPuntos()
        {
            for (int i = 0; i < _puntos.Children.Count; i++)
            {
                var p = (Ellipse)_puntos.Children[i];
                p.Fill = i == _actual ? Brushes.White : new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
                p.Effect = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 0, Opacity = 0.5, Color = Colors.Black };
            }
        }
    }
}
