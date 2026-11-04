using System.Globalization;
using System.Text.Json.Serialization;

namespace RutaApp.Blazor.Models;


public class Ruta
{
    public int Numero { get; set; }
    public string Nombre { get; set; } = "";
    public string Modo { get; set; } = "Autobús";        // para los filtros: Autobús / Exprés / Metro
    public string ModoTexto { get; set; } = "Autobús";   
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
    public Trayecto TrayectoFavorito { get; set; }       // si es una opción de viaje marcada como guardada (aparece en Perfil)
    public bool DatosReales { get; set; }                // true = Minutos/Tarifa vienen de la base de datos
    public bool Reparado { get; set; }                   // true = ya se corrigieron los tramos del recorrido que cortaban esquinas
    public bool TrazoEnCalles { get; set; } = true;      // false = el recorrido une las paradas en línea recta y hay que ajustarlo a las calles

    /// <summary>Color propio de la ruta (lista, tarjeta y línea en el mapa), tomado de la paleta según su número.</summary>
    public string Color => Paleta.DeRuta(Numero);

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
    public List<double[]> Trazo { get; set; }        // recorrido de la ruta [lat, lng] en orden
    public List<Tramo> Tramos { get; set; }          // solo en opciones de viaje: caminar / bus / caminar...
    public List<Sugerencia> Escalas { get; set; }    // paradas intermedias del viaje (A → B → C)

    // Variantes (ramales) de la ruta, por ejemplo Komatsu Inverso / Directo. Con más de una, el mapa deja elegir.
    // Vienen de la base de datos (RutaService): cada variante es una fila de routes con el mismo "grupo".
    public List<RutaOficial> Variantes { get; set; }
    public int VarianteActual { get; set; }
    public bool TieneVariantes => Variantes != null && Variantes.Count > 1;
    public RutaOficial Oficial => Variantes != null && Variantes.Count > 0 ? Variantes[VarianteActual] : null;

    /// <summary>Cambia a la variante indicada: recorrido, paradas, origen y destino.</summary>
    public void UsarVariante(int indice)
    {
        if (Variantes == null || indice < 0 || indice >= Variantes.Count) return;
        VarianteActual = indice;
        var o = Variantes[indice];

        Trazo = o.Trazo.Select(p => new[] { p[0], p[1] }).ToList();
        TrazoEnCalles = o.TrazoEnCalles;
        Reparado = o.Reparado;
        Paradas = o.Paradas.Count;

        if (o.Paradas.Count > 0)
        {
            OrigenNombre = o.Paradas[0].Nombre;
            DestinoNombre = o.Paradas[o.Paradas.Count - 1].Nombre;
            LatO = o.Paradas[0].Latitud; LngO = o.Paradas[0].Longitud;
            LatD = o.Paradas[o.Paradas.Count - 1].Latitud;
            LngD = o.Paradas[o.Paradas.Count - 1].Longitud;
        }
        else if (Trazo.Count > 0)
        {
            LatO = Trazo[0][0]; LngO = Trazo[0][1];
            LatD = Trazo[Trazo.Count - 1][0]; LngD = Trazo[Trazo.Count - 1][1];
        }
    }

    /// <summary>Instrucciones del viaje paso a paso (camina, toma tal ruta...), armadas con sus tramos.</summary>
    public List<PasoViaje> Pasos()
    {
        var lista = new List<PasoViaje>();
        if (Tramos == null) return lista;
        for (int i = 0; i < Tramos.Count; i++)
        {
            var t = Tramos[i];
            if (t.Puntos == null || t.Puntos.Count == 0) continue;
            var ini = t.Puntos[0];
            bool ultimo = i == Tramos.Count - 1;
            if (!t.EsBus)
                lista.Add(new PasoViaje("IcoWalk", "Camina " + Math.Max(1, t.Minutos) + " min",
                    ultimo ? "hasta tu destino" : (i == 0 ? "hasta la parada" : "hasta la siguiente parada"), false, "#475569", ini[0], ini[1]));
            else
                lista.Add(new PasoViaje("IcoBus", "Toma " + (string.IsNullOrWhiteSpace(t.Linea) ? "el camión" : t.Linea),
                    Math.Max(1, t.Minutos) + " min a bordo", true, t.Color ?? "#2563EB", ini[0], ini[1]));
        }
        return lista;
    }

    public string EtiquetaMayus => Etiqueta.ToUpperInvariant();
    public string TransbordosTexto =>
        Transbordos == 0 ? "Sin transbordos" : Transbordos == 1 ? "1 transbordo" : Transbordos + " transbordos";

    public bool Guardada { get; set; }
    public bool Seleccionada { get; set; }
}

/// <summary>Punto de control de una ruta oficial (A, B, C...). Ya no se usa en la ruta; se deja por si otra clase lo referencia.</summary>
public record PuntoControl(string Letra, string Nombre, double Lat, double Lng);

/// <summary>
/// Un viaje guardado por el usuario (Perfil → Rutas guardadas → Personalizadas). Conserva el recorrido para poder
/// volver a verlo en el mapa y compartirlo.
/// </summary>
public class Trayecto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Alias { get; set; } = "";
    public string Recorrido { get; set; } = "";   // "Casa > Escuela"
    public string Detalle { get; set; } = "";     // "18 min · $2.75"
    public string Origen { get; set; } = "";
    public string Destino { get; set; } = "";
    public double OrigenLat { get; set; }
    public double OrigenLng { get; set; }
    public double DestinoLat { get; set; }
    public double DestinoLng { get; set; }
    public int Minutos { get; set; }
    public decimal Tarifa { get; set; }
    public string Lineas { get; set; } = "";
    public List<Tramo> Tramos { get; set; } = new();
    public List<Sugerencia> Escalas { get; set; } = new();
    public bool Compartido { get; set; }          // llegó por un código de otra persona

    [JsonIgnore]
    public bool TieneRecorrido => Tramos != null && Tramos.Count > 0;
}

/// <summary>Un lugar que el usuario guarda para planificar (Casa, Escuela, Trabajo u otro punto de referencia).</summary>
public class LugarGuardado
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "otro";    // casa | escuela | trabajo | otro
    public string Direccion { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }

    [JsonIgnore]
    public string Icono => Tipo switch { "casa" => "IcoHome", "escuela" => "IcoSchool", "trabajo" => "IcoBriefcase", _ => "IcoFlag" };
    [JsonIgnore]
    public string TipoTexto => Tipo switch { "casa" => "Casa", "escuela" => "Escuela", "trabajo" => "Trabajo", _ => "Punto de referencia" };
}

/// <summary>
/// Un viaje que el usuario ya hizo (de dónde a dónde y en qué camiones). Se agrega solo al pulsar "Comenzar viaje".
/// Conserva los tramos para poder dibujarlo de nuevo en el mapa al tocarlo.
/// </summary>
public class ViajeHistorial
{
    public int Id { get; set; }
    public double OrigenLat { get; set; }
    public double OrigenLng { get; set; }
    public double DestinoLat { get; set; }
    public double DestinoLng { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int Minutos { get; set; }
    public decimal Tarifa { get; set; }
    public string Destino { get; set; } = "";
    public string Camiones { get; set; } = "";  // camiones que tomó: "Bus 6 → Bus 9"
    public List<Tramo> Tramos { get; set; } = new();
    public List<Sugerencia> Escalas { get; set; } = new();

    /// <summary>De dónde salió. Si salió de su ubicación, lleva la calle: "Ubicación actual (Calle Aldama)".</summary>
    public string Origen { get; set; } = "";

    public string GuardadoId { get; set; }      // id del Trayecto guardado a partir de este viaje (si lo hay)

    [JsonIgnore]
    public string Titulo => Origen + " > " + Destino;
    [JsonIgnore]
    public string CamionesTexto => string.IsNullOrWhiteSpace(Camiones) ? "Camiones: sin datos" : "Camiones: " + Camiones;
    [JsonIgnore]
    public string Cuando => Formato.Hace(Fecha);
    [JsonIgnore]
    public string Detalle => Cuando + " · " + Minutos + " min";
    [JsonIgnore]
    public bool EstaGuardado => !string.IsNullOrEmpty(GuardadoId);
    [JsonIgnore]
    public bool TieneRecorrido => Tramos != null && Tramos.Count > 0;
}

/// <summary>Un paso del viaje: caminar o tomar un camión.</summary>
public record PasoViaje(string Icono, string Titulo, string Detalle, bool EsBus, string Color, double Lat, double Lng);

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
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int Id { get; set; }        // id_post en la base de datos 
    public int IdAutor { get; set; }   // id del usuario que publicó 
}

/// <summary>Un tramo del viaje: caminando (punteado) o en camión (línea azul).</summary>
public class Tramo
{
    public bool EsBus { get; set; }
    public List<double[]> Puntos { get; set; } = new();   // [lat, lng]
    public string TextoInicio { get; set; }   // etiqueta de la parada donde empieza (solo camión)
    public string TextoFin { get; set; }      // etiqueta de la parada donde termina (solo camión)
    public string Linea { get; set; }         // nombre de la ruta que se toma (solo camión)
    public string Color { get; set; }         // color de esa ruta en el mapa
    public int Minutos { get; set; }          // duración del tramo
    public bool AjustarACalles { get; set; }  // el recorrido une paradas en línea recta: se ajusta a las calles al mostrarlo
    public bool Reparar { get; set; }         // recorrido guardado: se corrigen solo los tramos que cortan esquinas
    public bool Ajustado { get; set; }
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
    /// Punto de la línea más cercano a (lat, lng) y a qué distancia en metros queda. Sirve para que una parada
    /// desfasada se dibuje sobre el recorrido en lugar de a un lado.
    /// </summary>
    public static double[] ProyectarEnLinea(IList<double[]> linea, double lat, double lng, out double metros)
    {
        metros = double.MaxValue;
        if (linea == null || linea.Count == 0) return null;
        double kx = 111320 * Math.Cos(lat * Math.PI / 180), ky = 110574;
        double[] mejor = linea[0];
        for (int i = 0; i < linea.Count; i++)
        {
            double ax = (linea[i][1] - lng) * kx, ay = (linea[i][0] - lat) * ky;
            double t = 0, bx = ax, by = ay;
            if (i + 1 < linea.Count)
            {
                bx = (linea[i + 1][1] - lng) * kx; by = (linea[i + 1][0] - lat) * ky;
                double dx = bx - ax, dy = by - ay, largo2 = dx * dx + dy * dy;
                if (largo2 > 0) t = Math.Max(0, Math.Min(1, -(ax * dx + ay * dy) / largo2));
            }
            double px = ax + (bx - ax) * t, py = ay + (by - ay) * t;
            double d = Math.Sqrt(px * px + py * py);
            if (d < metros)
            {
                metros = d;
                mejor = i + 1 < linea.Count
                    ? new[] { linea[i][0] + (linea[i + 1][0] - linea[i][0]) * t, linea[i][1] + (linea[i + 1][1] - linea[i][1]) * t }
                    : linea[i];
            }
        }
        return mejor;
    }
}

/// <summary>Petición de otra pantalla (Perfil) para elegir un lugar tocando el mapa de Planificar.</summary>
public class SolicitudLugar
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "otro";
}

public class CategoriaAjustes
{
    public string Titulo { get; set; } = "";
    public List<string> Ajustes { get; set; } = new();
}

public static class Formato
{
    /// <summary>"ahora", "hace 5 min", "hace 3 h", "ayer", "hace 4 d" o la fecha si pasó más de una semana.</summary>
    public static string Hace(DateTime fecha)
    {
        var t = DateTime.Now - fecha;
        if (t.TotalMinutes < 1) return "ahora";
        if (t.TotalMinutes < 60) return "hace " + (int)t.TotalMinutes + " min";
        if (t.TotalHours < 24) return "hace " + (int)t.TotalHours + " h";
        if (t.TotalDays < 2) return "ayer";
        if (t.TotalDays < 8) return "hace " + (int)t.TotalDays + " d";
        return fecha.ToString("d 'de' MMM", new CultureInfo("es-MX"));
    }
}

/// <summary>Colores de las rutas (y de los acentos de la interfaz): una paleta variada, no solo azules.</summary>
public static class Paleta
{
    public static readonly string[] Rutas =
    {
        "#2563EB", "#0D9488", "#7C3AED", "#DB2777", "#EA580C", "#16A34A", "#CA8A04", "#0891B2", "#DC2626", "#4F46E5"
    };

    public static string DeRuta(int numero) => Rutas[Math.Abs(numero) % Rutas.Length];
}
