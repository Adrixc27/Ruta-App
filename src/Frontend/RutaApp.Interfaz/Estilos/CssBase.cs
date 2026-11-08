namespace RutaApp.Interfaz.Estilos;

/// <summary>Reinicio de estilos, tipografía base y utilidades sueltas.</summary>
public static class CssBase
{
    public const string Reglas = """
        *, *::before, *::after { box-sizing: border-box; }

        html, body {
            height: 100%;
            margin: 0;
            font-family: 'Source Sans 3', 'Source Sans Pro', 'Segoe UI', sans-serif;
            font-size: 14px;
            color: var(--text-primary);
            background: var(--bg-panel);
        }

        h1, h2, h3, h4, p { margin: 0; }
        h1:focus { outline: none; }
        b { font-weight: 700; }
        a { color: inherit; text-decoration: none; }
        button { font: inherit; color: inherit; }
        ::selection { background: var(--brand-tint); }

        /* Scrollbar delgado */
        * { scrollbar-width: thin; scrollbar-color: var(--border-default) transparent; }

        .icono { flex: none; display: block; }
        .azul { color: var(--brand-primary); }
        .vacio { text-align: center; color: var(--text-secondary); margin: 28px 0; }
        .nota { color: var(--text-secondary); font-size: 12px; margin-top: 8px; }
        .nota.exito { color: var(--success); font-weight: 700; }
        .centrado-fila { display: flex; justify-content: center; margin: 4px 0 8px; }

        /* Contenedor de la app: ocupa toda la ventana y recibe las clases de accesibilidad */
        #app, .ruta-app { height: 100%; }
        """;
}
