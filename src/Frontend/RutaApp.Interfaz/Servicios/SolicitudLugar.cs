namespace RutaApp.Interfaz.Servicios;

/// <summary>Petición de otra pantalla (Perfil) para elegir un lugar tocando el mapa de Planificar.</summary>
public class SolicitudLugar
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "otro";
}
