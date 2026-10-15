using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Ruta_App
{
    /// <summary>
    /// Animación de entrada de la landing page. Se repite en ciclo hasta que el usuario hace clic:
    ///   1) un camión cruza la pantalla por encima de las letras de "RUTA" y las va mostrando
    ///      (debajo, el lema "Recomendador Urbano de Transporte Accesible");
    ///   2) las letras se quedan unos segundos;
    ///   3) se desvanecen y el camión vuelve a pasar.
    /// Mientras el camión avanza los clics se ignoran; al terminar su pasada, un clic desvanece la
    /// pantalla y deja ver la landing page. Reproducir() la vuelve a mostrar (logo de la landing).
    /// </summary>
    public class IntroView : UserControl
    {
        public static readonly DependencyProperty ProgresoProperty =
            DependencyProperty.Register("Progreso", typeof(double), typeof(IntroView),
                new PropertyMetadata(0.0, (d, e) => ((IntroView)d).Actualizar((double)e.NewValue)));

        /// <summary>0 = el camión aún no entra; 1 = ya salió por el lado derecho.</summary>
        public double Progreso
        {
            get { return (double)GetValue(ProgresoProperty); }
            set { SetValue(ProgresoProperty, value); }
        }

        public event Action Terminada;

        private const double LadoCamion = 130;
        private const double SegundosPasada = 3.6;      // lo que tarda el camión en cruzar
        private const double SegundosLetras = 2.8;      // las letras se quedan visibles
        private const double SegundosDesvanecer = 0.7;

        private readonly Grid _raiz = new Grid { Background = (Brush)Application.Current.FindResource("BgPanel") };
        private readonly StackPanel _marca = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBlock _nombre;
        private readonly TextBlock _pista;
        private readonly Canvas _escena = new Canvas { IsHitTestVisible = false };
        private readonly Rectangle _carretera;
        private readonly Grid _camion = new Grid { Width = LadoCamion, Height = LadoCamion };
        private readonly RectangleGeometry _recorte = new RectangleGeometry();

        private int _version;                 // cada Reproducir() inicia un ciclo nuevo y cancela el anterior
        private bool _terminada;
        private bool _viajando;               // el camión está cruzando: no se acepta el clic
        private TaskCompletionSource<bool> _pasada;

        public IntroView()
        {
            var azul = (Brush)Application.Current.FindResource("BrandPrimary");

            _nombre = new TextBlock
            {
                Text = "RUTA", FontSize = 150, FontWeight = FontWeights.Black, Foreground = azul,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Lema: las iniciales (R, U, T, A) en negrita para que se lea RUTA
            var lema = new TextBlock
            {
                FontSize = 24, Foreground = azul, HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, -6, 0, 0)
            };
            AgregarPalabra(lema, "R", "ecomendador ");
            AgregarPalabra(lema, "U", "rbano de ");
            AgregarPalabra(lema, "T", "ransporte ");
            AgregarPalabra(lema, "A", "ccesible");

            _marca.Children.Add(_nombre);
            _marca.Children.Add(lema);
            _marca.Clip = _recorte;                    // el camión va abriendo este recorte hacia la derecha
            _raiz.Children.Add(_marca);

            // Escena: carretera y camión (por encima de las letras)
            _carretera = new Rectangle
            {
                Height = 6, RadiusX = 3, RadiusY = 3, Opacity = 0.9,
                Fill = (Brush)Application.Current.FindResource("BorderDefault")
            };
            _camion.Children.Add(new Icon
            {
                Data = (Geometry)Application.Current.FindResource("IcoBus"),
                Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
                Relleno = Brushes.White, Width = LadoCamion, Height = LadoCamion
            });
            _camion.RenderTransform = new TranslateTransform();
            _escena.Children.Add(_carretera);
            _escena.Children.Add(_camion);
            _raiz.Children.Add(_escena);

            // "Haz clic para continuar" (aparece cuando el camión termina de pasar)
            _pista = new TextBlock
            {
                Text = "Haz clic para continuar", FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = azul,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 56), Opacity = 0, IsHitTestVisible = false
            };
            _raiz.Children.Add(_pista);

            Content = _raiz;

            Loaded += (s, e) => { if (_version == 0) Reproducir(); };
            MouseLeftButtonDown += (s, e) => { if (!_viajando) Terminar(); };   // no se puede saltar mientras pasa el camión
            SizeChanged += (s, e) => Actualizar(Progreso);
        }

        private static void AgregarPalabra(TextBlock t, string inicial, string resto)
        {
            t.Inlines.Add(new Run(inicial) { FontWeight = FontWeights.Black });
            t.Inlines.Add(new Run(resto) { FontWeight = FontWeights.Normal });
        }

        /// <summary>Muestra la animación desde el principio y la repite en ciclo hasta que el usuario haga clic.</summary>
        public void Reproducir()
        {
            if (_pasada != null) _pasada.TrySetResult(false);   // cancela el ciclo anterior
            int version = ++_version;
            _terminada = false;
            _viajando = false;

            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
            Visibility = Visibility.Visible;
            UpdateLayout();

            var ignorar = CicloAsync(version);
        }

        private async Task CicloAsync(int version)
        {
            while (version == _version && !_terminada)
            {
                // 1) El camión cruza y va mostrando las letras
                OcultarPista();
                _marca.BeginAnimation(OpacityProperty, null);
                _marca.Opacity = 1;
                BeginAnimation(ProgresoProperty, null);
                Progreso = 0;
                Actualizar(0);

                _pasada = new TaskCompletionSource<bool>();
                var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(SegundosPasada))
                {
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.HoldEnd
                };
                var esta = _pasada;
                anim.Completed += (s, e) => esta.TrySetResult(true);
                _viajando = true;
                BeginAnimation(ProgresoProperty, anim);
                bool llego = await esta.Task;
                _viajando = false;
                if (!llego || version != _version || _terminada) return;

                // 2) Las letras se quedan unos segundos (aquí ya se acepta el clic)
                MostrarPista();
                await Task.Delay(TimeSpan.FromSeconds(SegundosLetras));
                if (version != _version || _terminada) return;

                // 3) Las letras se desvanecen y el camión vuelve a pasar
                OcultarPista();
                _marca.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(SegundosDesvanecer)));
                await Task.Delay(TimeSpan.FromSeconds(SegundosDesvanecer + 0.3));
            }
        }

        private void MostrarPista()
        {
            var latido = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            latido.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            latido.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.35)));
            latido.KeyFrames.Add(new LinearDoubleKeyFrame(0.45, KeyTime.FromPercent(0.75)));
            latido.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(1)));
            latido.Duration = TimeSpan.FromSeconds(2.2);
            _pista.BeginAnimation(OpacityProperty, latido);
        }

        private void OcultarPista()
        {
            _pista.BeginAnimation(OpacityProperty, null);
            _pista.Opacity = 0;
        }

        // Coloca el camión sobre las letras y abre el recorte hasta donde va su frente
        private void Actualizar(double p)
        {
            double ancho = ActualWidth;
            if (ancho <= 0 || _marca.ActualWidth <= 0 || _nombre.ActualHeight <= 0) return;

            // Centro vertical de las letras (en coordenadas de la ventana)
            var origenNombre = _nombre.TransformToAncestor(this).Transform(new Point(0, 0));
            double centroLetras = origenNombre.Y + _nombre.ActualHeight * 0.54;

            double x = -LadoCamion + p * (ancho + LadoCamion);                 // borde izquierdo del camión
            double rebote = Math.Sin(p * 80) * 1.5;                             // pequeño traqueteo
            Canvas.SetLeft(_camion, 0);
            Canvas.SetTop(_camion, centroLetras - LadoCamion / 2 + 8);
            var mueve = (TranslateTransform)_camion.RenderTransform;
            mueve.X = x;
            mueve.Y = rebote;

            // Carretera justo bajo las ruedas
            Canvas.SetLeft(_carretera, 0);
            _carretera.Width = ancho;
            Canvas.SetTop(_carretera, centroLetras + LadoCamion * 0.375 + 8);

            double frente = x + LadoCamion * 0.88;                              // frente del camión
            double izqMarca = _marca.TransformToAncestor(this).Transform(new Point(0, 0)).X;
            double revelado = p >= 1 ? _marca.ActualWidth : Math.Max(0, Math.Min(_marca.ActualWidth, frente - izqMarca));
            _recorte.Rect = new Rect(0, -20, revelado, _marca.ActualHeight + 40);
        }

        private void Terminar()
        {
            if (_terminada) return;
            _terminada = true;
            if (_pasada != null) _pasada.TrySetResult(false);
            OcultarPista();
            BeginAnimation(ProgresoProperty, null);

            var fundido = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
            fundido.Completed += (s, e) =>
            {
                if (!_terminada) return;               // se volvió a reproducir mientras se desvanecía
                Visibility = Visibility.Collapsed;
                var h = Terminada;
                if (h != null) h();
            };
            BeginAnimation(OpacityProperty, fundido);
        }
    }
}
