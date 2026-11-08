namespace RutaApp.Interfaz.Servicios;

// Navegación entre pantallas y búsqueda actual (Planificar → Comparar).
public partial class EstadoApp
{
    /// <summary>Parámetro de la última navegación (por ejemplo una Ruta para abrirla en el mapa). Se consume una vez.</summary>
    public object ParametroNavegacion { get; private set; }

    /// <summary>Claves: planificar, comparar, rutas, comunidad, perfil, config y ajuste (parametro = nombre del ajuste).</summary>
    public void Navegar(string clave, object parametro = null)
    {
        ParametroNavegacion = parametro;
        string url = clave switch
        {
            "config" => "configuracion",
            "ajuste" => "configuracion/ajuste?nombre=" + Uri.EscapeDataString(parametro as string ?? ""),
            _ => clave
        };
        _nav.NavigateTo("/" + url);
    }

    public T TomarParametro<T>() where T : class
    {
        var p = ParametroNavegacion as T;
        if (p != null) ParametroNavegacion = null;
        return p;
    }

    // Búsqueda actual (Planificar → Comparar)
    public const string MiUbicacion = "Mi ubicación actual";

    public string Origen { get; set; } = MiUbicacion;
    public string Destino { get; set; } = "";

    // Coordenadas de la búsqueda actual (las llena Planificar; NaN = desconocidas)
    public double OrigenLat { get; set; } = double.NaN;
    public double OrigenLng { get; set; }
    public double DestinoLat { get; set; } = double.NaN;
    public double DestinoLng { get; set; }

    public List<Sugerencia> Escalas { get; set; } = new();   // paradas intermedias de la búsqueda

    public bool HayCoordenadas => !double.IsNaN(OrigenLat) && !double.IsNaN(DestinoLat);

    /// <summary>Olvida la búsqueda actual (destino, paradas y coordenadas), por ejemplo al empezar el viaje.</summary>
    public void NuevaBusqueda()
    {
        Destino = "";
        Escalas = new List<Sugerencia>();
        OrigenLat = double.NaN;
        DestinoLat = double.NaN;
    }

    /// <summary>Última ubicación del equipo obtenida (null si aún no se pide o no se pudo).</summary>
    public double[] MiPosicion { get; set; }
}
