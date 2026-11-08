namespace RutaApp.Interfaz.Idiomas;

/// <summary>
/// Traduce los textos de la interfaz. Los textos se escriben en español y, si el idioma es inglés, se busca su
/// traducción en <see cref="TextosIngles"/>; un texto sin traducción se muestra en español.
/// </summary>
public class Traductor
{
    private readonly AjustesService _ajustes;

    public Traductor(AjustesService ajustes) => _ajustes = ajustes;

    public bool EsIngles => _ajustes.Datos.Idioma == "en";

    public string this[string español] =>
        EsIngles && !string.IsNullOrEmpty(español) && TextosIngles.Textos.TryGetValue(español, out var ingles) ? ingles : español;

    /// <summary>Traduce un texto con huecos: T.F("Hola, {0}", nombre).</summary>
    public string F(string español, params object[] valores) => string.Format(this[español], valores);
}
