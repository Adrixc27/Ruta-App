namespace RutaApp.Nucleo.Modelos;

// Un tramo del viaje: caminando (punteado) o en camión (línea azul).
public class Tramo
{
    public bool EsBus { get; set; }
    public List<double[]> Puntos { get; set; } = new();   // [lat, lng]
    public string TextoInicio { get; set; }   // etiqueta de la parada donde empieza (solo camión)
    public string TextoFin { get; set; }      // etiqueta de la parada donde termina (solo camión)
    public string Linea { get; set; }         // nombre de la ruta que se toma (solo camión)
    public string Color { get; set; }         // color de esa ruta en el mapa
    public int Minutos { get; set; }          // duración del tramo
    public bool AjustarACalles { get; set; }  // el recorrido une paradas en línea recta: se ajusta a las calles al mostrarlo
    public int Metros { get; set; }                       // distancia del tramo (a pie)
    public List<string> Instrucciones { get; set; }       // cómo caminar (calles y giros), solo a pie
    public string ParadaInicio { get; set; }              // parada (o zona) donde se sube, solo camión
    public string ParadaFin { get; set; }                 // parada (o zona) donde se baja, solo camión
    public string Simbolo { get; set; }                   // símbolo de la ruta (para quien no distingue colores)
    public string Patron { get; set; }                    // trazo de la ruta en el mapa (guiones) para modo daltonismo
    public int EsperaMin { get; set; }                    // cada cuántos minutos pasa el camión
    public bool Reparar { get; set; }         // recorrido guardado: se corrigen solo los tramos que cortan esquinas
    public bool Ajustado { get; set; }
}
