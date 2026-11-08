namespace RutaApp.Interfaz.Idiomas;

/// <summary>Traducciones al inglés: la clave es el texto en español tal como aparece en la interfaz.</summary>
public static class TextosIngles
{
    public static readonly Dictionary<string, string> Textos = Reunir(InglesPantallas.Pares, InglesAjustes.Pares, InglesMensajes.Pares);

    // Si una clave se repite, se queda con la primera
    private static Dictionary<string, string> Reunir(params (string Español, string Ingles)[][] grupos)
    {
        var textos = new Dictionary<string, string>();
        foreach (var grupo in grupos)
            foreach (var (español, ingles) in grupo)
                textos.TryAdd(español, ingles);
        return textos;
    }
}
