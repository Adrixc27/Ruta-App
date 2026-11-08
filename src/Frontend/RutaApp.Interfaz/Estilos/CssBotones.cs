namespace RutaApp.Interfaz.Estilos;

// Botones y botones de icono.
public static class CssBotones
{
    public const string Reglas = """
        .btn {
            display: inline-flex; align-items: center; justify-content: center; gap: 8px;
            padding: 13px 20px; border-radius: 12px; border: 1px solid transparent;
            font-size: 15px; font-weight: 700; cursor: pointer; text-align: center; transition: opacity .15s, background .15s;
        }
        .btn:disabled { opacity: .5; cursor: default; }
        .btn-primario { background: var(--brand-primary); color: #fff; }
        .btn-primario:hover:not(:disabled) { opacity: .9; }
        .btn-primario:active:not(:disabled) { opacity: .78; }
        .btn-contorno { background: var(--bg-white); color: var(--text-primary); border-color: var(--border-default); font-size: 14px; padding: 12px 20px; }
        .btn-contorno:hover:not(:disabled) { background: var(--bg-panel); }
        .btn-contorno.azul { color: var(--brand-primary); }
        .btn-suave { background: var(--bg-panel); color: var(--text-primary); border-color: var(--border-default); font-size: 13px; font-weight: 600; padding: 11px 16px; }
        .btn-suave:hover { background: var(--bg-white); }
        .btn-sinborde { background: var(--bg-white); color: var(--brand-primary); border: 0; }
        .btn-sinborde:hover { background: var(--bg-panel); }
        .btn-chico { font-size: 14px; padding: 9px 14px; }
        .btn-grande { font-size: 16px; padding: 17px 24px; }
        .btn-ancho { width: 100%; }
        .btn-buscar { font-size: 16px; padding: 16px; margin-top: 24px; }
        .btn-ghost, .btn-ghost-oscuro {
            background: transparent; border: 0; border-radius: 8px; cursor: pointer; padding: 6px 8px;
            color: var(--brand-primary); font-size: 13px; font-weight: 700; display: inline-flex; align-items: center; gap: 6px;
        }
        .btn-ghost:hover { background: var(--brand-tint); }
        .btn-ghost-oscuro { color: var(--text-primary); font-weight: 600; font-size: 15px; padding: 8px 14px; margin-right: 12px; }
        .btn-ghost-oscuro:hover { background: var(--brand-tint); }
        .btn-ghost.subrayado { text-decoration: underline; }
        .btn-ghost.centrado { display: flex; margin: 18px auto 0; font-size: 14px; }
        .btn-mini { font-size: 12px; padding: 2px 6px; }
        .btn-icono {
            display: inline-flex; align-items: center; justify-content: center; padding: 8px; border: 0; border-radius: 8px;
            background: transparent; color: var(--text-secondary); cursor: pointer;
        }
        .btn-icono:hover { background: var(--bg-panel); }
        .btn-icono.suave { background: var(--brand-tint); color: var(--brand-primary); }
        .btn-icono.suave:hover { background: #c6dcfb; }

        .card { background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: 16px; }
        .label-upper { color: var(--text-secondary); font-size: 13px; font-weight: 700; margin: 28px 0 4px; }

        .error-texto { color: var(--danger); font-size: 12px; margin: 10px 0; }
        .aviso-texto { color: var(--text-secondary); font-size: 12px; margin: 10px 0; }
        """;
}
