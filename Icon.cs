using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    /// <summary>
    /// Icono vectorial. Uso:  &lt;local:Icon Data="{StaticResource IcoMap}" Width="20" Height="20"/&gt;
    /// El trazo toma el Foreground del padre (así el icono cambia de color junto con el texto).
    /// </summary>
    public class Icon : Control
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Geometry), typeof(Icon), new PropertyMetadata(null));

        public static readonly DependencyProperty RellenoProperty =
            DependencyProperty.Register("Relleno", typeof(Brush), typeof(Icon), new PropertyMetadata(null));

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>Color de relleno opcional (estrella favorita, marcador guardado, corazón...).</summary>
        public Brush Relleno
        {
            get { return (Brush)GetValue(RellenoProperty); }
            set { SetValue(RellenoProperty, value); }
        }
    }
}
