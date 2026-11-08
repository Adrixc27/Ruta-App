namespace RutaApp.Nucleo.Modelos;

/// <summary>Un paso del viaje: caminar o tomar un camión.</summary>
public record PasoViaje(string Icono, string Titulo, string Detalle, bool EsBus, string Color, double Lat, double Lng, List<string> Instrucciones = null, string Simbolo = null);
