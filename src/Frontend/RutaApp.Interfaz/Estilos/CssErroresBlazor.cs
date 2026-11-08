namespace RutaApp.Interfaz.Estilos;

/// <summary>Validación de formularios y barra de error de Blazor.</summary>
public static class CssErroresBlazor
{
    public const string Reglas = """
        .valid.modified:not([type=checkbox]) { outline: 1px solid #26b050; }
        .invalid { outline: 1px solid #e50000; }
        .validation-message { color: #e50000; }

        #blazor-error-ui {
            color-scheme: light only; background: lightyellow; bottom: 0; box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2); box-sizing: border-box;
            display: none; left: 0; padding: 0.6rem 1.25rem 0.7rem 1.25rem; position: fixed; width: 100%; z-index: 5000;
        }
        #blazor-error-ui .dismiss { cursor: pointer; position: absolute; right: 0.75rem; top: 0.5rem; }

        .blazor-error-boundary { background: #b32121; padding: 1rem 1rem 1rem 3.7rem; color: white; }
        .blazor-error-boundary::after { content: "Ocurrió un error."; }
        """;
}
