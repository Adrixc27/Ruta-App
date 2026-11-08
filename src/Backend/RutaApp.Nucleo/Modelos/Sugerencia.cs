namespace RutaApp.Nucleo.Modelos;

/// <summary>Un lugar encontrado por el buscador (o una parada intermedia del viaje).</summary>
public class Sugerencia
{
    public string Numero { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Detalle { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
}
