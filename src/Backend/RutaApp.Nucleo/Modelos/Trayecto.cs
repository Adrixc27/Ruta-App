using System.Text.Json.Serialization;

namespace RutaApp.Nucleo.Modelos;

// Un viaje guardado por el usuario (Perfil → Rutas guardadas → Personalizadas). Conserva el recorrido para poder
// volver a verlo en el mapa y compartirlo.
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
