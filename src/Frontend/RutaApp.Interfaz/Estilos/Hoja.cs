namespace RutaApp.Interfaz.Estilos;

// Hoja de estilos completa de la app. El orden de las secciones es el de la cascada: no lo cambies sin revisar la pantalla.
public static class Hoja
{
    public static readonly string Completa = string.Join(
        "\n",
        Tokens.Raiz(),
        CssBase.Reglas,
        CssBotones.Reglas,
        CssCampos.Reglas,
        CssMarca.Reglas,
        CssPaginasPublicas.Reglas,
        CssCarrusel.Reglas,
        CssShell.Reglas,
        CssMapa.Reglas,
        CssPaginasConEncabezado.Reglas,
        CssModales.Reglas,
        CssAjustes.Reglas,
        CssErroresBlazor.Reglas,
        CssTonosAzul.Reglas,
        CssMejoras.Reglas,
        CssResponsivo.Reglas,
        CssAnimaciones.Reglas,
        CssAccesibilidad.Reglas);
}
