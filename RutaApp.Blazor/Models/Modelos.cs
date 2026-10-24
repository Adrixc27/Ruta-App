using System.Globalization;

namespace RutaApp.Blazor.Models;

// ─────────────────────────────────────────────────────────────
//  Modelos de la interfaz (los mismos que usaba la app WPF, ya sin INotifyPropertyChanged:
//  Blazor vuelve a pintar el componente cuando cambia el estado).
// ─────────────────────────────────────────────────────────────

public class Ruta
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
    public string Descripcion { get; set; } = "";         // texto de la tabla routes
    public string Lineas { get; set; } = "";             // chip de la pantalla Comparar
    public string Etiqueta { get; set; } = "";           // Recomendado / Más rápido / Económico
    public Trayecto TrayectoFavorito { get; set; }       // si es una opción de viaje marcada como favorita (aparece en Perfil)
    public bool DatosReales { get; set; }                // true = Minutos/Tarifa vienen de la base de datos (el trazo del mapa puede ser de prueba)

    public string OrigenNombre { get; set; } = "";
    public string DestinoNombre { get; set; } = "";
    public double LatO { get; set; }
    public double LngO { get; set; }
    public double LatD { get; set; }
    public double LngD { get; set; }

    // Textos ya formateados para la interfaz
    public string NumeroTexto => Numero.ToString();
    public string TarifaTexto => "$" + Tarifa.ToString("0.00", CultureInfo.InvariantCulture);
    public string MinutosTexto => Minutos + " min";
    public string Meta => ModoTexto + " · " + MinutosTexto + " · " + TarifaTexto;
    public string Subtitulo => string.IsNullOrEmpty(Descripcion) ? ModoTexto + " · " + Horario : Descripcion;
    public string FrecuenciaTexto =>
        string.IsNullOrEmpty(Resumen) ? "Cada " + FrecuenciaMin + " min desde parada central" : Resumen;

    // Cuando la ruta es una opción de viaje (camina → camión → camina) hacia un destino concreto
    public bool EsViaje { get; set; }
    public string Resumen { get; set; } = "";        // "Camina 4 min · Bus 12 min · Camina 3 min"
    public int CaminataMetros { get; set; }
    public List<double[]> Trazo { get; set; }        // recorrido de la ruta [lat, lng] en orden (por calles)
    public List<double[]> Waypoints { get; set; }    // puntos clave por donde pasa (para calcular el Trazo)
    public List<Tramo> Tramos { get; set; }          // solo en opciones de viaje: caminar / bus / caminar...
    public List<Sugerencia> Escalas { get; set; }    // paradas intermedias del viaje (A → B → C)
    public List<PuntoControl> PuntosControl { get; set; }   // puntos de control del plano oficial (si la ruta tiene plano)

    // Variantes (ramales) del plano oficial, por ejemplo Komatsu Inverso / Directo. Con más de una, el mapa deja elegir.
    public List<RutaOficial> Variantes { get; set; }
    public int VarianteActual { get; set; }
    public bool TieneVariantes => Variantes != null && Variantes.Count > 1;
    public RutaOficial Oficial => Variantes != null && Variantes.Count > 0 ? Variantes[VarianteActual] : null;

    /// <summary>Cambia a la variante indicada del plano oficial: recorrido, puntos de control, frecuencia y horario.</summary>
    public void UsarVariante(int indice)
    {
        if (Variantes == null || indice < 0 || indice >= Variantes.Count) return;
        VarianteActual = indice;
        var o = Variantes[indice];
        Trazo = o.Trazo.Select(p => new[] { p[0], p[1] }).ToList();
        Waypoints = null;   // el trazo ya es el oficial: no se recalcula con OSRM
        PuntosControl = o.PuntosControl;
        FrecuenciaMin = o.Frecuencia;
        if (!string.IsNullOrEmpty(o.Horario)) Horario = o.Horario;
        Paradas = o.PuntosControl.Count;
        OrigenNombre = o.PuntosControl[0].Letra + " · " + o.PuntosControl[0].Nombre;
        DestinoNombre = "Fin del ciclo (vuelve a " + o.PuntosControl[0].Letra + ")";
        LatO = Trazo[0][0]; LngO = Trazo[0][1];
        LatD = Trazo[Trazo.Count - 1][0]; LngD = Trazo[Trazo.Count - 1][1];
        if (!DatosReales && o.CicloMin > 0) Minutos = o.CicloMin;
    }
    public string EtiquetaMayus => Etiqueta.ToUpperInvariant();
    public string TransbordosTexto =>
        Transbordos == 0 ? "Sin transbordos" : Transbordos == 1 ? "1 transbordo" : Transbordos + " transbordos";

    public bool Guardada { get; set; }
    public bool Seleccionada { get; set; }
}

/// <summary>Punto de control de una ruta oficial (A, B, C...): una intersección por donde pasa el camión.</summary>
public record PuntoControl(string Letra, string Nombre, double Lat, double Lng);

public class Reciente
{
    public Ruta Ruta { get; set; }
    public string Texto { get; set; } = "";
}

public class Trayecto
{
    public string Alias { get; set; } = "";
    public string Recorrido { get; set; } = "";   // "Kennedy > BOW"
    public string Detalle { get; set; } = "";     // "18 min, de lunes a viernes"
    public bool Favorito { get; set; }
}

/// <summary>
/// Un viaje que el usuario ya hizo (de dónde a dónde y en qué camiones). Se agrega solo al pulsar "Comenzar viaje".
/// </summary>
public class ViajeHistorial
{
    public int Id { get; set; }
    public double OrigenLat { get; set; }
    public double OrigenLng { get; set; }
    public double DestinoLat { get; set; }
    public double DestinoLng { get; set; }
    public string Cuando { get; set; } = "";    // "Hoy, 8:05"
    public int Minutos { get; set; }
    public string Destino { get; set; } = "";
    public string Camiones { get; set; } = "";  // camiones que tomó: "Bus 6 → Bus 9"

    /// <summary>De dónde salió. Si salió de su ubicación, lleva la calle: "Ubicación actual (Calle Aldama)".</summary>
    public string Origen { get; set; } = "";

    public string Titulo => Origen + " > " + Destino;
    public string CamionesTexto => string.IsNullOrWhiteSpace(Camiones) ? "Camiones: sin datos" : "Camiones: " + Camiones;
    public string Detalle => Cuando + " · " + Minutos + " min";

    // El viaje guardado aparece en Perfil → Rutas Guardadas; aquí se recuerda cuál es para poder quitarlo
    public Trayecto Guardado { get; set; }
    public bool EstaGuardado => Guardado != null;
}

public class Etiqueta
{
    public string Texto { get; set; } = "";
    public bool EsRuta { get; set; }
    public string Icono => EsRuta ? "IcoBus" : "IcoPin";
}

public class Publicacion
{
    public string Tipo { get; set; } = "aviso";   // aviso | pregunta | propuesta
    public string Autor { get; set; } = "";
    public bool Oficial { get; set; }
    public string Tiempo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Cuerpo { get; set; } = "";
    public List<Etiqueta> Etiquetas { get; set; } = new();
    public int Comentarios { get; set; }
    public int Orden { get; set; }        // mayor = más reciente
    public int RutaNumero { get; set; }   // 0 = no está ligada a una ruta
    public int Likes { get; set; }
    public bool MeGusta { get; set; }

    public string Inicial => Autor.Length > 0 ? Autor.Substring(0, 1).ToUpperInvariant() : "?";
    public string TipoTexto => Tipo.ToUpperInvariant();
    public string IconoTipo => Tipo == "aviso" ? "IcoAlert" : (Tipo == "pregunta" ? "IcoHelp" : "IcoBulb");
    public string Cta => Tipo == "pregunta" ? "Responder" : "Ver en el mapa";
}

/// <summary>Un tramo del viaje: caminando (punteado) o en camión (línea azul).</summary>
public class Tramo
{
    public bool EsBus { get; set; }
    public List<double[]> Puntos { get; set; } = new();   // [lat, lng]
    public string TextoInicio { get; set; }   // etiqueta de la parada donde empieza (solo camión)
    public string TextoFin { get; set; }      // etiqueta de la parada donde termina (solo camión)
}

/// <summary>Un lugar encontrado por el buscador (o una parada intermedia del viaje).</summary>
public class Sugerencia
{
    public string Numero { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Detalle { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
}

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

public class CategoriaAjustes
{
    public string Titulo { get; set; } = "";
    public List<string> Ajustes { get; set; } = new();
}
