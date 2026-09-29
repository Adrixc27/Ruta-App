using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    /// <summary>
    /// TextBox con icono (o punto de color), placeholder y borde azul al enfocar.
    /// Uso: &lt;local:CampoTexto Icono="{StaticResource IcoMail}" Hint="tu@correo.com"/&gt;
    /// </summary>
    public partial class CampoTexto : UserControl
    {
        public event TextChangedEventHandler TextChanged;

        public CampoTexto()
        {
            InitializeComponent();
            Caja.TextChanged += (s, e) =>
            {
                Placeholder.Visibility = Caja.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
                var h = TextChanged;
                if (h != null) h(this, e);
            };
        }

        public string Text
        {
            get { return Caja.Text; }
            set { Caja.Text = value ?? ""; }
        }

        public string Hint
        {
            set { Placeholder.Text = value; }
        }

        /// <summary>Icono a la izquierda (una Geometry de Estilos.xaml).</summary>
        public Geometry Icono
        {
            set { Ico.Data = value; Ico.Visibility = Visibility.Visible; Dot.Visibility = Visibility.Collapsed; }
        }

        /// <summary>Si se define, en vez de icono se muestra un punto de este color (origen / destino).</summary>
        public Brush Punto
        {
            set { Dot.Fill = value; Dot.Visibility = Visibility.Visible; Ico.Visibility = Visibility.Collapsed; }
        }

        public CornerRadius Redondeo
        {
            set { Marco.CornerRadius = value; }
        }

        public Brush Fondo
        {
            set { Marco.Background = value; }
        }

        /// <summary>Altura mínima del campo (por defecto 46). Los buscadores usan 38–42.</summary>
        public double Alto
        {
            set
            {
                Marco.MinHeight = value;
                double v = Math.Max(0, (value - 22) / 2);
                Caja.Padding = new Thickness(0, v, 0, v);
            }
        }

        public void Enfocar()
        {
            Caja.Focus();
        }
    }
}
