using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ruta_App
{
    public partial class PlanificarView : UserControl, INavegable
    {
        public PlanificarView()
        {
            InitializeComponent();
            ListaRecientes.ItemsSource = Estado.Recientes;
            Mapa.BannerTexto("Visualizando mapa amplio de Chihuahua, Chih.");
            AlNavegar(null);
        }

        /// <summary>Se llama cada vez que se entra a esta pantalla: recarga lo que haya en Estado.</summary>
        public void AlNavegar(object parametro)
        {
            TxtOrigen.Text = Estado.Origen;
            TxtDestino.Text = Estado.Destino;
            TxtError.Visibility = Visibility.Collapsed;

            if (Estado.Prioridad == "Económico") PrioEconomico.IsChecked = true;
            else if (Estado.Prioridad == "Cómodo") PrioComodo.IsChecked = true;
            else PrioRapido.IsChecked = true;
        }

        private void Prioridad_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb != null && rb.Tag != null) Estado.Prioridad = rb.Tag.ToString();
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string destino = TxtDestino.Text.Trim();
            if (destino.Length == 0)
            {
                TxtError.Visibility = Visibility.Visible;
                TxtDestino.Enfocar();
                return;
            }

            TxtError.Visibility = Visibility.Collapsed;
            Estado.Origen = string.IsNullOrWhiteSpace(TxtOrigen.Text) ? "Mi ubicación actual" : TxtOrigen.Text.Trim();
            Estado.Destino = destino;

            var nav = Estado.Navegar;
            if (nav != null) nav("comparar", null);
        }

        // Clic en una ruta reciente → abre Rutas con esa ruta en el mapa
        private void Reciente_Click(object sender, MouseButtonEventArgs e)
        {
            var reciente = ((FrameworkElement)sender).DataContext as Reciente;
            if (reciente == null) return;
            var nav = Estado.Navegar;
            if (nav != null) nav("rutas", reciente.Ruta);
        }

        // La estrella guarda / quita la ruta de guardadas
        private void Estrella_Click(object sender, RoutedEventArgs e)
        {
            var reciente = ((FrameworkElement)sender).DataContext as Reciente;
            if (reciente != null) Estado.AlternarGuardada(reciente.Ruta);
        }
    }
}
