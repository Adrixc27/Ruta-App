namespace RutaApp.Interfaz.Estilos;

// Estados de interacción (hover, foco, activo, deshabilitado), estados de carga y estados de la interfaz
// (vacío, sin resultados, error, éxito). Va después de los estilos de cada pantalla para que todos los
// controles respondan igual.
public static class CssEstados
{
    public const string Reglas = """
        /* ── Interacción: deshabilitado. Un control apagado no responde al puntero ni cambia al pasar por encima */
        .btn:disabled, .btn-ghost:disabled, .btn-ghost-oscuro:disabled, .btn-icono:disabled, .btn-pie:disabled, .chip-radio:disabled, .chip-lugar:disabled,
        .pill-radio:disabled, .seg:disabled, .reporte-tipo:disabled, .tipo-opcion:disabled, .mapa-control:disabled, .ajuste-item:disabled,
        .btn[aria-disabled="true"] {
            opacity: var(--opacidad-deshabilitado); pointer-events: none; box-shadow: none; transform: none; filter: none;
        }
        .textarea:disabled, .select-ruta select:disabled, .campo input:disabled { cursor: not-allowed; }
        .textarea:disabled, .campo:has(input:disabled), .select-ruta:has(select:disabled) { background: var(--bg-panel); opacity: .7; }

        /* ── Interacción: hover */
        .btn-pie:hover { background: var(--hover-bg); color: var(--text-primary); }
        .btn-pie.azul:hover { color: var(--brand-primary); }
        .textarea:hover, .select-ruta:hover { border-color: var(--blue-300); }
        .tipo-opcion:hover { background: var(--blue-50); }
        /* Una publicación no es un botón: no se levanta al pasar el puntero */
        .post:hover { transform: none; box-shadow: var(--sombra-card); border-color: var(--blue-300); }

        /* ── Interacción: activo (mientras se pulsa) */
        .btn-contorno:active:not(:disabled), .btn-suave:active:not(:disabled), .btn-sinborde:active:not(:disabled) { background: var(--activo-bg); border-color: var(--blue-300); }
        .btn-ghost:active, .btn-ghost-oscuro:active, .btn-icono:active, .btn-pie:active, .ajuste-item:active, .chip-lugar:active { background: var(--activo-bg); }
        .btn-peligro:active:not(:disabled) { filter: brightness(.92); }
        .pill-radio:active:not(.activo), .chip-radio:active:not(.activo), .reporte-tipo:active:not(.sel), .tipo-opcion:active:not(.sel) { background: var(--hover-bg); }
        .btn-pie { transition: background var(--dur), color var(--dur), transform var(--dur-rapida); }
        .btn-pie:active { transform: scale(.96); }

        /* ── Interacción: foco. Los campos marcan el borde y el anillo; el resto usa el contorno global */
        .textarea:focus-visible, .select-ruta select:focus-visible { outline: none; }
        .textarea:focus, .select-ruta:focus-within { border-color: var(--brand-primary); box-shadow: var(--anillo-foco); }
        .campo.invalido, .textarea.invalido { border-color: var(--danger); box-shadow: var(--anillo-error); }
        .tipo-opcion { font: inherit; color: inherit; text-align: left; align-items: flex-start; transition: background var(--dur), border-color var(--dur); }
        .post-foto:focus-visible, .adjunto-quitar:focus-visible { outline-offset: -3px; }

        /* ── Carga: indicador giratorio. Siempre va con un texto que dice qué se está cargando */
        @keyframes gira { to { transform: rotate(360deg); } }
        @keyframes brillo { from { background-position: 100% 50%; } to { background-position: 0 50%; } }
        .spinner {
            display: inline-block; flex: none; width: 1em; height: 1em; border-radius: var(--radio-circulo);
            border: 2px solid currentColor; border-right-color: transparent; animation: gira .7s linear infinite;
        }
        .spinner.grande { font-size: 28px; border-width: 3px; color: var(--brand-primary); }
        .btn.cargando { cursor: progress; }
        .btn.cargando:disabled { opacity: .85; }
        .estado-carga { display: flex; align-items: center; gap: var(--espacio-2); padding: var(--espacio-3) 0; font-size: var(--texto-base); font-weight: var(--peso-medio); color: var(--text-secondary); }

        /* ── Carga: esqueleto con la misma forma que el contenido que va a llegar */
        .esqueleto {
            display: block; border-radius: var(--radio-sm); background: linear-gradient(90deg, var(--blue-100) 25%, var(--blue-50) 37%, var(--blue-100) 63%);
            background-size: 400% 100%; animation: brillo 1.4s ease infinite;
        }
        .esqueleto.linea { height: 12px; margin-top: var(--espacio-2); }
        .esqueleto.titulo { height: 20px; width: 70%; margin-top: var(--espacio-4); }
        .esqueleto.circulo { width: 34px; height: 34px; border-radius: var(--radio-circulo); flex: none; }
        .esqueleto.corta { width: 45%; }
        .esqueleto.media { width: 80%; }
        .post.post-esqueleto { animation: none; pointer-events: none; }
        .post-esqueleto header > div { flex: 1; }
        .post-esqueleto header .linea:first-child { margin-top: 0; }
        .post-esqueleto footer { margin-top: auto; gap: var(--espacio-3); }
        .post-esqueleto footer .linea { width: 44px; margin-top: 0; }

        /* ── Interfaz: vacío, sin resultados, error y éxito. Cada uno dice la causa y qué hacer después */
        .estado-vista { display: flex; flex-direction: column; align-items: center; text-align: center; gap: var(--espacio-2); padding: var(--espacio-8) var(--espacio-5); color: var(--text-secondary); }
        .estado-vista.compacto { padding: var(--espacio-5) var(--espacio-4); }
        .estado-icono { width: 44px; height: 44px; border-radius: var(--radio-circulo); display: flex; align-items: center; justify-content: center; background: var(--blue-100); color: var(--brand-primary); margin-bottom: var(--espacio-1); }
        .estado-vista.error .estado-icono { background: var(--danger-tint); color: var(--danger); }
        .estado-vista.exito .estado-icono { background: var(--success-tint); color: var(--success); }
        .estado-vista b { font-size: var(--texto-lg); font-weight: var(--peso-fuerte); color: var(--text-primary); }
        .estado-vista p { max-width: 44ch; font-size: var(--texto-md); line-height: var(--linea-texto); margin: 0; }
        .estado-accion { display: flex; flex-wrap: wrap; justify-content: center; gap: var(--espacio-2); margin-top: var(--espacio-2); }

        /* Mensajes dentro de un formulario */
        .mensaje { display: flex; align-items: flex-start; gap: var(--espacio-2); padding: var(--espacio-3); margin-top: var(--espacio-3); border-radius: var(--radio-md); font-size: var(--texto-base); line-height: var(--linea-texto); background: var(--blue-50); color: var(--text-primary); border: var(--borde-suave); }
        .mensaje .icono { margin-top: 1px; color: var(--brand-primary); }
        .mensaje.error { background: var(--danger-tint); border-color: #FCA5A5; }
        .mensaje.error .icono { color: var(--danger); }
        .mensaje.exito { background: var(--success-tint); border-color: #86EFAC; }
        .mensaje.exito .icono { color: var(--success); }

        /* ── Ubicación de un reporte: buscando, lista o sin ubicación */
        .ubicacion-reporte { display: flex; align-items: center; gap: var(--espacio-3); padding: var(--espacio-3) var(--espacio-4); margin-top: var(--espacio-3); border: var(--borde); border-radius: var(--radio-md); background: var(--bg-white); }
        .ubicacion-reporte > .icono, .ubicacion-reporte > .spinner { color: var(--brand-primary); }
        .ubicacion-reporte > .spinner { font-size: 18px; }
        .ubicacion-texto { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
        .ubicacion-texto b { font-size: var(--texto-md); font-weight: var(--peso-fuerte); }
        .ubicacion-texto small { font-size: var(--texto-sm); color: var(--text-secondary); line-height: var(--linea-texto); }
        .ubicacion-reporte.lista { border-color: #86EFAC; background: var(--success-tint); }
        .ubicacion-reporte.lista > .icono { color: var(--success); }
        .ubicacion-reporte.fallo { border-color: #FCD34D; background: var(--warning-tint); }
        .ubicacion-reporte.fallo > .icono { color: var(--warning); }
        .ubicacion-reporte .btn-ghost { flex: none; }

        /* ── Comunidad: fotos de una publicación */
        .post-fotos { display: grid; gap: var(--espacio-1); margin-top: var(--espacio-3); border-radius: var(--radio-md); overflow: hidden; }
        .post-fotos.n2 { grid-template-columns: 1fr 1fr; }
        .post-fotos.n3 { grid-template-columns: 2fr 1fr; grid-template-rows: 1fr 1fr; aspect-ratio: 16 / 10; }
        .post-foto { display: block; padding: 0; border: 0; min-height: 0; overflow: hidden; background: var(--blue-100); cursor: zoom-in; aspect-ratio: 16 / 10; }
        .post-fotos.n2 .post-foto { aspect-ratio: 1; }
        .post-fotos.n3 .post-foto { aspect-ratio: auto; }
        .post-fotos.n3 .post-foto:first-child { grid-row: span 2; }
        .post-foto img { display: block; width: 100%; height: 100%; object-fit: cover; transition: filter var(--dur); }
        .post-foto:hover img { filter: brightness(.9); }
        .post-foto:active img { filter: brightness(.8); }

        /* Fotos adjuntas al escribir una publicación */
        .adjuntos { display: flex; flex-wrap: wrap; gap: var(--espacio-2); }
        .adjunto, .adjunto-agregar { position: relative; width: 88px; height: 88px; border-radius: var(--radio-md); overflow: hidden; flex: none; }
        .adjunto { border: var(--borde); background: var(--blue-100); }
        .adjunto img { display: block; width: 100%; height: 100%; object-fit: cover; }
        .adjunto-quitar { position: absolute; top: 4px; right: 4px; width: 24px; height: 24px; padding: 0; border: 0; border-radius: var(--radio-circulo); background: rgba(27, 54, 93, .78); color: var(--text-on-brand); display: flex; align-items: center; justify-content: center; cursor: pointer; transition: background var(--dur); }
        .adjunto-quitar:hover { background: var(--danger); }
        .adjunto-quitar:active { background: #B91C1C; }
        .adjunto-agregar { display: flex; flex-direction: column; align-items: center; justify-content: center; gap: var(--espacio-1); border: 1px dashed var(--border-default); background: var(--bg-white); color: var(--text-secondary); font-size: var(--texto-sm); font-weight: var(--peso-fuerte); cursor: pointer; transition: background var(--dur), border-color var(--dur), color var(--dur); }
        .adjunto-agregar:hover { border-color: var(--brand-primary); background: var(--blue-50); color: var(--brand-primary); }
        .adjunto-agregar:active { background: var(--hover-bg); }
        .adjunto-agregar:focus-within { outline: 2px solid var(--brand-primary); outline-offset: 2px; }
        .adjunto-agregar.ocupado { pointer-events: none; color: var(--brand-primary); }
        .adjunto-agregar .spinner { font-size: 20px; }
        .adjunto-agregar.ocupado span:last-child { font-size: var(--texto-xs); }
        .chips-problemas .chip-radio { display: inline-flex; align-items: center; gap: var(--espacio-1); }
        .adjunto-agregar input { position: absolute; width: 1px; height: 1px; opacity: 0; overflow: hidden; }

        /* Foto ampliada */
        .visor-foto { background: rgba(15, 30, 55, .86); cursor: zoom-out; }
        .visor-foto img { max-width: min(1100px, 100%); max-height: calc(100vh - 96px); border-radius: var(--radio-md); box-shadow: var(--sombra-modal); cursor: default; animation: aparece-modal var(--dur-lenta) var(--curva); }
        .visor-cerrar { position: fixed; top: var(--espacio-4); right: var(--espacio-4); width: 40px; height: 40px; border: 0; border-radius: var(--radio-circulo); background: var(--bg-white); color: var(--text-primary); display: flex; align-items: center; justify-content: center; cursor: pointer; transition: background var(--dur); }
        .visor-cerrar:hover { background: var(--hover-bg); }
        .visor-cerrar:active { background: var(--activo-bg); }

        /* Comentarios */
        .comentarios { display: flex; flex-direction: column; gap: var(--espacio-2); margin-top: var(--espacio-3); padding-top: var(--espacio-3); border-top: var(--borde-suave); }
        .comentarios p { margin: 0; }
        .comentario-nuevo { display: flex; gap: var(--espacio-2); align-items: center; }
        .comentario-nuevo .campo { flex: 1; }
        .etiqueta.ubicacion { background: var(--rose-tint); border-color: var(--rose-tint); color: var(--rose); }

        /* ── Modales: nunca más altos que la ventana. El de nueva publicación deja fijos el título y los botones y desplaza solo el formulario */
        .modal { max-height: calc(100dvh - 40px); overflow-y: auto; }
        .modal.modal-publicacion { display: flex; flex-direction: column; overflow: hidden; padding: 0; max-width: 600px; }
        .modal-publicacion .modal-cabecera { flex: none; padding: var(--espacio-5) var(--espacio-6) var(--espacio-3); }
        .modal-cuerpo { flex: 1; min-height: 0; overflow-y: auto; padding: 0 var(--espacio-6) var(--espacio-4); }
        .modal-publicacion .modal-acciones { flex: none; margin: 0; padding: var(--espacio-3) var(--espacio-6) var(--espacio-4); border-top: var(--borde-suave); }
        .modal-publicacion .etiqueta-form { margin-top: 14px; margin-bottom: 6px; }
        .modal-cuerpo > .etiqueta-form:first-child { margin-top: var(--espacio-1); }
        .modal-publicacion .nota-modal { margin-top: 10px; }
        .modal-publicacion .tipo-opcion { display: grid; grid-template-columns: auto 1fr; column-gap: 10px; align-items: center; padding: 10px 12px; }
        .modal-publicacion .tipo-opcion.sel { padding: 9px 11px; }
        .modal-publicacion .tipo-icono { grid-row: span 2; }
        .modal-publicacion .tipo-opcion b, .modal-publicacion .tipo-opcion small { margin-top: 0; }
        .modal-publicacion .tipo-opcion.sel::after { top: 6px; right: 6px; width: 16px; height: 16px; font-size: 10px; }
        .modal-publicacion .adjunto, .modal-publicacion .adjunto-agregar { width: 68px; height: 68px; }
        .modal-publicacion .adjunto-agregar { font-size: var(--texto-xs); }
        .modal-publicacion .ubicacion-reporte { padding: 10px var(--espacio-3); }

        /* Comunidad: orden de la lista y publicación a la que se llegó desde el mapa */
        .orden { display: flex; flex-wrap: wrap; gap: 6px; }
        .post.destacada { border-color: var(--brand-primary); box-shadow: var(--anillo-foco), var(--sombra-card); }
        .post:focus { outline: none; }
        .mk-reporte { cursor: pointer; }

        @media (max-width: 820px) {
            .modal { max-height: calc(100dvh - 24px); }
            .modal-publicacion .tipos { grid-template-columns: 1fr; gap: 6px; }
            .ubicacion-reporte { flex-wrap: wrap; }
            .adjunto, .adjunto-agregar { width: 76px; height: 76px; }
        }
        """;
}
