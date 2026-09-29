using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ruta_App
{
    public class CategoriaAjustes
    {
        public string Titulo { get; set; } = "";
        public List<string> Ajustes { get; set; } = new List<string>();
    }

    public partial class ConfiguracionView : UserControl
    {
        private readonly List<CategoriaAjustes> _categorias = new List<CategoriaAjustes>
        {
            new CategoriaAjustes { Titulo = "GENERAL", Ajustes = new List<string>
                { "Idioma de la aplicación", "Unidades de distancia (km / mi)", "Notificaciones push" } },
            new CategoriaAjustes { Titulo = "PREFERENCIAS DE TRANSPORTE", Ajustes = new List<string>
                { "Modo preferido (Metro, Autobús, Tren)", "Evitar transbordos complejos", "Priorizar rutas con accesibilidad" } },
            new CategoriaAjustes { Titulo = "SOPORTE", Ajustes = new List<string>
                { "Reportar un problema en la ruta", "Centro de ayuda", "Enviar comentarios" } },
            new CategoriaAjustes { Titulo = "LEGAL", Ajustes = new List<string>
                { "Términos de servicio", "Política de privacidad", "Sobre Nosotros" } }
        };

        public ConfiguracionView()
        {
            InitializeComponent();
            Filtrar();
        }

        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrar();
        }

        /// <summary>Muestra solo los ajustes que contienen el texto buscado (y oculta categorías vacías).</summary>
        private void Filtrar()
        {
            string q = Buscar.Text.Trim();

            var visibles = _categorias
                .Select(c => new CategoriaAjustes
                {
                    Titulo = c.Titulo,
                    Ajustes = c.Ajustes
                        .Where(a => q.Length == 0 ||
                                    a.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                    c.Titulo.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        .ToList()
                })
                .Where(c => c.Ajustes.Count > 0)
                .ToList();

            Categorias.ItemsSource = visibles;
            TxtSinResultados.Visibility = visibles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Ajuste_Click(object sender, RoutedEventArgs e)
        {
            string ajuste = ((FrameworkElement)sender).DataContext as string;
            if (ajuste == null) return;

            if (ajuste == "Sobre Nosotros")
            {
                MessageBox.Show("R.U.T.A. — Transporte público\nVersión 4.12.0 (escritorio)",
                                "Sobre Nosotros", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Aquí conectarás cada ajuste real (idioma, unidades, etc.)
            MessageBox.Show("«" + ajuste + "» estará disponible próximamente.",
                            "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLegal_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Los términos legales y licencias estarán disponibles próximamente.",
                            "R.U.T.A.", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
