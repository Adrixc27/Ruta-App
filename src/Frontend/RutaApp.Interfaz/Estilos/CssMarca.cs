namespace RutaApp.Interfaz.Estilos;

/// <summary>Logotipo y marca.</summary>
public static class CssMarca
{
    public const string Reglas = """
        .marca { display: flex; align-items: center; gap: 12px; }
        .marca-logo { width: 40px; height: 40px; border-radius: 10px; background: var(--brand-primary); color: #fff; font-size: 20px; font-weight: 900; display: flex; align-items: center; justify-content: center; flex: none; }
        .marca-logo-blanco { background: #fff; color: var(--brand-primary); }
        .marca-clara .marca-logo { background: #fff; color: var(--brand-primary); }
        .marca-texto { display: flex; flex-direction: column; line-height: 1.1; }
        .marca-texto b { font-size: 22px; font-weight: 900; }
        .marca-texto small { font-size: 11px; font-weight: 600; color: var(--text-secondary); }
        .marca-clara .marca-texto { color: #fff; }
        .marca-clara .marca-texto small { color: rgba(255, 255, 255, .85); }
        .marca-azul b, .marca-azul small { color: var(--brand-primary); }
        .marca-azul b { font-size: 20px; }
        .marca-azul small { font-size: 10px; opacity: .85; }
        .marca-clickable { cursor: pointer; }
        """;
}
