using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Ruta_App
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Piezas visuales reutilizables (tarjetas, filas, interruptores...) para las pantallas
    //  de detalle de Configuración. Solo diseño: ningún control guarda ni cambia nada todavía.
    // ─────────────────────────────────────────────────────────────────────────
    internal static class Ui
    {
        public static Brush B(string k) { return (Brush)Application.Current.FindResource(k); }
        public static Geometry G(string k) { return (Geometry)Application.Current.FindResource(k); }
        public static Style S(string k) { return (Style)Application.Current.FindResource(k); }

        public static T Con<T>(this T e, double izq, double arr, double der, double abj) where T : FrameworkElement
        {
            e.Margin = new Thickness(izq, arr, der, abj);
            return e;
        }

        public static T Arriba<T>(this T e, double arr) where T : FrameworkElement
        {
            e.Margin = new Thickness(0, arr, 0, 0);
            return e;
        }

        public static TextBlock Txt(string texto, double tam = 14, string color = "TextPrimary", bool negrita = false, bool wrap = true)
        {
            return new TextBlock
            {
                Text = texto,
                FontSize = tam,
                FontWeight = negrita ? FontWeights.Bold : FontWeights.Normal,
                Foreground = B(color),
                TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        public static TextBlock Titulo(string texto, double tam = 19)
        {
            var t = Txt(texto, tam, "TextPrimary", true);
            t.FontWeight = FontWeights.Black;
            return t;
        }

        public static Icon Ico(string clave, double tam = 18, string color = "BrandPrimary")
        {
            return new Icon { Data = G(clave), Width = tam, Height = tam, Foreground = B(color), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
        }

        // ── Contenedores ──────────────────────────────────────────────────────
        public static Border Tarjeta(UIElement hijo, double relleno = 24)
        {
            return new Border
            {
                Background = B("BgCard"),
                BorderBrush = B("BorderDefault"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(relleno),
                Child = hijo,
                Effect = new DropShadowEffect { BlurRadius = 18, ShadowDepth = 3, Direction = 270, Opacity = 0.08, Color = Color.FromRgb(0x1B, 0x36, 0x5D) }
            };
        }

        public static StackPanel Encabezado(string titulo, string subtitulo)
        {
            var p = new StackPanel();
            p.Children.Add(Titulo(titulo));
            if (!string.IsNullOrEmpty(subtitulo)) p.Children.Add(Txt(subtitulo, 13, "TextSecondary").Arriba(6));
            return p;
        }

        /// <summary>Tarjeta con título, subtítulo y contenido debajo.</summary>
        public static Border TarjetaCon(string titulo, string subtitulo, params UIElement[] contenido)
        {
            var p = Encabezado(titulo, subtitulo);
            bool primero = true;
            foreach (var c in contenido)
            {
                var fe = c as FrameworkElement;
                if (fe != null && primero) fe.Margin = new Thickness(fe.Margin.Left, 20 + fe.Margin.Top, fe.Margin.Right, fe.Margin.Bottom);
                primero = false;
                p.Children.Add(c);
            }
            return Tarjeta(p);
        }

        public static StackPanel Pila(params UIElement[] hijos)
        {
            var p = new StackPanel();
            foreach (var h in hijos) p.Children.Add(h);
            return p;
        }

        /// <summary>Dos columnas: contenido principal a la izquierda y panel lateral de ancho fijo a la derecha.</summary>
        public static Grid Dos(UIElement principal, UIElement lateral, double anchoLateral)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(anchoLateral) });
            Grid.SetColumn(lateral, 1);
            ((FrameworkElement)principal).VerticalAlignment = VerticalAlignment.Top;
            ((FrameworkElement)lateral).VerticalAlignment = VerticalAlignment.Top;
            ((FrameworkElement)lateral).Margin = new Thickness(24, 0, 0, 0);
            g.Children.Add(principal);
            g.Children.Add(lateral);
            return g;
        }

        // ── Filas y controles ─────────────────────────────────────────────────
        public static Border CajaIcono(string icono, double lado = 38, bool relleno = false)
        {
            return new Border
            {
                Width = lado,
                Height = lado,
                CornerRadius = new CornerRadius(10),
                Background = relleno ? B("BrandPrimary") : B("BrandTint"),
                Child = Ico(icono, lado * 0.5, relleno ? "BgWhite" : "BrandPrimary"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        public static Border Interruptor(bool activo)
        {
            var perilla = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = Brushes.White,
                HorizontalAlignment = activo ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(3),
                Effect = new DropShadowEffect { BlurRadius = 3, ShadowDepth = 1, Opacity = 0.25 }
            };
            return new Border
            {
                Width = 44,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = activo ? B("BrandPrimary") : new SolidColorBrush(Color.FromRgb(0xB9, 0xD0, 0xEC)),
                Child = perilla,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
        }

        /// <summary>Fila con icono, título, descripción y un control a la derecha (interruptor, chevron...).</summary>
        public static Border FilaAjuste(string icono, string titulo, string desc, UIElement derecha, bool linea = true)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var ico = CajaIcono(icono);
            var texto = Pila(Txt(titulo, 14, "TextPrimary", true), Txt(desc, 12, "TextSecondary").Arriba(3));
            texto.Margin = new Thickness(14, 0, 14, 0);
            texto.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(texto, 1);
            Grid.SetColumn(derecha, 2);
            g.Children.Add(ico);
            g.Children.Add(texto);
            g.Children.Add(derecha);

            return new Border
            {
                Child = g,
                Padding = new Thickness(0, 12, 0, 12),
                BorderBrush = B("BorderDefault"),
                BorderThickness = new Thickness(0, 0, 0, linea ? 1 : 0)
            };
        }

        public static Border OpcionRadio(string titulo, string desc, bool seleccionada, string insignia = null)
        {
            var radio = new Grid { Width = 20, Height = 20, VerticalAlignment = VerticalAlignment.Center };
            radio.Children.Add(new Ellipse { Stroke = B("BrandPrimary"), StrokeThickness = 2 });
            if (seleccionada) radio.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = B("BrandPrimary") });

            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var texto = Pila(Txt(titulo, 14, "TextPrimary", true), Txt(desc, 12, "TextSecondary").Arriba(3));
            texto.Margin = new Thickness(14, 0, 14, 0);
            Grid.SetColumn(texto, 1);
            g.Children.Add(radio);
            g.Children.Add(texto);
            if (insignia != null)
            {
                var chip = Chip(insignia, "BrandPrimary", "BgWhite");
                Grid.SetColumn(chip, 2);
                g.Children.Add(chip);
            }

            return new Border
            {
                Child = g,
                Padding = new Thickness(16, 14, 16, 14),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(0, 0, 0, 12),
                Background = seleccionada ? B("BrandTint") : B("BgPanel"),
                BorderBrush = seleccionada ? B("BrandPrimary") : B("BorderDefault"),
                BorderThickness = new Thickness(seleccionada ? 2 : 1)
            };
        }

        public static Border Chip(string texto, string fondo, string color, double tam = 11)
        {
            return new Border
            {
                Background = B(fondo),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = Txt(texto, tam, color, true, false)
            };
        }

        /// <summary>Recuadro de aviso: tipo "info" (azul), "ok" (verde) o "alerta" (ámbar).</summary>
        public static Border Aviso(string icono, string titulo, string texto, string tipo = "info")
        {
            string fondo = tipo == "ok" ? "SuccessTint" : (tipo == "alerta" ? "WarningTint" : "BrandTint");
            string colorIco = tipo == "ok" ? "Success" : (tipo == "alerta" ? "Warning" : "BrandPrimary");

            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var ico = Ico(icono, 20, colorIco);
            ico.VerticalAlignment = VerticalAlignment.Top;
            ico.Margin = new Thickness(0, 1, 12, 0);
            g.Children.Add(ico);

            var p = new StackPanel();
            if (!string.IsNullOrEmpty(titulo)) p.Children.Add(Txt(titulo, 13, "TextPrimary", true));
            p.Children.Add(Txt(texto, 12, "TextSecondary").Arriba(string.IsNullOrEmpty(titulo) ? 0 : 3));
            Grid.SetColumn(p, 1);
            g.Children.Add(p);

            return new Border { Background = B(fondo), CornerRadius = new CornerRadius(10), Padding = new Thickness(16, 14, 16, 14), Child = g };
        }

        public static Border ParValor(string etiqueta, string valor, bool linea = true, bool recuadro = false)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var a = Txt(etiqueta, 13, "TextSecondary");
            var b = Txt(valor, 13, recuadro ? "BrandPrimary" : "TextPrimary", true);
            b.TextAlignment = TextAlignment.Right;
            Grid.SetColumn(b, 1);
            g.Children.Add(a);
            g.Children.Add(b);
            return new Border
            {
                Child = g,
                Padding = recuadro ? new Thickness(14, 12, 14, 12) : new Thickness(0, 11, 0, 11),
                Margin = recuadro ? new Thickness(0, 0, 0, 8) : new Thickness(0),
                CornerRadius = new CornerRadius(recuadro ? 8 : 0),
                Background = recuadro ? B("BgPanel") : null,
                BorderBrush = B("BorderDefault"),
                BorderThickness = new Thickness(0, 0, 0, linea && !recuadro ? 1 : 0)
            };
        }

        public static Button Boton(string texto, string icono = null, bool relleno = true)
        {
            var fila = new StackPanel { Orientation = Orientation.Horizontal };
            if (icono != null) fila.Children.Add(Ico(icono, 17, relleno ? "BgWhite" : "TextPrimary").Con(0, 0, 8, 0));
            fila.Children.Add(Txt(texto, 14, relleno ? "BgWhite" : "TextPrimary", true, false));
            var b = new Button { Content = fila, Style = S(relleno ? "BtnPrimary" : "BtnOutline"), Padding = new Thickness(18, 11, 18, 11), HorizontalAlignment = HorizontalAlignment.Left };
            return b;
        }

        public static StackPanel Etiquetado(string etiqueta, UIElement campo)
        {
            var p = new StackPanel();
            p.Children.Add(Txt(etiqueta, 12, "TextPrimary", true).Con(0, 0, 0, 6));
            p.Children.Add(campo);
            return p;
        }

        public static Border Desplegable(string icono, string texto)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.Children.Add(Ico(icono, 17, "TextSecondary").Con(0, 0, 10, 0));
            var t = Txt(texto, 13, "TextPrimary", false, false);
            Grid.SetColumn(t, 1);
            g.Children.Add(t);
            var ch = Ico("IcoChevronDown", 16, "TextPrimary");
            Grid.SetColumn(ch, 2);
            g.Children.Add(ch);
            return new Border
            {
                Child = g,
                Padding = new Thickness(14, 12, 14, 12),
                CornerRadius = new CornerRadius(10),
                Background = B("BgCard"),
                BorderBrush = B("BorderDefault"),
                BorderThickness = new Thickness(1)
            };
        }

        public static Border AreaTexto(string texto, string contador)
        {
            var g = new Grid { MinHeight = 96 };
            g.Children.Add(Txt(texto, 13, "TextPrimary").Con(0, 0, 0, 22));
            var c = Txt(contador, 11, "TextSecondary");
            c.HorizontalAlignment = HorizontalAlignment.Right;
            c.VerticalAlignment = VerticalAlignment.Bottom;
            g.Children.Add(c);
            return new Border
            {
                Child = g,
                Padding = new Thickness(14, 12, 14, 10),
                CornerRadius = new CornerRadius(10),
                Background = B("BgCard"),
                BorderBrush = B("BorderDefault"),
                BorderThickness = new Thickness(1)
            };
        }

        public static StackPanel CasillaTexto(string texto, bool marcada)
        {
            var caja = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(5),
                Background = marcada ? B("BrandPrimary") : B("BgCard"),
                BorderBrush = B("BrandPrimary"),
                BorderThickness = new Thickness(marcada ? 0 : 1.5),
                Child = marcada ? Ico("IcoCheck", 14, "BgWhite") : null,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (marcada) ((Icon)caja.Child).HorizontalAlignment = HorizontalAlignment.Center;
            var p = new StackPanel { Orientation = Orientation.Horizontal };
            p.Children.Add(caja);
            p.Children.Add(Txt(texto, 13, "TextPrimary", true, false).Con(10, 0, 0, 0));
            return p;
        }

        public static Border BarraInferior(string info, string textoBoton)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.Children.Add(Ico("IcoInfo", 18, "TextSecondary").Con(0, 0, 10, 0));
            var t = Txt(info, 13, "TextSecondary");
            Grid.SetColumn(t, 1);
            g.Children.Add(t);
            var b = Boton(textoBoton, "IcoCheck");
            b.Padding = new Thickness(20, 12, 20, 12);
            Grid.SetColumn(b, 2);
            g.Children.Add(b);
            return Tarjeta(g, 14);
        }

        /// <summary>Slider de solo diseño: pista, tramo relleno y perilla.</summary>
        public static Grid Deslizador(double proporcion)
        {
            var g = new Grid { Height = 22 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(proporcion, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - proporcion, GridUnitType.Star) });
            g.Children.Add(new Border { Height = 6, CornerRadius = new CornerRadius(3), Background = B("BgSidebar"), VerticalAlignment = VerticalAlignment.Center });
            var relleno = new Border { Height = 6, CornerRadius = new CornerRadius(3), Background = B("BrandPrimary"), VerticalAlignment = VerticalAlignment.Center };
            g.Children.Add(relleno);
            var perilla = new Ellipse
            {
                Width = 18, Height = 18, Fill = Brushes.White, Stroke = B("BrandPrimary"), StrokeThickness = 3,
                HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, -9, 0)
            };
            g.Children.Add(perilla);
            return g;
        }

        public static Border Casilla(string icono)
        {
            return CajaIcono(icono, 36);
        }

        // ── Marco común de las pantallas de detalle ───────────────────────────
        public static Grid Marco(string categoria, string titulo, string subtitulo, UIElement cuerpo, UIElement barra = null)
        {
            var g = new Grid { Margin = new Thickness(40, 30, 40, 14) };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Migas: ← Volver a Configuración / Categoría
            var migas = new StackPanel { Orientation = Orientation.Horizontal };
            var volver = new Button { Style = S("BtnGhost"), Padding = new Thickness(0, 2, 0, 2) };
            var vf = new StackPanel { Orientation = Orientation.Horizontal };
            vf.Children.Add(Ico("IcoArrowLeft", 15, "BrandPrimary").Con(0, 0, 8, 0));
            vf.Children.Add(Txt("Volver a Configuración", 13, "BrandPrimary", true, false));
            volver.Content = vf;
            volver.Click += (s, e) => { var nav = Estado.Navegar; if (nav != null) nav("config", null); };
            migas.Children.Add(volver);
            migas.Children.Add(Txt("/", 13, "TextSecondary", false, false).Con(12, 0, 12, 0));
            migas.Children.Add(Txt(categoria, 13, "TextSecondary", false, false));
            g.Children.Add(migas);

            // Título + chip "Cambios sincronizados"
            var fila = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var tit = Pila(Titulo(titulo, 30), Txt(subtitulo, 14, "TextSecondary").Arriba(8));
            fila.Children.Add(tit);
            var sync = new Border
            {
                Background = B("SuccessTint"), CornerRadius = new CornerRadius(14), Padding = new Thickness(12, 6, 14, 6),
                VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 4, 0, 0)
            };
            var sf = new StackPanel { Orientation = Orientation.Horizontal };
            sf.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = B("Success"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
            sf.Children.Add(Txt("Cambios sincronizados", 12, "Success", true, false));
            sync.Child = sf;
            Grid.SetColumn(sync, 1);
            fila.Children.Add(sync);
            Grid.SetRow(fila, 1);
            g.Children.Add(fila);

            // Cuerpo con scroll
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 22, 0, 0),
                Content = new Border { Child = cuerpo, Padding = new Thickness(0, 0, 4, 12) }
            };
            Grid.SetRow(scroll, 2);
            g.Children.Add(scroll);

            if (barra != null)
            {
                ((FrameworkElement)barra).Margin = new Thickness(0, 12, 0, 0);
                Grid.SetRow(barra, 3);
                g.Children.Add(barra);
            }

            // Pie
            var pie = new Grid { Margin = new Thickness(4, 18, 4, 0) };
            pie.Children.Add(Txt("RUTA v4.12.0 · Escritorio estable", 11, "TextSecondary", false, false));
            var ayuda = new Button { Style = S("BtnGhost"), Padding = new Thickness(0), HorizontalAlignment = HorizontalAlignment.Right };
            ayuda.Content = Txt("Ayuda y soporte", 11, "BrandPrimary", true, false);
            ayuda.Click += (s, e) => { var nav = Estado.Navegar; if (nav != null) nav("ajuste", "Centro de ayuda"); };
            pie.Children.Add(ayuda);
            Grid.SetRow(pie, 4);
            g.Children.Add(pie);

            return g;
        }

        // ── "Sobre Nosotros" (se usa en Configuración y en la página pública) ──
        public static UIElement ContenidoSobreNosotros(string ubicacion, double anchoLateral = 376)
        {
            // Tarjeta azul de misión
            var logo = new Border
            {
                Width = 54, Height = 54, CornerRadius = new CornerRadius(14), Background = Brushes.White, HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock { Text = "R", FontSize = 26, FontWeight = FontWeights.Black, Foreground = B("BrandPrimary"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
            };
            var mision = new StackPanel();
            mision.Children.Add(logo);
            var etiq = new TextBlock { Text = "NUESTRA MISIÓN", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.White, Opacity = 0.85, Margin = new Thickness(0, 14, 0, 0) };
            mision.Children.Add(etiq);
            mision.Children.Add(new TextBlock { Text = "Moverte por la ciudad debe ser claro, accesible y confiable.", FontSize = 25, FontWeight = FontWeights.Black, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) });
            mision.Children.Add(new TextBlock { Text = "RUTA reúne información de operadores, datos abiertos y reportes de la comunidad para ayudarte a tomar mejores decisiones de viaje.", FontSize = 14, Foreground = Brushes.White, Opacity = 0.92, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0), LineHeight = 24, LineStackingStrategy = LineStackingStrategy.BlockLineHeight });
            var tarjetaMision = new Border { Background = B("BrandPrimary"), CornerRadius = new CornerRadius(20), Padding = new Thickness(28), Child = mision };

            // Compromisos
            var tiles = new UniformGrid { Columns = 3 };
            Func<string, string, string, int, Border> tile = (ico, t, d, i) =>
            {
                var p = Pila(Ico(ico, 20), Txt(t, 13, "TextPrimary", true).Arriba(10), Txt(d, 12, "TextSecondary").Arriba(6));
                return new Border { Background = B("BgPanel"), CornerRadius = new CornerRadius(10), Padding = new Thickness(14), Margin = new Thickness(i == 0 ? 0 : 6, 0, i == 2 ? 0 : 6, 0), Child = p };
            };
            tiles.Children.Add(tile("IcoAccess", "Accesible", "Preferencias de movilidad incluidas desde el inicio.", 0));
            tiles.Children.Add(tile("IcoShield", "Responsable", "Privacidad y transparencia en cada decisión.", 1));
            tiles.Children.Add(tile("IcoUsers", "Colaborativa", "Información mejorada por toda la comunidad.", 2));
            var compromiso = TarjetaCon("Nuestro compromiso", "Diseñamos una herramienta pública y útil para cada forma de moverse.", tiles);
            compromiso.Margin = new Thickness(0, 16, 0, 0);

            var izquierda = Pila(tarjetaMision, compromiso);

            // Información técnica
            var version = TarjetaCon("RUTA para escritorio", "Información técnica de esta instalación.",
                Pila(ParValor("Versión", "4.12.0"),
                     ParValor("Compilación", "2026.10.01 · estable"),
                     ParValor("Datos de transporte", "Actualizados hace 4 min"),
                     ParValor("Licencia", "RUTA Community License")));
            var okFila = new StackPanel { Orientation = Orientation.Horizontal };
            okFila.Children.Add(Ico("IcoCheckCircle", 18, "Success").Con(0, 0, 10, 0));
            okFila.Children.Add(Txt("Tienes la versión más reciente", 13, "Success", true, false));
            var ok = new Border { Background = B("SuccessTint"), CornerRadius = new CornerRadius(10), Padding = new Thickness(14, 12, 14, 12), HorizontalAlignment = HorizontalAlignment.Left, Child = okFila, Margin = new Thickness(0, 18, 0, 0) };
            ((StackPanel)version.Child).Children.Add(ok);

            // Contacto
            var correo = new StackPanel { Orientation = Orientation.Horizontal };
            correo.Children.Add(Ico("IcoMail", 18).Con(0, 0, 10, 0));
            correo.Children.Add(Txt("hola@ruta.com", 13, "TextPrimary", true, false));
            var lugar = new StackPanel { Orientation = Orientation.Horizontal };
            lugar.Children.Add(Ico("IcoPin", 18).Con(0, 0, 10, 0));
            lugar.Children.Add(Txt(ubicacion, 13, "TextPrimary", false, false));
            var botones = new StackPanel { Orientation = Orientation.Horizontal };
            botones.Children.Add(Boton("Escribirnos", "IcoSend"));
            botones.Children.Add(Boton("Sitio web", "IcoExternal", false).Con(10, 0, 0, 0));
            var contacto = TarjetaCon("Contacto", "¿Tienes una propuesta, alianza o consulta institucional?",
                correo.Arriba(0), lugar.Arriba(8), botones.Arriba(14));
            contacto.Margin = new Thickness(0, 16, 0, 0);
            var derecha = Pila(version, contacto);

            return Dos(izquierda, derecha, anchoLateral);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Pantalla de detalle de un ajuste (Configuración → ...). Solo diseño.
    // ─────────────────────────────────────────────────────────────────────────
    public class AjusteDetalleView : UserControl, INavegable
    {
        public AjusteDetalleView()
        {
            Background = Ui.B("BgPanel");
        }

        /// <summary>parametro = nombre del ajuste tal como aparece en la lista de Configuración.</summary>
        public void AlNavegar(object parametro)
        {
            string ajuste = (parametro as string) ?? "";
            Func<string, bool> es = clave => ajuste.IndexOf(clave, StringComparison.OrdinalIgnoreCase) >= 0;

            UIElement pantalla;
            if (es("Notificaciones")) pantalla = Notificaciones();
            else if (es("Unidades")) pantalla = Unidades();
            else if (es("Idioma")) pantalla = Idioma();
            else if (es("Modo preferido")) pantalla = ModoPreferido();
            else if (es("transbordos")) pantalla = Transbordos();
            else if (es("accesibilidad")) pantalla = Accesibilidad();
            else if (es("Reportar")) pantalla = Reportar();
            else if (es("Centro de ayuda")) pantalla = CentroAyuda();
            else if (es("comentarios")) pantalla = Comentarios();
            else if (es("Términos")) pantalla = Terminos();
            else if (es("privacidad")) pantalla = Privacidad();
            else if (es("Sobre Nosotros")) pantalla = SobreNosotros();
            else pantalla = Ui.Marco("Configuración", ajuste, "Esta pantalla estará disponible próximamente.", new Border());

            Content = pantalla;
        }

        // ── General ───────────────────────────────────────────────────────────
        private static UIElement Notificaciones()
        {
            var maestro = new Border
            {
                Background = Ui.B("BrandTint"), CornerRadius = new CornerRadius(10), Padding = new Thickness(16, 14, 16, 14), Margin = new Thickness(0, 0, 0, 12),
                Child = FilaSimple("Permitir notificaciones push", "Activadas en este equipo", true)
            };
            var filas = Ui.Pila(
                maestro,
                Ui.FilaAjuste("IcoSend", "Alertas de ruta", "Cambios, retrasos y cancelaciones en rutas guardadas.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoAlert", "Incidencias cercanas", "Interrupciones y cierres relevantes en tu zona.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoClock", "Recordatorios de viaje", "Avisos 10 minutos antes de la hora de salida.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoSparkles", "Novedades de RUTA", "Funciones nuevas, consejos y comunicaciones del producto.", Ui.Interruptor(false)));
            var principal = Ui.TarjetaCon("Alertas y avisos", "Personaliza tus notificaciones. Las alertas críticas de seguridad siempre permanecerán activas.", filas);

            var lateral = Ui.TarjetaCon("Resumen", "Frecuencia estimada según tu actividad.",
                Ui.Pila(Ui.ParValor("Alertas activas", "3 de 4", true, true),
                        Ui.ParValor("Promedio semanal", "5 avisos", true, true),
                        Ui.ParValor("Horario silencioso", "23:00–07:00", true, true),
                        Ui.Aviso("IcoCheckCircle", "Permisos del sistema", "RUTA tiene permiso para mostrar avisos en este equipo.", "ok").Arriba(4)));

            return Ui.Marco("General", "Notificaciones push", "Controla qué alertas recibe tu cuenta y cuándo quieres recibirlas.",
                Ui.Dos(principal, lateral, 316), Ui.BarraInferior("Puedes cambiar estos ajustes en cualquier momento.", "Guardar preferencias"));
        }

        // Fila con título/descripción a la izquierda e interruptor a la derecha (sin icono)
        private static Grid FilaSimple(string titulo, string desc, bool activo)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.Children.Add(Ui.Pila(Ui.Txt(titulo, 14, "TextPrimary", true), Ui.Txt(desc, 12, "TextSecondary").Arriba(3)));
            var i = Ui.Interruptor(activo);
            Grid.SetColumn(i, 1);
            g.Children.Add(i);
            return g;
        }

        private static UIElement Unidades()
        {
            var principal = Ui.TarjetaCon("Sistema de medida", "Esta preferencia afecta los resultados, mapas y avisos de caminata.",
                Ui.OpcionRadio("Kilómetros", "Metros para tramos cortos y kilómetros para trayectos largos.", true, "Recomendado"),
                Ui.OpcionRadio("Millas", "Pies para tramos cortos y millas para trayectos largos.", false),
                Ui.Aviso("IcoRefresh", "Conversión automática", "Las distancias históricas se convertirán sin modificar tus rutas guardadas."));

            var total = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            total.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            total.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            total.Children.Add(Ui.Txt("Distancia total", 13, "TextPrimary", true));
            var km = Ui.Titulo("8,1 km", 20);
            Grid.SetColumn(km, 1);
            total.Children.Add(km);

            var lateral = Ui.TarjetaCon("Ejemplo de ruta", "Vista previa con kilómetros seleccionados.",
                Ui.Pila(Ui.ParValor("Caminar hasta Estación Norte", "450 m · 6 min", true, true),
                        Ui.ParValor("Viajar en Metro L2", "7,4 km · 18 min", true, true),
                        Ui.ParValor("Caminar al destino", "280 m · 4 min", true, true),
                        new Border { Height = 1, Background = Ui.B("BorderDefault"), Margin = new Thickness(0, 6, 0, 0) },
                        total));

            return Ui.Marco("General", "Unidades de distancia", "Define cómo se muestran las distancias durante la planificación y navegación.",
                Ui.Dos(principal, lateral, 356), Ui.BarraInferior("Los cambios se aplicarán a tus próximas rutas.", "Guardar unidad"));
        }

        private static UIElement Idioma()
        {
            var principal = Ui.TarjetaCon("Idioma de interfaz", "Tu idioma actual es Español. El cambio se aplicará en todos tus dispositivos.",
                Ui.OpcionRadio("Español", "Español (Latinoamérica)", true, "Actual"),
                Ui.OpcionRadio("English", "English (United States)", false),
                Ui.OpcionRadio("Português", "Português (Brasil)", false),
                Ui.Aviso("IcoInfo", "Contenido regional", "Los nombres de estaciones y calles se conservarán en su idioma oficial."));

            var proxima = new Border
            {
                Background = Ui.B("BgPanel"), CornerRadius = new CornerRadius(10), Padding = new Thickness(16, 14, 16, 14),
                Child = Ui.Pila(Ui.Txt("PRÓXIMA RUTA", 11, "BrandPrimary", true),
                                Ui.Titulo("Centro → Universidad", 17).Arriba(4),
                                Ui.Txt("Sale en 8 min · 3 transbordos", 13, "TextSecondary").Arriba(4))
            };
            var lateral = Ui.TarjetaCon("Vista previa", "Así verás los elementos principales de la aplicación.",
                Ui.Pila(proxima,
                        Ui.Txt("Formato de fecha: 1 de octubre de 2026", 12, "TextSecondary").Arriba(14),
                        Ui.Txt("Hora: 18:45 · Distancia: kilómetros", 12, "TextSecondary").Arriba(4)));

            return Ui.Marco("General", "Idioma de la aplicación", "Elige el idioma para menús, indicaciones y mensajes de RUTA.",
                Ui.Dos(principal, lateral, 356), Ui.BarraInferior("La app se recargará al guardar el nuevo idioma.", "Guardar idioma"));
        }

        // ── Preferencias de transporte ────────────────────────────────────────
        private static UIElement ModoPreferido()
        {
            Func<string, string, string, bool, int, Border> modo = (ico, titulo, desc, sel, i) =>
            {
                var radio = new Grid { Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
                radio.Children.Add(new Ellipse { Stroke = Ui.B("BrandPrimary"), StrokeThickness = 2 });
                if (sel) radio.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = Ui.B("BrandPrimary") });

                var cabecera = new Grid();
                cabecera.Children.Add(Ui.CajaIcono(ico, 48, sel));
                cabecera.Children.Add(radio);
                if (!sel) ((Border)cabecera.Children[0]).Background = Brushes.White;

                var contenido = Ui.Pila(cabecera,
                    Ui.Titulo(titulo, 20).Arriba(14),
                    Ui.Txt(desc, 13, "TextSecondary").Arriba(4),
                    (sel ? Ui.Chip("Preferido", "BrandPrimary", "BgWhite") : Ui.Chip("Alternativa", "BgSidebar", "TextSecondary")).Arriba(12));

                return new Border
                {
                    Child = contenido, MinHeight = 250, Padding = new Thickness(20), CornerRadius = new CornerRadius(14),
                    Margin = new Thickness(i == 0 ? 0 : 8, 0, i == 2 ? 0 : 8, 0),
                    Background = sel ? Ui.B("BrandTint") : Ui.B("BgPanel"),
                    BorderBrush = sel ? Ui.B("BrandPrimary") : Ui.B("BorderDefault"),
                    BorderThickness = new Thickness(sel ? 2 : 1)
                };
            };

            var tarjetas = new UniformGrid { Columns = 3 };
            tarjetas.Children.Add(modo("IcoTrain", "Metro", "Rápido y frecuente", true, 0));
            tarjetas.Children.Add(modo("IcoBus", "Autobús", "Mayor cobertura urbana", false, 1));
            tarjetas.Children.Add(modo("IcoTrain", "Tren", "Ideal para trayectos largos", false, 2));

            var principal = Ui.TarjetaCon("Elige tu modo principal", "La selección influye en el orden de los resultados, pero siempre mostraremos alternativas viables.",
                tarjetas,
                Ui.Aviso("IcoWand", "Preferencia inteligente", "Si tu modo preferido tiene demoras significativas, RUTA destacará una alternativa más rápida.").Arriba(18));

            return Ui.Marco("Preferencias de Transporte", "Modo preferido", "Indica qué transporte debe priorizar RUTA al comparar opciones similares.",
                principal, Ui.BarraInferior("Metro será priorizado cuando dos rutas tengan tiempos similares.", "Guardar modo"));
        }

        private static UIElement Transbordos()
        {
            var extremos = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            extremos.Children.Add(Ui.Txt("Ruta más rápida", 11, "TextSecondary", false, false));
            var der = Ui.Txt("Ruta más simple", 11, "TextSecondary", false, false);
            der.HorizontalAlignment = HorizontalAlignment.Right;
            extremos.Children.Add(der);

            var cabeza = new Grid { Margin = new Thickness(0, 14, 0, 12) };
            cabeza.Children.Add(Ui.Txt("Tiempo extra aceptable", 13, "TextPrimary", true, false));
            var hasta = Ui.Txt("Hasta 10 min", 13, "BrandPrimary", true, false);
            hasta.HorizontalAlignment = HorizontalAlignment.Right;
            cabeza.Children.Add(hasta);

            var principal = Ui.TarjetaCon("Preferencia de transbordos", "RUTA priorizará trayectos más sencillos, incluso si requieren algunos minutos extra.",
                Ui.FilaAjuste("IcoShuffle", "Evitar transbordos complejos", "Limita cambios con poco margen, pasillos largos o salidas a nivel de calle.", Ui.Interruptor(true)),
                cabeza, Ui.Deslizador(0.5), extremos,
                Ui.Aviso("IcoInfo", "Cómo funciona", "Nunca descartaremos la única ruta disponible. Esta preferencia reorganiza las opciones cuando existen alternativas.").Arriba(18));

            Func<string, string, string, bool, Border> opcion = (etq, titulo, detalle, sel) =>
            {
                var g = new Grid();
                g.Children.Add(Ui.Pila(Ui.Txt(etq, 11, sel ? "BrandPrimary" : "TextSecondary", true),
                                       Ui.Titulo(titulo, 16).Arriba(6),
                                       Ui.Txt(detalle, 12, "TextSecondary").Arriba(8)));
                if (sel)
                {
                    var c = Ui.Ico("IcoCheckCircle", 20);
                    c.HorizontalAlignment = HorizontalAlignment.Right;
                    c.VerticalAlignment = VerticalAlignment.Top;
                    g.Children.Add(c);
                }
                return new Border
                {
                    Child = g, Padding = new Thickness(16), CornerRadius = new CornerRadius(10), Margin = new Thickness(0, 0, 0, 12),
                    Background = sel ? Ui.B("BrandTint") : Ui.B("BgPanel"),
                    BorderBrush = sel ? Ui.B("BrandPrimary") : Ui.B("BorderDefault"),
                    BorderThickness = new Thickness(sel ? 1.5 : 0)
                };
            };
            var lateral = Ui.TarjetaCon("Comparación de ejemplo", "Centro → Parque Industrial",
                Ui.Pila(opcion("RECOMENDADA", "Metro L2 + Autobús 14", "42 min · 1 transbordo sencillo", true),
                        opcion("MÁS RÁPIDA", "Metro L1 + L3 + Autobús 8", "36 min · 3 transbordos", false)));

            return Ui.Marco("Preferencias de Transporte", "Evitar transbordos complejos", "Reduce conexiones apresuradas, recorridos confusos y cambios entre estaciones distantes.",
                Ui.Dos(principal, lateral, 356), Ui.BarraInferior("Los cambios se aplicarán a tus próximas rutas.", "Guardar preferencia"));
        }

        private static UIElement Accesibilidad()
        {
            var filas = Ui.Pila(
                Ui.FilaAjuste("IcoUpDown", "Ascensores disponibles", "Prioriza estaciones con ascensores operativos en ambos sentidos.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoAccess", "Rampas de acceso", "Busca paradas, estaciones y vehículos con acceso mediante rampa.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoXCircle", "Trayectos sin escaleras", "Evita rutas que requieran subir o bajar escaleras.", Ui.Interruptor(true)),
                Ui.FilaAjuste("IcoBuilding", "Estaciones accesibles", "Muestra solo estaciones con infraestructura accesible verificada.", Ui.Interruptor(true)));
            var principal = Ui.TarjetaCon("Necesidades de accesibilidad", "Activamos opciones seguras y verificadas siempre que estén disponibles.", filas);

            var perfil = new Border
            {
                Background = Ui.B("BrandTint"), CornerRadius = new CornerRadius(10), Padding = new Thickness(14),
                Child = FilaIconoTexto("IcoAccess", "Movilidad asistida", "4 criterios seleccionados")
            };
            var perfilCard = Ui.TarjetaCon("Perfil activo", "Tus preferencias se aplican a nuevas búsquedas.",
                Ui.Pila(perfil,
                        Ui.Aviso("IcoAlert", "Datos en tiempo real", "Te avisaremos si un ascensor reportado como necesario deja de estar disponible.", "alerta").Arriba(14)));

            var enlace = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };
            enlace.Children.Add(Ui.Txt("Conoce nuestros criterios de verificación", 12, "BrandPrimary", true, false));
            enlace.Children.Add(Ui.Ico("IcoArrowRight", 14).Con(6, 0, 0, 0));
            var info = Ui.TarjetaCon("Información de accesibilidad", "Los datos se actualizan con reportes de operadores y de la comunidad.", enlace);
            info.Margin = new Thickness(0, 16, 0, 0);

            return Ui.Marco("Preferencias de Transporte", "Priorizar rutas con accesibilidad", "Personaliza los criterios de movilidad que RUTA debe considerar al planificar.",
                Ui.Dos(principal, Ui.Pila(perfilCard, info), 316), Ui.BarraInferior("Las rutas accesibles pueden requerir más tiempo de viaje.", "Guardar accesibilidad"));
        }

        private static Grid FilaIconoTexto(string icono, string titulo, string desc)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.Children.Add(Ui.CajaIcono(icono, 44, true));
            var t = Ui.Pila(Ui.Txt(titulo, 14, "TextPrimary", true), Ui.Txt(desc, 12, "TextSecondary").Arriba(2));
            t.Margin = new Thickness(14, 0, 0, 0);
            Grid.SetColumn(t, 1);
            g.Children.Add(t);
            return g;
        }

        // ── Soporte ───────────────────────────────────────────────────────────
        private static UIElement Reportar()
        {
            var fila1 = new Grid();
            fila1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fila1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var tipo = Ui.Etiquetado("Tipo de incidencia", Ui.Desplegable("IcoAlert", "Retraso o servicio interrumpido"));
            tipo.Margin = new Thickness(0, 0, 8, 0);
            var fecha = Ui.Etiquetado("Fecha y hora", Ui.Desplegable("IcoCalendar", "Hoy, 18:25"));
            fecha.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(fecha, 1);
            fila1.Children.Add(tipo);
            fila1.Children.Add(fecha);

            var ruta = Ui.Pila(Ui.Etiquetado("Ruta o ubicación", Ui.Desplegable("IcoPin", "Metro L2 · Estación Universidad")),
                               Ui.Txt("Seleccionada desde tu viaje reciente de hoy.", 11, "TextSecondary").Arriba(6));

            var adjuntar = new Grid();
            adjuntar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            adjuntar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            adjuntar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            adjuntar.Children.Add(Ui.Ico("IcoPaperclip", 18).Con(0, 0, 10, 0));
            var a1 = Ui.Txt("Adjuntar captura o fotografía", 13, "BrandPrimary", true, false);
            Grid.SetColumn(a1, 1);
            adjuntar.Children.Add(a1);
            var a2 = Ui.Txt("PNG o JPG · máx. 10 MB", 11, "TextSecondary", false, false);
            Grid.SetColumn(a2, 2);
            adjuntar.Children.Add(a2);
            var adj = new Border { Child = adjuntar, Padding = new Thickness(16, 14, 16, 14), CornerRadius = new CornerRadius(10), Background = Ui.B("BgPanel"), BorderBrush = Ui.B("BorderDefault"), BorderThickness = new Thickness(1) };

            var principal = Ui.TarjetaCon("Detalles de la incidencia", "Los campos marcados son necesarios para enviar el reporte.",
                fila1,
                ruta.Arriba(14),
                Ui.Etiquetado("Descripción", Ui.AreaTexto("El tren permaneció detenido por más de 15 minutos y no hubo avisos en la estación. La conexión con el Autobús 14 se perdió.", "142 / 500")).Arriba(14),
                adj.Arriba(14),
                Ui.CasillaTexto("Permitir que soporte me contacte sobre este reporte", true).Arriba(14));

            Func<string, StackPanel> chequeo = t =>
            {
                var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
                p.Children.Add(Ui.Ico("IcoCheckCircle", 18, "Success").Con(0, 0, 10, 0));
                p.Children.Add(Ui.Txt(t, 13, "TextPrimary", false, false));
                return p;
            };
            var lateral = Ui.TarjetaCon("Antes de enviar", "Revisa que la información sea correcta.",
                Ui.Pila(chequeo("Tipo y hora confirmados"), chequeo("Ubicación verificada"), chequeo("Descripción incluida"),
                        Ui.Aviso("IcoInfo", "Privacidad", "No compartas datos personales de otros viajeros. Tu ubicación solo se usará para revisar esta incidencia.").Arriba(6),
                        Ui.Boton("Enviar reporte", "IcoSend").Arriba(16)));
            var pila = Ui.Pila(lateral, Ui.Txt("Tiempo de respuesta habitual: menos de 24 horas", 12, "TextSecondary").Arriba(12));

            return Ui.Marco("Soporte", "Reportar un problema en la ruta", "Cuéntanos qué ocurrió para ayudar a mejorar la información de viaje.",
                Ui.Dos(principal, pila, 316));
        }

        private static UIElement Comentarios()
        {
            var estrellas = new StackPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < 5; i++)
            {
                var e = Ui.Ico("IcoStar", 28);
                e.Relleno = i < 4 ? Ui.B("BrandPrimary") : null;
                e.Margin = new Thickness(0, 0, 12, 0);
                estrellas.Children.Add(e);
            }
            estrellas.Children.Add(Ui.Chip("4 de 5 · Muy buena", "BrandTint", "BrandPrimary", 12));

            var principal = Ui.TarjetaCon("Cuéntanos tu experiencia", "Leemos cada comentario y lo compartimos con el equipo responsable.",
                Ui.Etiquetado("¿Cómo valorarías RUTA?", estrellas),
                Ui.Etiquetado("Categoría", Ui.Desplegable("IcoRoute", "Planificación y comparación de rutas")).Arriba(16),
                Ui.Etiquetado("Comentario", Ui.AreaTexto("La comparación de rutas es muy clara. Me gustaría poder fijar una opción favorita para verla siempre al principio de los resultados.", "138 / 800")).Arriba(16),
                Ui.CasillaTexto("Pueden contactarme para ampliar este comentario", false).Arriba(16));

            Func<string, string, string, Grid> paso = (n, t, d) =>
            {
                var g = new Grid { Margin = new Thickness(0, 0, 0, 12) };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.Children.Add(new Border
                {
                    Width = 30, Height = 30, CornerRadius = new CornerRadius(15), Background = Ui.B("BrandTint"), VerticalAlignment = VerticalAlignment.Top,
                    Child = Ui.Txt(n, 12, "BrandPrimary", true, false).Con(0, 0, 0, 0)
                });
                ((TextBlock)((Border)g.Children[0]).Child).HorizontalAlignment = HorizontalAlignment.Center;
                var tx = Ui.Pila(Ui.Txt(t, 13, "TextPrimary", true), Ui.Txt(d, 11, "TextSecondary").Arriba(2));
                tx.Margin = new Thickness(12, 0, 0, 0);
                Grid.SetColumn(tx, 1);
                g.Children.Add(tx);
                return g;
            };
            var lateral = Ui.TarjetaCon("Lo que ocurre después", "Convertimos los comentarios en mejoras concretas.",
                Ui.Pila(paso("1", "Revisamos", "Clasificamos tu comentario por tema."),
                        paso("2", "Priorizamos", "Lo comparamos con otras necesidades."),
                        paso("3", "Mejoramos", "Incorporamos cambios en próximas versiones."),
                        Ui.Aviso("IcoHeart", "Gracias por participar", "No usamos tus comentarios con fines publicitarios.", "ok").Arriba(6),
                        Ui.Boton("Enviar comentario", "IcoSend").Arriba(16)));

            return Ui.Marco("Soporte", "Enviar comentarios", "Tu opinión nos ayuda a construir una experiencia de transporte más clara y útil.",
                Ui.Dos(principal, lateral, 316));
        }

        private static UIElement CentroAyuda()
        {
            var busqueda = new Grid();
            busqueda.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            busqueda.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            busqueda.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            busqueda.Children.Add(Ui.Ico("IcoSearch", 20, "TextSecondary").Con(0, 0, 12, 0));
            var ph = Ui.Txt("Buscar en el centro de ayuda...", 14, "TextSecondary", false, false);
            Grid.SetColumn(ph, 1);
            busqueda.Children.Add(ph);
            var k = Ui.Chip("⌘ K", "BgPanel", "TextSecondary", 11);
            Grid.SetColumn(k, 2);
            busqueda.Children.Add(k);
            var barra = Ui.Tarjeta(busqueda, 14);

            Func<string, string, string, Border> cat = (ico, t, d) =>
            {
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.Children.Add(Ui.CajaIcono(ico, 40));
                var tx = Ui.Pila(Ui.Txt(t, 14, "TextPrimary", true), Ui.Txt(d, 11, "TextSecondary").Arriba(2));
                tx.Margin = new Thickness(12, 0, 0, 0);
                Grid.SetColumn(tx, 1);
                g.Children.Add(tx);
                var ch = Ui.Ico("IcoChevronRight", 16);
                Grid.SetColumn(ch, 2);
                g.Children.Add(ch);
                return new Border { Child = g, Background = Ui.B("BgPanel"), CornerRadius = new CornerRadius(12), Padding = new Thickness(16, 14, 16, 14), Margin = new Thickness(0, 0, 12, 12) };
            };
            var cats = new UniformGrid { Columns = 2 };
            cats.Children.Add(cat("IcoRoute", "Planificar rutas", "Búsquedas, filtros y resultados"));
            cats.Children.Add(cat("IcoUser", "Cuenta y perfil", "Acceso, datos y dispositivos"));
            cats.Children.Add(cat("IcoBell", "Alertas y avisos", "Notificaciones e incidencias"));
            cats.Children.Add(cat("IcoAccess", "Accesibilidad", "Preferencias de movilidad"));
            var principal = Ui.TarjetaCon("Explora por categoría", "Guías breves para resolver las dudas más comunes.", cats);

            Func<string, Border> faq = t =>
            {
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.Children.Add(Ui.Txt(t, 13, "TextPrimary", false, false));
                var ch = Ui.Ico("IcoChevronRight", 15);
                Grid.SetColumn(ch, 1);
                g.Children.Add(ch);
                return new Border { Child = g, Padding = new Thickness(0, 12, 0, 12), BorderBrush = Ui.B("BorderDefault"), BorderThickness = new Thickness(0, 0, 0, 1) };
            };
            var preguntas = Ui.TarjetaCon("Preguntas frecuentes", "Respuestas consultadas esta semana.",
                Ui.Pila(faq("¿Cómo guardo una ruta frecuente?"), faq("¿Por qué cambió el tiempo estimado?"),
                        faq("¿Cómo reporto un ascensor fuera de servicio?"), faq("¿Puedo usar RUTA sin conexión?"),
                        faq("¿Cómo elimino mi historial de viajes?")));

            var ayuda = new Border
            {
                Background = Ui.B("BrandTint"), CornerRadius = new CornerRadius(14), Padding = new Thickness(18), Margin = new Thickness(0, 16, 0, 0),
                Child = Ui.Pila(Ui.Txt("¿Aún necesitas ayuda?", 13, "TextPrimary", true),
                                Ui.Txt("Nuestro equipo responde de 08:00 a 20:00.", 12, "TextSecondary").Arriba(2),
                                Ui.Boton("Contactar soporte", "IcoChat", false).Arriba(12))
            };

            var cuerpo = Ui.Pila(barra, Ui.Dos(principal, Ui.Pila(preguntas, ayuda), 356).Arriba(16));
            return Ui.Marco("Soporte", "Centro de ayuda", "Encuentra respuestas rápidas o ponte en contacto con nuestro equipo.", cuerpo);
        }

        // ── Legal ─────────────────────────────────────────────────────────────
        private static UIElement NavegadorDocumento(string titulo, string subtitulo, string[] secciones, string boton)
        {
            var lista = new StackPanel { Margin = new Thickness(0, 14, 0, 0) };
            for (int i = 0; i < secciones.Length; i++)
            {
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.Children.Add(Ui.Txt(secciones[i], 13, i == 0 ? "BrandPrimary" : "TextPrimary", true, false));
                if (i == 0)
                {
                    var ch = Ui.Ico("IcoChevronRight", 15);
                    Grid.SetColumn(ch, 1);
                    g.Children.Add(ch);
                }
                lista.Children.Add(new Border
                {
                    Child = g, Padding = new Thickness(12, 9, 12, 9), CornerRadius = new CornerRadius(8), Margin = new Thickness(0, 0, 0, 4),
                    Background = i == 0 ? Ui.B("BrandTint") : null
                });
            }
            var p = Ui.Pila(Ui.Titulo(titulo, 19), Ui.Txt(subtitulo, 13, "TextSecondary").Arriba(6), lista);
            if (boton != null) p.Children.Add(Ui.Boton(boton, "IcoShield", false).Arriba(12));
            return Ui.Tarjeta(p, 24);
        }

        private static UIElement Documento(string etiqueta, params UIElement[] bloques)
        {
            var p = new StackPanel();
            p.Children.Add(Ui.Txt(etiqueta, 11, "BrandPrimary", true));
            foreach (var b in bloques) p.Children.Add(b);
            return Ui.Tarjeta(p, 24);
        }

        private static UIElement H(string t, double tam = 16) { return Ui.Titulo(t, tam).Arriba(14); }
        private static UIElement P(string t) { var x = Ui.Txt(t, 13, "TextSecondary"); x.LineHeight = 22; x.LineStackingStrategy = LineStackingStrategy.BlockLineHeight; return x.Arriba(6); }

        private static UIElement Privacidad()
        {
            Func<string, string, string, int, Border> tile = (ico, t, d, i) => new Border
            {
                Background = Ui.B("BgPanel"), CornerRadius = new CornerRadius(10), Padding = new Thickness(14), Margin = new Thickness(i == 0 ? 0 : 6, 0, i == 2 ? 0 : 6, 0),
                Child = Ui.Pila(Ui.Ico(ico, 20), Ui.Txt(t, 13, "TextPrimary", true).Arriba(8), Ui.Txt(d, 11, "TextSecondary").Arriba(4))
            };
            var tiles = new UniformGrid { Columns = 3, Margin = new Thickness(0, 16, 0, 0) };
            tiles.Children.Add(tile("IcoPin", "Ubicación", "Solo mientras buscas o navegas.", 0));
            tiles.Children.Add(tile("IcoRoute", "Viajes", "Rutas guardadas e historial opcional.", 1));
            tiles.Children.Add(tile("IcoBell", "Preferencias", "Alertas, accesibilidad e idioma.", 2));

            var doc = Documento("PRIVACIDAD POR DISEÑO",
                Ui.Titulo("Resumen de privacidad", 22).Arriba(4),
                P("RUTA utiliza la información mínima necesaria para planificar trayectos, personalizar alertas y mantener segura tu cuenta. No vendemos tus datos personales."),
                tiles,
                H("Datos que recopilamos"),
                P("Recopilamos datos de cuenta, preferencias, consultas de ruta e información técnica del dispositivo. El historial de ubicación precisa solo se guarda si activas “Historial de viajes”."),
                Ui.Aviso("IcoLocate", "Ubicación y permisos", "Puedes revocar el acceso a ubicación, notificaciones y cámara desde Configuración. RUTA seguirá funcionando con opciones limitadas.").Arriba(16),
                H("Tus derechos"),
                P("Puedes acceder, corregir, descargar o eliminar tus datos. Las solicitudes se atienden en un máximo de 30 días y pueden requerir verificación de identidad."));

            var nav = NavegadorDocumento("En esta política", "Vigente desde el 18 de septiembre de 2026",
                new[] { "Resumen", "Datos que recopilamos", "Uso de la información", "Ubicación y permisos", "Conservación", "Tus derechos", "Contacto" },
                "Gestionar mis datos");

            return Ui.Marco("Legal", "Política de privacidad", "Explicamos con claridad qué datos usamos, por qué los necesitamos y cómo los protegemos.",
                IzquierdaDerecha(nav, doc));
        }

        private static UIElement Terminos()
        {
            var doc = Documento("DOCUMENTO VIGENTE",
                Ui.Titulo("1. Aceptación de los términos", 22).Arriba(4),
                P("Al acceder o utilizar RUTA aceptas estos Términos de servicio. Si no estás de acuerdo con alguna condición, no utilices la aplicación."),
                H("2. Uso del servicio"),
                P("RUTA ofrece información para planificar viajes en transporte público. Debes usar el servicio de forma lícita, mantener actualizada la información de tu cuenta y evitar actividades que interfieran con su funcionamiento."),
                H("3. Información de rutas y horarios"),
                P("Los horarios, tiempos estimados y avisos proceden de operadores y fuentes colaboradoras. Trabajamos para ofrecer información precisa, pero puede cambiar por tráfico, clima, obras o decisiones operativas."),
                Ui.Aviso("IcoAlert", null, "Verifica los avisos oficiales antes de realizar viajes críticos o conexiones con horario limitado.", "alerta").Arriba(16),
                H("4. Cuenta y seguridad"),
                P("Eres responsable de proteger tus credenciales y de notificarnos cualquier acceso no autorizado. Podemos suspender cuentas que incumplan estos términos o comprometan la seguridad del servicio."));

            var nav = NavegadorDocumento("Contenido", "Última actualización: 18 de septiembre de 2026",
                new[] { "1. Aceptación", "2. Uso del servicio", "3. Información de rutas", "4. Cuenta y seguridad", "5. Propiedad intelectual", "6. Limitación de responsabilidad", "7. Contacto" },
                null);

            return Ui.Marco("Legal", "Términos de servicio", "Condiciones que regulan el uso de RUTA y sus servicios de planificación.",
                IzquierdaDerecha(nav, doc));
        }

        // Navegador a la izquierda (ancho fijo) y documento a la derecha
        private static UIElement IzquierdaDerecha(UIElement izquierda, UIElement derecha)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ((FrameworkElement)izquierda).VerticalAlignment = VerticalAlignment.Top;
            ((FrameworkElement)derecha).Margin = new Thickness(24, 0, 0, 0);
            ((FrameworkElement)derecha).VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(derecha, 1);
            g.Children.Add(izquierda);
            g.Children.Add(derecha);
            return g;
        }

        private static UIElement SobreNosotros()
        {
            return Ui.Marco("Legal", "Sobre Nosotros", "Conoce la misión, el equipo y la versión actual de RUTA.",
                Ui.ContenidoSobreNosotros("Chihuahua, CHIH"));
        }
    }
}
