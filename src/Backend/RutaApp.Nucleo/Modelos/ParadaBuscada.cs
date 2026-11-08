namespace RutaApp.Nucleo.Modelos;

// Una parada encontrada por el buscador (de las rutas oficiales).
public record ParadaBuscada(string Nombre, double Lat, double Lng, string RutaNombre, int RutaNumero);
