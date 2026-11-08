namespace RutaApp.Nucleo.Modelos;

/// <summary>Colores de las rutas (y de los acentos de la interfaz): una paleta variada, no solo azules.</summary>
public static class Paleta
{
    public static readonly string[] Rutas =
    {
        "#2563EB", "#0D9488", "#7C3AED", "#DB2777", "#EA580C", "#16A34A", "#CA8A04", "#0891B2", "#DC2626", "#4F46E5"
    };

    public static string DeRuta(int numero) => Rutas[Math.Abs(numero) % Rutas.Length];

    // Para quien no distingue colores: cada ruta tiene también un símbolo y un trazo distinto en el mapa
    private static readonly string[] Simbolos = { "●", "▲", "■", "◆", "★", "✚", "▼", "♠", "♥", "♣" };
    private static readonly string[] Patrones = { "", "16,8", "28,8", "14,6,3,6", "34,10,6,10", "8,8", "22,6,6,6", "44,10", "10,5,10,5,2,5", "18,10" };

    public static string Simbolo(int numero) => Simbolos[Math.Abs(numero) % Simbolos.Length];
    public static string Patron(int numero) => Patrones[Math.Abs(numero) % Patrones.Length];
}
