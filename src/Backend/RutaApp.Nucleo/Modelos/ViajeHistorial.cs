using System.Text.Json.Serialization;

namespace RutaApp.Nucleo.Modelos;

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
