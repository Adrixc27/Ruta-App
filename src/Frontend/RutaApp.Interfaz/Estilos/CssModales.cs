namespace RutaApp.Interfaz.Estilos;

/// <summary>Diálogos y ventanas modales.</summary>
public static class CssModales
{
    public const string Reglas = """
        .modal-fondo { position: fixed; inset: 0; z-index: 2000; background: var(--overlay); display: flex; align-items: center; justify-content: center; padding: 20px; overflow-y: auto; }
        .modal { background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: 20px; padding: 28px; box-shadow: 0 10px 40px rgba(27, 54, 93, .25); width: 100%; }
        .modal h2 { font-size: 20px; font-weight: 900; }
        .modal-sesion { max-width: 560px; border-radius: 24px; background: #fff; padding: 56px 56px 40px; }
        .modal-sesion h2 { font-size: 26px; text-align: center; }
        .modal-sesion .btn { width: 100%; margin-top: 12px; }
        .modal-sesion h2 + .btn { margin-top: 36px; }
        .modal-dialogo { max-width: 420px; }
        .modal-dialogo p { margin: 10px 0 0; color: var(--text-secondary); white-space: pre-line; font-size: 14px; }
        .modal-nombre { max-width: 420px; }
        .modal-nombre p { color: var(--text-secondary); font-size: 13px; margin: 4px 0 18px; }
        .modal-publicacion { max-width: 560px; padding: 29px; }
        .modal-cabecera { display: flex; justify-content: space-between; align-items: flex-start; }
        .modal-cabecera h2 { font-size: 22px; }
        .modal-cabecera p { color: var(--text-secondary); font-size: 14px; margin-top: 4px; }
        .modal-acciones { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
        .modal-publicacion .etiqueta-form { margin-top: 18px; margin-bottom: 8px; }
        .nota-modal { display: flex; align-items: center; gap: 8px; font-size: 11px; color: var(--text-secondary); margin-top: 16px; }
        .tipos { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
        .tipo-opcion { position: relative; display: flex; flex-direction: column; background: #fff; border: 1px solid var(--border-default); border-radius: 14px; padding: 15px; cursor: pointer; }
        .tipo-opcion:hover { border-color: var(--brand-primary); }
        .tipo-opcion.sel { border: 2px solid var(--brand-primary); padding: 14px; }
        .tipo-opcion.sel::after { content: "✓"; position: absolute; top: 10px; right: 10px; width: 18px; height: 18px; border-radius: 50%; background: var(--brand-primary); color: #fff; font-size: 11px; display: flex; align-items: center; justify-content: center; }
        .tipo-icono { width: 34px; height: 34px; border-radius: 10px; display: flex; align-items: center; justify-content: center; }
        .tipo-opcion b { font-size: 15px; font-weight: 900; margin-top: 8px; }
        .tipo-opcion small { font-size: 11px; color: var(--text-secondary); margin-top: 4px; }
        .select-ruta { display: flex; align-items: center; gap: 10px; background: #fff; border: 1px solid var(--border-default); border-radius: 12px; padding: 0 15px; }
        .select-ruta select { flex: 1; border: 0; outline: none; background: transparent; padding: 12px 0; font: inherit; font-weight: 600; font-size: 14px; color: var(--text-primary); }
        """;
}
