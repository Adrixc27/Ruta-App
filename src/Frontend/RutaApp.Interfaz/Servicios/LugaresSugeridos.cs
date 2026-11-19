namespace RutaApp.Interfaz.Servicios;

// Lugares conocidos de Chihuahua que se recomiendan al elegir el destino. Se eligieron porque quedan cerca de las rutas de la muestra y el
// planificador sí encuentra viaje hacia ellos desde los demás (EstadoApp.LugaresRecomendados además descarta los que queden lejos de las
// rutas que haya cargadas). Las coordenadas son las de OpenStreetMap.
public static class LugaresSugeridos
{
    public record Lugar(string Nombre, string Tipo, double Lat, double Lng);

    public static readonly Lugar[] Todos =
    {
        new("Catedral de Chihuahua", "Centro histórico", 28.63663, -106.07653),
        new("Palacio de Gobierno", "Centro histórico", 28.63908, -106.07337),
        new("Quinta Gameros", "Museo", 28.63224, -106.07356),
        new("Parque Lerdo", "Parque", 28.63005, -106.07440),
        new("Museo de la Revolución (Casa de Villa)", "Museo", 28.62659, -106.06855),
        new("Parque El Palomar", "Parque", 28.64064, -106.08310),
        new("Hospital General Dr. Salvador Zubirán", "Hospital", 28.64962, -106.06961),
        new("Ciudad Judicial", "Gobierno", 28.63501, -106.08097),
        new("Hospital Infantil de Especialidades", "Hospital", 28.62815, -106.03062),
        new("Fashion Mall", "Plaza comercial", 28.62272, -106.11492),
        new("Plaza Cumbres", "Plaza comercial", 28.63925, -106.12191),
        new("Instituto Tecnológico de Chihuahua", "Escuela", 28.66233, -106.08076),
    };
}
