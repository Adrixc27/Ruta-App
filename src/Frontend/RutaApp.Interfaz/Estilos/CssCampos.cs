namespace RutaApp.Interfaz.Estilos;

/// <summary>Campos de texto.</summary>
public static class CssCampos
{
    public const string Reglas = """
        .campo {
            display: flex; align-items: center; gap: 12px; background: var(--bg-white);
            border: 1px solid var(--border-default); padding: 0 14px; width: 100%;
        }
        .campo.enfocado { border-color: var(--brand-primary); box-shadow: 0 0 0 1px var(--brand-primary); }
        .campo input { flex: 1; min-width: 0; border: 0; outline: none; background: transparent; font: inherit; font-size: 14px; color: var(--text-primary); padding: 10px 0; }
        .campo input::placeholder { color: var(--text-secondary); opacity: .75; }
        .campo-punto { width: 10px; height: 10px; border-radius: 50%; flex: none; }
        .campo .btn-icono { padding: 6px; margin-right: -8px; }

        .textarea, .area-texto textarea {
            width: 100%; border: 1px solid var(--border-default); border-radius: 12px; background: var(--bg-white);
            padding: 12px 14px; font: inherit; font-size: 14px; color: var(--text-primary); resize: vertical; outline: none;
        }
        .textarea:focus { border-color: var(--brand-primary); box-shadow: 0 0 0 1px var(--brand-primary); }
        """;
}
