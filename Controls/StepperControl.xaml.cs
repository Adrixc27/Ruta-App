using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    /// <summary>Indicador de progreso del viaje. Uso: &lt;local:StepperControl Paso="1"/&gt;</summary>
    public partial class StepperControl : UserControl
    {
        public static readonly DependencyProperty PasoProperty =
            DependencyProperty.Register("Paso", typeof(int), typeof(StepperControl),
                new PropertyMetadata(1, (d, e) => ((StepperControl)d).Construir()));

        public int Paso
        {
            get { return (int)GetValue(PasoProperty); }
            set { SetValue(PasoProperty, value); }
        }

        private static readonly string[] Etiquetas = { "Planificar", "Comparar", "Viajar" };

        public StepperControl()
        {
            InitializeComponent();
            Construir();
        }

        private Brush B(string clave) { return (Brush)FindResource(clave); }

        private void Construir()
        {
            Raiz.Children.Clear();
            Raiz.ColumnDefinitions.Clear();

            // columnas: paso, conector, paso, conector, paso
            for (int i = 0; i < 5; i++)
            {
                Raiz.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = (i % 2 == 0) ? GridLength.Auto : new GridLength(1, GridUnitType.Star)
                });
            }

            for (int n = 1; n <= 3; n++)
            {
                bool activo = n == Paso;
                bool hecho = n < Paso;

                var circulo = new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(12),
                    BorderThickness = new Thickness(1),
                    Background = activo ? B("BrandPrimary") : B("BgWhite"),
                    BorderBrush = (activo || hecho) ? B("BrandPrimary") : B("BorderDefault")
                };

                if (hecho)
                {
                    circulo.Child = new Icon
                    {
                        Data = (Geometry)FindResource("IcoCheck"),
                        Width = 12,
                        Height = 12,
                        Foreground = B("BrandPrimary")
                    };
                }
                else
                {
                    circulo.Child = new TextBlock
                    {
                        Text = n.ToString(),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = activo ? Brushes.White : B("TextSecondary"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }

                var etiqueta = new TextBlock
                {
                    Text = Etiquetas[n - 1],
                    FontSize = 13,
                    FontWeight = activo ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = activo ? B("TextPrimary") : B("TextSecondary"),
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(circulo);
                panel.Children.Add(etiqueta);
                Grid.SetColumn(panel, (n - 1) * 2);
                Raiz.Children.Add(panel);

                if (n < 3)
                {
                    var conector = new Border
                    {
                        Height = 2,
                        CornerRadius = new CornerRadius(1),
                        Margin = new Thickness(8, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = hecho ? B("BrandPrimary") : B("BorderDefault")
                    };
                    Grid.SetColumn(conector, (n - 1) * 2 + 1);
                    Raiz.Children.Add(conector);
                }
            }
        }
    }
}
