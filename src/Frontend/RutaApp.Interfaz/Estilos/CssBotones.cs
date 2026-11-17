namespace RutaApp.Interfaz.Estilos;

// Botones y botones de icono.
public static class CssBotones
{
    public const string Reglas = """
        .btn {
            display: inline-flex; align-items: center; justify-content: center; gap: var(--espacio-2);
            padding: 13px 20px; border-radius: var(--radio-md); border: 1px solid transparent;
            font-size: var(--texto-lg); font-weight: var(--peso-fuerte); cursor: pointer; text-align: center; transition: opacity .15s, background .15s;
        }
        .btn:disabled { opacity: .5; cursor: default; }
        .btn-primario { background: var(--brand-primary); color: var(--text-on-brand); }
        .btn-primario:hover:not(:disabled) { opacity: .9; }
        .btn-primario:active:not(:disabled) { opacity: .78; }
        .btn-contorno { background: var(--bg-white); color: var(--text-primary); border-color: var(--border-default); font-size: var(--texto-md); padding: 12px 20px; }
        .btn-contorno:hover:not(:disabled) { background: var(--bg-panel); }
        .btn-contorno.azul { color: var(--brand-primary); }
        .btn-suave { background: var(--bg-panel); color: var(--text-primary); border-color: var(--border-default); font-size: var(--texto-base); font-weight: var(--peso-medio); padding: 11px 16px; }
        .btn-suave:hover { background: var(--bg-white); }
        .btn-sinborde { background: var(--bg-white); color: var(--brand-primary); border: 0; }
        .btn-sinborde:hover { background: var(--bg-panel); }
        .btn-chico { font-size: var(--texto-md); padding: 9px 14px; }
        .btn-grande { font-size: var(--texto-lg); padding: 17px 24px; }
        .btn-ancho { width: 100%; }
        .btn-buscar { font-size: var(--texto-lg); padding: 16px; margin-top: 24px; }
        .btn-ghost, .btn-ghost-oscuro {
            background: transparent; border: 0; border-radius: var(--radio-sm); cursor: pointer; padding: 6px 8px;
            color: var(--brand-primary); font-size: var(--texto-base); font-weight: var(--peso-fuerte); display: inline-flex; align-items: center; gap: 6px;
        }
        .btn-ghost:hover { background: var(--brand-tint); }
        .btn-ghost-oscuro { color: var(--text-primary); font-weight: var(--peso-medio); font-size: var(--texto-lg); padding: 8px 14px; margin-right: 12px; }
        .btn-ghost-oscuro:hover { background: var(--brand-tint); }
        .btn-ghost.subrayado { text-decoration: underline; }
        .btn-ghost.centrado { display: flex; margin: 18px auto 0; font-size: var(--texto-md); }
        .btn-mini { font-size: var(--texto-sm); padding: 2px 6px; }
        .btn-icono {
            display: inline-flex; align-items: center; justify-content: center; padding: 8px; border: 0; border-radius: var(--radio-sm);
            background: transparent; color: var(--text-secondary); cursor: pointer;
        }
        .btn-icono:hover { background: var(--bg-panel); }
        .btn-icono.suave { background: var(--brand-tint); color: var(--brand-primary); }
        .btn-icono.suave:hover { background: #c6dcfb; }

        .card { background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: var(--radio-lg); }
        .label-upper { color: var(--text-secondary); font-size: var(--texto-base); font-weight: var(--peso-fuerte); margin: 28px 0 4px; }

        .error-texto { color: var(--danger); font-size: var(--texto-sm); margin: 10px 0; }
        .aviso-texto { color: var(--text-secondary); font-size: var(--texto-sm); margin: 10px 0; }
        """;
}
