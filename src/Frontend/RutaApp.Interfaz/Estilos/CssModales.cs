namespace RutaApp.Interfaz.Estilos;

// Diálogos y ventanas modales.
public static class CssModales
{
    public const string Reglas = """
        .modal-fondo { position: fixed; inset: 0; z-index: 2000; background: var(--overlay); display: flex; align-items: center; justify-content: center; padding: 20px; overflow-y: auto; }
        .modal { background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: var(--radio-xl); padding: 28px; box-shadow: var(--sombra-modal); width: 100%; }
        .modal h2 { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); }
        .modal-sesion { max-width: 560px; border-radius: var(--radio-xl); background: var(--bg-white); padding: 56px 56px 40px; }
        .modal-sesion h2 { font-size: var(--texto-3xl); text-align: center; }
        .modal-sesion .btn { width: 100%; margin-top: 12px; }
        .modal-sesion h2 + .btn { margin-top: 36px; }
        .modal-dialogo { max-width: 420px; }
        .modal-dialogo p { margin: 10px 0 0; color: var(--text-secondary); white-space: pre-line; font-size: var(--texto-md); }
        .modal-nombre { max-width: 420px; }
        .modal-nombre p { color: var(--text-secondary); font-size: var(--texto-base); margin: 4px 0 18px; }
        .modal-publicacion { max-width: 560px; padding: 29px; }
        .modal-cabecera { display: flex; justify-content: space-between; align-items: flex-start; }
        .modal-cabecera h2 { font-size: var(--texto-2xl); }
        .modal-cabecera p { color: var(--text-secondary); font-size: var(--texto-md); margin-top: 4px; }
        .modal-acciones { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
        .modal-publicacion .etiqueta-form { margin-top: 18px; margin-bottom: 8px; }
        .nota-modal { display: flex; align-items: center; gap: var(--espacio-2); font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 16px; }
        .tipos { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
        .tipo-opcion { position: relative; display: flex; flex-direction: column; background: var(--bg-white); border: 1px solid var(--border-default); border-radius: var(--radio-lg); padding: 15px; cursor: pointer; }
        .tipo-opcion:hover { border-color: var(--brand-primary); }
        .tipo-opcion.sel { border: 2px solid var(--brand-primary); padding: 14px; }
        .tipo-opcion.sel::after { content: "✓"; position: absolute; top: 10px; right: 10px; width: 18px; height: 18px; border-radius: var(--radio-circulo); background: var(--brand-primary); color: var(--text-on-brand); font-size: var(--texto-xs); display: flex; align-items: center; justify-content: center; }
        .tipo-icono { width: 34px; height: 34px; border-radius: var(--radio-md); display: flex; align-items: center; justify-content: center; }
        .tipo-opcion b { font-size: var(--texto-lg); font-weight: var(--peso-titulo); margin-top: 8px; }
        .tipo-opcion small { font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 4px; }
        .select-ruta { display: flex; align-items: center; gap: 10px; background: var(--bg-white); border: 1px solid var(--border-default); border-radius: var(--radio-md); padding: 0 15px; }
        .select-ruta select { flex: 1; border: 0; outline: none; background: transparent; padding: 12px 0; font: inherit; font-weight: var(--peso-medio); font-size: var(--texto-md); color: var(--text-primary); }
        """;
}
