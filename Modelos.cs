using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Ruta_App
{
    // ─────────────────────────────────────────────────────────────
    //  Infraestructura pequeña
    // ─────────────────────────────────────────────────────────────

    /// <summary>Las vistas que quieran enterarse cuando se navega hacia ellas implementan esto.</summary>
    public interface INavegable
    {
        void AlNavegar(object parametro);
    }

    /// <summary>Convierte el nombre de un recurso ("IcoBus") en la Geometry correspondiente.</summary>
    public class IconoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clave = value as string;
            if (clave == null) return null;
            return Application.Current.TryFindResource(clave);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propiedad)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(propiedad));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Modelos
    // ─────────────────────────────────────────────────────────────

    public class Ruta : Observable
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = "";
        public string Modo { get; set; } = "Autobús";        // para los filtros: Autobús / Exprés / Metro
        public string ModoTexto { get; set; } = "Autobús";   // lo que se muestra: "Metro + Bus"
        public int FrecuenciaMin { get; set; } = 10;
        public decimal Tarifa { get; set; } = 2.75m;
        public bool Desvio { get; set; }
        public string Horario { get; set; } = "5:30 - 23:00";
        public int Paradas { get; set; } = 20;
        public int Minutos { get; set; } = 20;
        public int Transbordos { get; set; }
        public string Lineas { get; set; } = "";             // chip de la pantalla Comparar
        public string Etiqueta { get; set; } = "";           // Recomendado / Más rápido / Económico

        public string OrigenNombre { get; set; } = "";
        public string DestinoNombre { get; set; } = "";
        public double LatO { get; set; }
        public double LngO { get; set; }
        public double LatD { get; set; }
        public double LngD { get; set; }

        // Textos ya formateados para los bindings
        public string NumeroTexto { get { return Numero.ToString(); } }
        public string TarifaTexto { get { return "$" + Tarifa.ToString("0.00", CultureInfo.InvariantCulture); } }
        public string MinutosTexto { get { return Minutos + " min"; } }
        public string Meta { get { return ModoTexto + " · Cada " + FrecuenciaMin + " min · " + TarifaTexto; } }
        public string Subtitulo { get { return ModoTexto + " · " + Horario; } }
        public string FrecuenciaTexto { get { return "Cada " + FrecuenciaMin + " min desde parada central"; } }
        public string EtiquetaMayus { get { return Etiqueta.ToUpperInvariant(); } }
        public string TransbordosTexto
        {
            get
            {
                if (Transbordos == 0) return "Sin transbordos";
                return Transbordos == 1 ? "1 transbordo" : Transbordos + " transbordos";
            }
        }

        private bool _guardada;
        public bool Guardada
        {
            get { return _guardada; }
            set { if (_guardada == value) return; _guardada = value; Notify("Guardada"); }
        }

        private bool _seleccionada;
        public bool Seleccionada
        {
            get { return _seleccionada; }
            set { if (_seleccionada == value) return; _seleccionada = value; Notify("Seleccionada"); }
        }
    }

    public class Reciente
    {
        public Ruta Ruta { get; set; }
        public string Texto { get; set; } = "";
    }

    public class Trayecto : Observable
    {
        public string Alias { get; set; } = "";
        public string Recorrido { get; set; } = "";   // "Kennedy > BOW"
        public string Detalle { get; set; } = "";     // "18 min, de lunes a viernes"

        private bool _favorito;
        public bool Favorito
        {
            get { return _favorito; }
            set { if (_favorito == value) return; _favorito = value; Notify("Favorito"); }
        }
    }

    public class Etiqueta
    {
        public string Texto { get; set; } = "";
        public bool EsRuta { get; set; }
        public string Icono { get { return EsRuta ? "IcoBus" : "IcoPin"; } }
    }

    public class Publicacion : Observable
    {
        public string Tipo { get; set; } = "aviso";   // aviso | pregunta | propuesta
        public string Autor { get; set; } = "";
        public bool Oficial { get; set; }
        public string Tiempo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Cuerpo { get; set; } = "";
        public List<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
        public int Comentarios { get; set; }
        public int Orden { get; set; }        // mayor = más reciente
        public int RutaNumero { get; set; }   // 0 = no está ligada a una ruta

        private int _likes;
        public int Likes
        {
            get { return _likes; }
            set { if (_likes == value) return; _likes = value; Notify("Likes"); }
        }

        private bool _meGusta;
        public bool MeGusta
        {
            get { return _meGusta; }
            set { if (_meGusta == value) return; _meGusta = value; Notify("MeGusta"); }
        }

        public string Inicial { get { return Autor.Length > 0 ? Autor.Substring(0, 1).ToUpperInvariant() : "?"; } }
        public string TipoTexto { get { return Tipo.ToUpperInvariant(); } }
        public string IconoTipo { get { return Tipo == "aviso" ? "IcoAlert" : (Tipo == "pregunta" ? "IcoHelp" : "IcoBulb"); } }
        public string Cta { get { return Tipo == "pregunta" ? "Responder" : "Ver en el mapa"; } }
    }

    // ─────────────────────────────────────────────────────────────
    //  Utilidades de mapa
    // ─────────────────────────────────────────────────────────────

    public static class Geo
    {
        public const double CentroLat = 28.6353;
        public const double CentroLng = -106.0889;

        /// <summary>
        /// Camino "escalonado" entre dos puntos (como el dibujo de Figma).
        /// Es solo demostrativo: cuando tengas rutas reales, reemplázalo por las coordenadas verdaderas.
        /// </summary>
        public static List<double[]> Camino(double latO, double lngO, double latD, double lngD)
        {
            double dLat = latD - latO;
            double dLng = lngD - lngO;
            var p1 = new[] { latO + dLat * 0.45, lngO };
            var p2 = new[] { p1[0], lngO + dLng * 0.55 };
            var p3 = new[] { latD, p2[1] };
            return new List<double[]>
            {
                new[] { latO, lngO }, p1, p2, p3, new[] { latD, lngD }
            };
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Estado compartido entre pantallas (sin base de datos: todo en memoria)
    // ─────────────────────────────────────────────────────────────

    public static class Estado
    {
        // Sesión
        public static string Nombre = "Laura Guerrero";
        public static string Correo = "laura.g@ruta.com";
        public static bool Invitado;

        // Búsqueda actual (Planificar → Comparar)
        public static string Origen = "Mi ubicación actual";
        public static string Destino = "";
        public static string Prioridad = "Rápido";

        // Navegación: la asigna MainWindow
        public static Action<string, object> Navegar;

        // Datos
        public static List<Ruta> Rutas = DatosDemo.CrearRutas();
        public static ObservableCollection<Reciente> Recientes = DatosDemo.CrearRecientes(Rutas);
        public static ObservableCollection<Trayecto> Trayectos = DatosDemo.CrearTrayectos();
        public static ObservableCollection<Publicacion> Publicaciones = DatosDemo.CrearPublicaciones();
        public static List<Publicacion> PublicacionesExtra = DatosDemo.CrearPublicacionesExtra();

        public static event Action GuardadasCambiaron;

        public static int TotalGuardadas { get { return Rutas.Count(r => r.Guardada); } }

        public static Ruta BuscarRuta(int numero)
        {
            return Rutas.FirstOrDefault(r => r.Numero == numero);
        }

        public static void AlternarGuardada(Ruta ruta)
        {
            ruta.Guardada = !ruta.Guardada;
            var h = GuardadasCambiaron;
            if (h != null) h();
        }

        public static void AgregarReciente(Ruta ruta)
        {
            var existente = Recientes.FirstOrDefault(r => r.Ruta == ruta);
            if (existente != null) Recientes.Remove(existente);
            Recientes.Insert(0, new Reciente { Ruta = ruta, Texto = "Hace un momento" });
            while (Recientes.Count > 3) Recientes.RemoveAt(Recientes.Count - 1);
        }

        public static string Iniciales(string nombre)
        {
            var partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();
            return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Datos de demostración (los mismos textos del diseño de Figma)
    // ─────────────────────────────────────────────────────────────

    internal static class DatosDemo
    {
        public static List<Ruta> CrearRutas()
        {
            // Las coordenadas son inventadas (cerca del centro de Chihuahua), solo para dibujar algo en el mapa.
            return new List<Ruta>
            {
                new Ruta { Numero = 3, Nombre = "Ruta 3 - Exprés Centro", Modo = "Exprés", ModoTexto = "Exprés",
                    FrecuenciaMin = 10, Tarifa = 1.50m, Horario = "6:00 - 22:00", Paradas = 12, Minutos = 25,
                    Lineas = "Bus Exprés", Etiqueta = "Económico",
                    OrigenNombre = "Terminal Norte", DestinoNombre = "Centro",
                    LatO = 28.7000, LngO = -106.1100, LatD = 28.6350, LngD = -106.0780 },

                new Ruta { Numero = 4, Nombre = "Ruta 4 - Zona Centro", FrecuenciaMin = 12, Tarifa = 2.75m,
                    Desvio = true, Horario = "5:30 - 22:30", Paradas = 22, Minutos = 22, Lineas = "Bus 104",
                    OrigenNombre = "Terminal Sur", DestinoNombre = "Zona Centro",
                    LatO = 28.5900, LngO = -106.1000, LatD = 28.6360, LngD = -106.0770 },

                new Ruta { Numero = 6, Nombre = "Ruta 6 - Villas", FrecuenciaMin = 10, Tarifa = 2.75m,
                    Horario = "5:30 - 23:00", Paradas = 24, Minutos = 18, Lineas = "Bus 102",
                    Etiqueta = "Recomendado", Guardada = true,
                    OrigenNombre = "Terminal Villas", DestinoNombre = "Centro Histórico",
                    LatO = 28.6100, LngO = -106.1450, LatD = 28.6355, LngD = -106.0770 },

                new Ruta { Numero = 7, Nombre = "Ruta 7 - Bosque Azul", Modo = "Metro", ModoTexto = "Metro + Bus",
                    FrecuenciaMin = 15, Tarifa = 3.50m, Horario = "5:00 - 22:30", Paradas = 18, Minutos = 14,
                    Transbordos = 1, Lineas = "Metro L + Bus 12", Etiqueta = "Más rápido", Guardada = true,
                    OrigenNombre = "Terminal Centro", DestinoNombre = "Bosque Azul",
                    LatO = 28.6350, LngO = -106.0790, LatD = 28.6900, LngD = -106.0450 },

                new Ruta { Numero = 9, Nombre = "Ruta 9 - Plan de Ayala", FrecuenciaMin = 12, Tarifa = 2.75m,
                    Paradas = 21, Minutos = 20, Lineas = "Bus 109",
                    OrigenNombre = "Plan de Ayala", DestinoNombre = "Centro Histórico",
                    LatO = 28.6620, LngO = -106.1200, LatD = 28.6355, LngD = -106.0770 },

                new Ruta { Numero = 12, Nombre = "Ruta 12 - Circunvalación II", FrecuenciaMin = 20, Tarifa = 2.75m,
                    Paradas = 26, Minutos = 25, Lineas = "Bus 112", Guardada = true,
                    OrigenNombre = "Circunvalación II", DestinoNombre = "Centro",
                    LatO = 28.6800, LngO = -106.0950, LatD = 28.6340, LngD = -106.0800 },

                new Ruta { Numero = 15, Nombre = "Ruta 15 - Komatsu", FrecuenciaMin = 18, Tarifa = 2.75m,
                    Paradas = 19, Minutos = 23, Lineas = "Bus 115",
                    OrigenNombre = "Komatsu", DestinoNombre = "Centro",
                    LatO = 28.7100, LngO = -106.0600, LatD = 28.6350, LngD = -106.0790 }
            };
        }

        public static ObservableCollection<Reciente> CrearRecientes(List<Ruta> rutas)
        {
            return new ObservableCollection<Reciente>
            {
                new Reciente { Ruta = rutas.First(r => r.Numero == 6), Texto = "Guardado recientemente" },
                new Reciente { Ruta = rutas.First(r => r.Numero == 7), Texto = "Hace 2 días" }
            };
        }

        public static ObservableCollection<Trayecto> CrearTrayectos()
        {
            return new ObservableCollection<Trayecto>
            {
                new Trayecto { Alias = "Trabajo", Recorrido = "Kennedy > BOW", Detalle = "18 min, de lunes a viernes" },
                new Trayecto { Alias = "Gimnasio", Recorrido = "Plan de Ayala > Komatsu", Detalle = "14 min" },
                new Trayecto { Alias = "Casa de mamá", Recorrido = "Circunvalacion II", Detalle = "25 min, domingos" }
            };
        }

        private static Etiqueta R(int n) { return new Etiqueta { Texto = "Ruta " + n, EsRuta = true }; }
        private static Etiqueta Z(string t) { return new Etiqueta { Texto = t, EsRuta = false }; }

        public static ObservableCollection<Publicacion> CrearPublicaciones()
        {
            return new ObservableCollection<Publicacion>
            {
                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 2 h",
                    Titulo = "Aviso de servicio",
                    Cuerpo = "La Ruta 4 tendrá un desvío temporal por obras en Zona Centro. Comparte alternativas con la comunidad.",
                    Etiquetas = new List<Etiqueta> { R(4), Z("Zona Centro") }, Likes = 12, Comentarios = 4, Orden = 100, RutaNumero = 4 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 5 h",
                    Titulo = "Tu experiencia importa",
                    Cuerpo = "¿Qué parada necesita mejor señalización? Cuéntanos cómo mejorar tus recorridos diarios.",
                    Etiquetas = new List<Etiqueta> { Z("Paradas") }, Likes = 19, Comentarios = 5, Orden = 90 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "ayer",
                    Titulo = "Nueva conexión",
                    Cuerpo = "Se propone extender la Ruta 7 hasta Bosque Azul. Revisa el mapa y deja tu opinión.",
                    Etiquetas = new List<Etiqueta> { R(7), Z("Bosque Azul") }, Likes = 26, Comentarios = 6, Orden = 80, RutaNumero = 7 },

                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 1 d",
                    Titulo = "Horario especial en Ruta 6",
                    Cuerpo = "Este domingo la Ruta 6 - Villas circulará cada 20 minutos por mantenimiento de unidades.",
                    Etiquetas = new List<Etiqueta> { R(6), Z("Villas") }, Likes = 8, Comentarios = 2, Orden = 70, RutaNumero = 6 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 1 d",
                    Titulo = "¿Horario de la Ruta 3 Exprés?",
                    Cuerpo = "Voy de Circunvalación II al centro y quiero saber a qué hora pasa el exprés por la mañana.",
                    Etiquetas = new List<Etiqueta> { R(3) }, Likes = 5, Comentarios = 9, Orden = 60, RutaNumero = 3 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 2 d",
                    Titulo = "Más frecuencia en Plan de Ayala",
                    Cuerpo = "Sugerimos una unidad extra en hora pico, de 7 a 9 am, para reducir el tiempo de espera.",
                    Etiquetas = new List<Etiqueta> { Z("Plan de Ayala") }, Likes = 31, Comentarios = 12, Orden = 50, RutaNumero = 9 }
            };
        }

        public static List<Publicacion> CrearPublicacionesExtra()
        {
            return new List<Publicacion>
            {
                new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 3 d",
                    Titulo = "Mantenimiento en Ruta 12",
                    Cuerpo = "La Ruta 12 - Circunvalación II no circulará el sábado de 2 a 5 am por trabajos de mantenimiento.",
                    Etiquetas = new List<Etiqueta> { R(12), Z("Circunvalación II") }, Likes = 4, Comentarios = 1, Orden = 40, RutaNumero = 12 },

                new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 3 d",
                    Titulo = "¿Aceptan tarjeta?",
                    Cuerpo = "¿Las unidades de la Ruta 15 ya aceptan pago con tarjeta o solo efectivo?",
                    Etiquetas = new List<Etiqueta> { R(15), Z("Komatsu") }, Likes = 7, Comentarios = 3, Orden = 30, RutaNumero = 15 },

                new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 4 d",
                    Titulo = "Paradas con techo",
                    Cuerpo = "Proponemos instalar techo en las paradas más concurridas de Zona Centro para los días de lluvia y sol fuerte.",
                    Etiquetas = new List<Etiqueta> { Z("Paradas"), Z("Zona Centro") }, Likes = 22, Comentarios = 8, Orden = 20 }
            };
        }
    }
}
