namespace RutaApp.Blazor;
//Representa una ruta oficial con sus paradas y trazo
public class RutaOficial
{
    public int Id { get; set; }
    public string Grupo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Variante { get; set; } = "Único";
    public string? BusNombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Precio { get; set; }
    public TimeSpan? TiempoRecorrido { get; set; }
    public TimeSpan? TiempoEstimado { get; set; }

    public List<Parada> Paradas { get; set; } = new();
    public List<double[]> Trazo { get; set; } = new();
    public bool Reparado { get; set; }         // los tramos que cortaban esquinas ya se corrigieron
    public bool TrazoEnCalles { get; set; }   // true = el trazo guardado en la base de datos ya sigue las calles
}
// Representa una parada de una ruta oficial
public record Parada(int Id, string Nombre, double Latitud, double Longitud, int Orden);