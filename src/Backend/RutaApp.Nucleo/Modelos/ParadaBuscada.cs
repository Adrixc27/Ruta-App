namespace RutaApp.Nucleo.Modelos;

/// <summary>Una parada encontrada por el buscador (de las rutas oficiales).</summary>
public record ParadaBuscada(string Nombre, double Lat, double Lng, string RutaNombre, int RutaNumero);
