using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ruta_App
{
    public partial class PlanificarView : UserControl, INavegable
    {
        private const string MiUbicacion = "Mi ubicación actual";
        private const int MaxParadasIntermedias = 3;

        public PlanificarView()
        {
            InitializeComponent();
            ListaRecientes.ItemsSource = Estado.Recientes;
            Mapa.BannerTexto("Visualizando mapa amplio de Chihuahua, Chih.");
            TxtOrigen.TextChanged += (s, e) => ProgramarBusqueda("origen", TxtOrigen);
            TxtDestino.TextChanged += (s, e) => ProgramarBusqueda("destino", TxtDestino);
            TxtOrigen.LostFocus += Campo_PerdioFoco;
            TxtDestino.LostFocus += Campo_PerdioFoco;
            AlNavegar(null);
            ProgramarBusqueda("origen", TxtOrigen);   // marca "Mi ubicación actual" al abrir
        }

        // ── Campos: origen, paradas intermedias (opcionales) y destino ────────
        private readonly Dictionary<string, CampoTexto> _escalas = new Dictionary<string, CampoTexto>();   // id → campo
        private readonly List<string> _ordenEscalas = new List<string>();                                  // ids en orden de pantalla
        private int _contadorEscalas;

        private CampoTexto CampoDe(string tipo)
        {
            if (tipo == "origen") return TxtOrigen;
            if (tipo == "destino") return TxtDestino;
            CampoTexto c;
            return _escalas.TryGetValue(tipo, out c) ? c : null;
        }

        private void BtnAnadirParada_Click(object sender, RoutedEventArgs e)
        {
            if (_escalas.Count >= MaxParadasIntermedias) return;

            string id = "escala" + (++_contadorEscalas);
            var campo = new CampoTexto
            {
                Punto = (System.Windows.Media.Brush)FindResource("TextSecondary"),
                Fondo = (System.Windows.Media.Brush)FindResource("BgCard"),
                Hint = "Parada intermedia"
            };
            campo.TextChanged += (s, a) => ProgramarBusqueda(id, campo);
            campo.LostFocus += Campo_PerdioFoco;

            var quitar = new Button
            {
                Content = "✕",
                Style = (Style)FindResource("BtnIcon"),
                ToolTip = "Quitar esta parada",
                Margin = new Thickness(6, 0, 0, 0)
            };
            quitar.Click += (s, a) => QuitarParada(id);

            var fila = new Grid { Margin = new Thickness(0, 0, 0, 12), Tag = id };
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(quitar, 1);
            fila.Children.Add(campo);
            fila.Children.Add(quitar);

            PanelEscalas.Children.Add(fila);
            _escalas[id] = campo;
            _ordenEscalas.Add(id);
            BtnAnadirParada.Visibility = _escalas.Count >= MaxParadasIntermedias ? Visibility.Collapsed : Visibility.Visible;
            campo.Enfocar();
        }

        private void QuitarParada(string id)
        {
            var fila = PanelEscalas.Children.OfType<Grid>().FirstOrDefault(g => (g.Tag as string) == id);
            if (fila != null) PanelEscalas.Children.Remove(fila);

            DispatcherTimer t;
            if (_temporizadores.TryGetValue(id, out t)) { t.Stop(); _temporizadores.Remove(id); }
            _escalas.Remove(id);
            _ordenEscalas.Remove(id);
            _elegido.Remove(id);
            Mapa.QuitarUbicacion(id);
            Mapa.LimpiarSugerencias();
            PopSug.IsOpen = false;
            BtnAnadirParada.Visibility = Visibility.Visible;
        }

        private void LimpiarParadas()
        {
            foreach (var id in _ordenEscalas.ToList()) QuitarParada(id);
        }

        // ── Ubicaciones en el mapa ────────────────────────────────────────────
        private readonly Dictionary<string, DispatcherTimer> _temporizadores = new Dictionary<string, DispatcherTimer>();
        private bool _ignorar;            // true mientras el código (no el usuario) cambia el texto
        private string _tipoActivo = "";  // a qué campo pertenecen las sugerencias abiertas
        private readonly Dictionary<string, Sugerencia> _elegido = new Dictionary<string, Sugerencia>();   // lugar elegido por campo

        // Etiqueta que lleva el marcador de una parada intermedia en el mapa (1, 2, 3...)
        private string EtiquetaMarcador(string tipo)
        {
            int i = _ordenEscalas.IndexOf(tipo);
            return i >= 0 ? (i + 1).ToString() : "";
        }

        // Espera a que el usuario deje de escribir (700 ms) y entonces busca lugares parecidos
        private void ProgramarBusqueda(string tipo, CampoTexto campo)
        {
            if (_ignorar) return;
            _elegido.Remove(tipo);

            DispatcherTimer t;
            if (!_temporizadores.TryGetValue(tipo, out t))
            {
                t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
                t.Tick += async (s, e) =>
                {
                    t.Stop();
                    await BuscarAsync(tipo, campo);
                };
                _temporizadores[tipo] = t;
            }
            t.Stop();
            t.Start();
        }

        private async System.Threading.Tasks.Task BuscarAsync(string tipo, CampoTexto campo)
        {
            string texto = campo.Text.Trim();

            if (texto.Length == 0)
            {
                Mapa.QuitarUbicacion(tipo);
                Mapa.LimpiarSugerencias();
                PopSug.IsOpen = false;
                return;
            }

            // "Mi ubicación actual": se marca la posición real del equipo
            if (tipo == "origen" && texto.Equals(MiUbicacion, StringComparison.OrdinalIgnoreCase))
            {
                Mapa.LimpiarSugerencias();
                PopSug.IsOpen = false;
                await Mapa.MostrarMiUbicacionAsync();
                return;
            }

            var lista = await Mapa.BuscarSugerenciasAsync(tipo, texto);
            if (lista == null) return;   // el usuario siguió escribiendo, o falló la búsqueda

            var items = new List<Sugerencia>(lista);
            if (tipo == "origen")
                items.Insert(0, new Sugerencia { Numero = "◎", Nombre = MiUbicacion, Detalle = "Usar la ubicación de este equipo", Lat = double.NaN });

            if (items.Count == 0) { PopSug.IsOpen = false; return; }

            _tipoActivo = tipo;
            ListaSug.ItemsSource = items;
            PopSug.PlacementTarget = campo;
            PopSug.Width = campo.ActualWidth;
            PopSug.IsOpen = true;
        }

        // El usuario elige una de las sugerencias: se escribe en el campo y se marca en el mapa
        private async void Sugerencia_Click(object sender, MouseButtonEventArgs e)
        {
            var s = ((FrameworkElement)sender).DataContext as Sugerencia;
            var campo = CampoDe(_tipoActivo);
            if (s == null || campo == null) return;

            _ignorar = true;
            campo.Text = s.Nombre;
            _ignorar = false;
            PopSug.IsOpen = false;

            if (double.IsNaN(s.Lat)) { _elegido.Remove(_tipoActivo); await Mapa.MostrarMiUbicacionAsync(); }
            else { _elegido[_tipoActivo] = s; Mapa.ElegirUbicacion(_tipoActivo, s.Nombre, s.Lat, s.Lng, EtiquetaMarcador(_tipoActivo)); }
        }

        private void Campo_PerdioFoco(object sender, RoutedEventArgs e)
        {
            // Pequeña espera para que el clic en una sugerencia alcance a registrarse
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            t.Tick += (s, a) =>
            {
                t.Stop();
                bool enfocado = TxtOrigen.IsKeyboardFocusWithin || TxtDestino.IsKeyboardFocusWithin ||
                                _escalas.Values.Any(c => c.IsKeyboardFocusWithin);
                if (!enfocado && !ListaSug.IsMouseOver) PopSug.IsOpen = false;
            };
            t.Start();
        }

        /// <summary>Se llama cada vez que se entra a esta pantalla: recarga lo que haya en Estado.</summary>
        public void AlNavegar(object parametro)
        {
            _ignorar = true;   // no buscar de nuevo solo por volver a esta pantalla
            TxtOrigen.Text = Estado.Origen;
            TxtDestino.Text = Estado.Destino;
            _ignorar = false;
            TxtError.Visibility = Visibility.Collapsed;
            PopSug.IsOpen = false;

            // Si la búsqueda anterior ya terminó (o se vació el destino), no se deja nada marcado en el mapa
            if (string.IsNullOrWhiteSpace(Estado.Destino))
            {
                _elegido.Remove("destino");
                Mapa.QuitarUbicacion("destino");
                Mapa.LimpiarSugerencias();
            }
            if (Estado.Escalas.Count == 0 && _escalas.Count > 0) LimpiarParadas();
        }

        // ── Buscar ────────────────────────────────────────────────────────────
        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            PopSug.IsOpen = false;
            string destino = TxtDestino.Text.Trim();
            if (destino.Length == 0)
            {
                TxtError.Text = "Escribe un destino para continuar.";
                TxtError.Visibility = Visibility.Visible;
                TxtDestino.Enfocar();
                return;
            }

            BtnBuscar.IsEnabled = false;
            try
            {
                var o = await ResolverAsync("origen", TxtOrigen);
                if (o == null)
                {
                    Error("No pudimos ubicar tu origen. Escribe una dirección o activa la ubicación de Windows.");
                    return;
                }

                // Paradas intermedias, en el orden en que están en pantalla (las vacías se ignoran)
                var escalas = new List<Sugerencia>();
                foreach (var id in _ordenEscalas)
                {
                    var campo = _escalas[id];
                    string texto = campo.Text.Trim();
                    if (texto.Length == 0) continue;
                    var p = await ResolverAsync(id, campo);
                    if (p == null) { Error("No encontramos la parada \"" + texto + "\". Prueba con más detalle."); return; }
                    escalas.Add(new Sugerencia { Nombre = texto, Lat = p[0], Lng = p[1] });
                }

                var d = await ResolverAsync("destino", TxtDestino);
                if (d == null)
                {
                    Error("No encontramos ese destino. Prueba con más detalle (calle, colonia).");
                    return;
                }

                TxtError.Visibility = Visibility.Collapsed;
                Estado.Origen = string.IsNullOrWhiteSpace(TxtOrigen.Text) ? MiUbicacion : TxtOrigen.Text.Trim();
                Estado.Destino = destino;
                Estado.Escalas = escalas;
                Estado.OrigenLat = o[0]; Estado.OrigenLng = o[1];
                Estado.DestinoLat = d[0]; Estado.DestinoLng = d[1];

                var nav = Estado.Navegar;
                if (nav != null) nav("comparar", null);
            }
            finally
            {
                BtnBuscar.IsEnabled = true;
            }
        }

        private void Error(string mensaje)
        {
            TxtError.Text = mensaje;
            TxtError.Visibility = Visibility.Visible;
        }

        /// <summary>Coordenadas [lat, lng] de lo que hay escrito en el campo, o null si no se pudo ubicar.</summary>
        private async System.Threading.Tasks.Task<double[]> ResolverAsync(string tipo, CampoTexto campo)
        {
            string texto = campo.Text.Trim();

            if (tipo == "origen" && (texto.Length == 0 || texto.Equals(MiUbicacion, StringComparison.OrdinalIgnoreCase)))
            {
                await Mapa.MostrarMiUbicacionAsync();
                return Mapa.MiPosicion;
            }

            Sugerencia s;
            if (_elegido.TryGetValue(tipo, out s) && s.Nombre == texto)
                return new[] { s.Lat, s.Lng };

            // No eligió de la lista: se toma el primer resultado de lo que escribió
            var lista = await Mapa.BuscarSugerenciasAsync(tipo, texto);
            if (lista == null || lista.Count == 0) return null;
            _elegido[tipo] = lista[0];
            Mapa.ElegirUbicacion(tipo, texto, lista[0].Lat, lista[0].Lng, EtiquetaMarcador(tipo));
            return new[] { lista[0].Lat, lista[0].Lng };
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
