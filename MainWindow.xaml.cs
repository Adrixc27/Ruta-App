using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ruta_App
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // Botón heredado del XAML original (si aún lo tienes referenciado)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // placeholder — puedes eliminar este botón del XAML si ya no lo usas
        }

        // Botón principal del MVP
        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtOrigen.Text.Trim();
            string destino = TxtDestino.Text.Trim();

            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                MessageBox.Show("Introduce origen y destino.", "R.U.T.A.",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mostrar tarjetas y ocultar placeholder
            PlaceholderCard.Visibility = Visibility.Collapsed;
            Ruta1Card.Visibility = Visibility.Visible;
            Ruta2Card.Visibility = Visibility.Visible;
            Ruta3Card.Visibility = Visibility.Visible;

            // Actualizar header
            LblHeader.Text = $"{origen}  →  {destino}";
            LblConteoRutas.Text = "3 rutas encontradas";

            // Métricas de la ruta recomendada (Ruta 1 por defecto)
            LblTiempo.Text = "28 min";
            LblTransbordos.Text = "1";
            LblCaminata.Text = "350 m";
            LblBus.Text = "L-2 / L-7";
        }

    }
}
