using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruta_App
{
    /// <summary>
    /// Página pública "Sobre Nosotros" (Figma: sobre nosotros). Se abre desde la landing page, sin iniciar sesión.
    /// </summary>
    public class SobreNosotrosView : UserControl
    {
        public event Action Inicio;
        public event Action VerRutas;

        public SobreNosotrosView()
        {
            var raiz = new Grid { Background = Ui.B("BgPanel") };
            raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // ── Encabezado ──
            var cab = new Grid { Background = Ui.B("BgPage"), Height = 80 };
            var marca = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(88, 0, 0, 0) };
            marca.Children.Add(new Border
            {
                Width = 40, Height = 40, CornerRadius = new CornerRadius(10), Background = Brushes.White,
                Child = new TextBlock { Text = "R", FontSize = 20, FontWeight = FontWeights.Black, Foreground = Ui.B("BrandPrimary"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
            });
            var nombre = new StackPanel { Margin = new Thickness(12, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            nombre.Children.Add(new TextBlock { Text = "RUTA", FontSize = 20, FontWeight = FontWeights.Black, Foreground = Ui.B("BrandPrimary") });
            nombre.Children.Add(new TextBlock { Text = "TRANSPORTE PÚBLICO", FontSize = 10, FontWeight = FontWeights.SemiBold, Foreground = Ui.B("BrandPrimary"), Opacity = 0.85 });
            marca.Children.Add(nombre);
            cab.Children.Add(marca);

            var nav = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 112, 0) };
            var rutas = new Button { Content = "Rutas", Style = Ui.S("BtnGhost"), Foreground = Ui.B("TextPrimary"), FontWeight = FontWeights.SemiBold, Padding = new Thickness(14, 8, 14, 8), Margin = new Thickness(0, 0, 16, 0) };
            rutas.Click += (s, e) => { var h = VerRutas; if (h != null) h(); };
            var inicio = new Button { Content = "Inicio", Style = Ui.S("BtnPrimary"), FontSize = 14, Padding = new Thickness(16, 9, 16, 9) };
            inicio.Click += (s, e) => { var h = Inicio; if (h != null) h(); };
            nav.Children.Add(rutas);
            nav.Children.Add(inicio);
            cab.Children.Add(nav);
            raiz.Children.Add(cab);

            // ── Cuerpo ──
            var cuerpo = new StackPanel { Margin = new Thickness(112, 36, 112, 24) };
            var titulo = Ui.Titulo("Sobre Nosotros", 40);
            titulo.HorizontalAlignment = HorizontalAlignment.Center;
            cuerpo.Children.Add(titulo);
            var sub = Ui.Txt("Conoce la misión, el equipo y los valores detrás de RUTA.", 15, "TextPrimary");
            sub.HorizontalAlignment = HorizontalAlignment.Center;
            sub.Margin = new Thickness(0, 10, 0, 28);
            cuerpo.Children.Add(sub);
            cuerpo.Children.Add(Ui.ContenidoSobreNosotros("Chihuahua, CHIH", 512));

            var pie = new Grid { Margin = new Thickness(4, 40, 4, 0) };
            pie.Children.Add(Ui.Txt("RUTA v4.12.0 · Escritorio estable", 11, "TextPrimary", false, false));
            var ayuda = Ui.Txt("Ayuda y soporte", 11, "BrandPrimary", true, false);
            ayuda.HorizontalAlignment = HorizontalAlignment.Right;
            pie.Children.Add(ayuda);
            cuerpo.Children.Add(pie);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = cuerpo };
            Grid.SetRow(scroll, 1);
            raiz.Children.Add(scroll);

            Content = raiz;
        }
    }
}
