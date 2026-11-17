namespace RutaApp.Interfaz.Estilos;

// Logotipo y marca.
public static class CssMarca
{
    public const string Reglas = """
        .marca { display: flex; align-items: center; gap: var(--espacio-3); }
        .marca-logo { width: 40px; height: 40px; border-radius: var(--radio-md); background: var(--brand-primary); color: var(--text-on-brand); font-size: var(--texto-2xl); font-weight: var(--peso-titulo); display: flex; align-items: center; justify-content: center; flex: none; }
        .marca-logo-blanco { background: var(--bg-white); color: var(--brand-primary); }
        .marca-clara .marca-logo { background: var(--bg-white); color: var(--brand-primary); }
        .marca-texto { display: flex; flex-direction: column; line-height: 1.1; }
        .marca-texto b { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); }
        .marca-texto small { font-size: var(--texto-xs); font-weight: var(--peso-medio); color: var(--text-secondary); }
        .marca-clara .marca-texto { color: var(--text-on-brand); }
        .marca-clara .marca-texto small { color: rgba(255, 255, 255, .85); }
        .marca-azul b, .marca-azul small { color: var(--brand-primary); }
        .marca-azul b { font-size: var(--texto-2xl); }
        .marca-azul small { font-size: var(--texto-xs); opacity: .85; }
        .marca-clickable { cursor: pointer; }
        """;
}
