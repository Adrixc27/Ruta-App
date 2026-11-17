namespace RutaApp.Interfaz.Estilos;

// Animaciones, pantallas bajas y ajustes funcionales.
public static class CssAnimaciones
{
    public const string Reglas = """
        @keyframes entra { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: none; } }
        @keyframes desvanece { from { opacity: 0; } to { opacity: 1; } }
        @keyframes aparece-modal { from { opacity: 0; transform: translateY(14px) scale(.96); } to { opacity: 1; transform: none; } }
        @keyframes toast-entra { from { opacity: 0; transform: translate(-50%, 16px) scale(.96); } to { opacity: 1; transform: translate(-50%, 0) scale(1); } }
        @keyframes pop-guardado { 0% { transform: scale(1); } 40% { transform: scale(1.45) rotate(-8deg); } 100% { transform: scale(1); } }
        @keyframes cae-pin { 0% { transform: translateY(-26px); opacity: 0; } 60% { transform: translateY(3px); opacity: 1; } 100% { transform: none; } }
        @keyframes sube-tarjeta { from { opacity: 0; transform: translateY(18px); } to { opacity: 1; transform: none; } }
        @keyframes confirma { 0% { box-shadow: 0 0 0 0 rgba(22, 163, 74, .45); } 100% { box-shadow: 0 0 0 12px rgba(22, 163, 74, 0); } }

        .contenido-cuerpo > .split, .contenido-cuerpo > .pagina, .contenido-cuerpo > .marco-detalle { animation: entra .3s ease-out; }
        .anima-entra { animation: entra .24s ease-out; }
        .modal-fondo { animation: desvanece .18s ease-out; }
        .modal { animation: aparece-modal .24s cubic-bezier(.2, .9, .3, 1.1); }
        .mapa-tarjeta { animation: sube-tarjeta .3s ease-out; }
        .mapa-banner { animation: desvanece .25s ease-out; }
        .mk-pin { animation: cae-pin .4s cubic-bezier(.2, .9, .3, 1.2); }
        .mk-parada, .mk-num, .mk-poi { animation: desvanece .3s ease-out; }
        .lugar-elegido.ok { animation: confirma .8s ease-out; }

        /* Listas: los elementos entran uno tras otro */
        .ruta-item, .opcion-ruta, .viaje-historial, .trayecto, .post, .chip-lugar-grupo, .sugerencia { animation: entra .3s ease-out backwards; }
        .lista-rutas > :nth-child(2), .lista-opciones > :nth-child(2), .trayectos-lista > :nth-child(2), .feed > :nth-child(2) { animation-delay: .04s; }
        .lista-rutas > :nth-child(3), .lista-opciones > :nth-child(3), .trayectos-lista > :nth-child(3), .feed > :nth-child(3) { animation-delay: .08s; }
        .lista-rutas > :nth-child(4), .lista-opciones > :nth-child(4), .trayectos-lista > :nth-child(4), .feed > :nth-child(4) { animation-delay: .12s; }
        .lista-rutas > :nth-child(5), .lista-opciones > :nth-child(5), .trayectos-lista > :nth-child(5), .feed > :nth-child(5) { animation-delay: .16s; }
        .lista-rutas > :nth-child(n+6), .lista-opciones > :nth-child(n+6), .trayectos-lista > :nth-child(n+6), .feed > :nth-child(n+6) { animation-delay: .2s; }

        /* Botones: se hunden al tocarlos; el icono de guardado "salta" al guardar */
        .btn, .btn-icono, .chip-radio, .chip-lugar, .nav-btn, .seg, .pill-radio, .btn-ghost, .reporte-tipo, .mapa-control, .carrusel-flecha {
            transition: transform .12s ease, background .15s, border-color .15s, box-shadow .15s, opacity .15s, filter .15s;
        }
        .btn:active:not(:disabled), .btn-icono:active, .chip-radio:active, .chip-lugar:active, .nav-btn:active, .seg:active, .pill-radio:active,
        .btn-ghost:active, .reporte-tipo:active, .mapa-control:active, .tarjeta-boton:active { transform: scale(.96); }
        .btn:hover:not(:disabled) { transform: translateY(-1px); }
        svg.icono[fill]:not([fill="none"]) { animation: pop-guardado .4s ease-out; }
        .btn-exito { background: var(--success); color: var(--text-on-brand); animation: confirma .8s ease-out; }
        .interruptor, .interruptor span { transition: all .2s ease; }

        /* Aviso breve (toast) */
        .toast-aviso {
            position: fixed; left: 50%; bottom: 28px; z-index: 3500; transform: translateX(-50%); max-width: calc(100vw - 32px);
            display: flex; align-items: center; gap: 10px; padding: 12px 18px; border-radius: var(--radio-lg); background: var(--text-primary); color: var(--text-on-brand);
            font-size: var(--texto-md); font-weight: var(--peso-fuerte); box-shadow: var(--sombra-modal); animation: toast-entra .3s cubic-bezier(.2, .9, .3, 1.2);
        }
        .toast-aviso .icono { color: #4ADE80; }

        @media (prefers-reduced-motion: reduce) {
            *, *::before, *::after { animation-duration: .01ms !important; animation-iteration-count: 1 !important; transition-duration: .01ms !important; }
        }

        /* Botones para volver a la página principal */
        .volver-landing { display: inline-flex; align-items: center; gap: 6px; flex: none; padding: 6px 12px; border-radius: var(--radio-pill); background: var(--bg-white); border: 1px solid var(--blue-200);
            font-size: var(--texto-base); font-weight: var(--peso-fuerte); color: var(--text-primary); }
        .volver-landing:hover { background: var(--blue-50); border-color: var(--brand-primary); color: var(--brand-primary); }
        .barra-ubicacion { gap: 14px; }
        .barra-ubicacion .migas { flex: 1; }
        .volver-inicio { position: absolute; top: 20px; left: 20px; display: inline-flex; align-items: center; gap: 6px; padding: 8px 14px; border-radius: var(--radio-pill); background: var(--bg-white);
            border: 1px solid var(--blue-200); font-size: var(--texto-base); font-weight: var(--peso-fuerte); color: var(--text-primary); z-index: 2; }
        .volver-inicio:hover { background: var(--blue-50); border-color: var(--brand-primary); color: var(--brand-primary); }
        .auth-panel { position: relative; }
        .auth-panel .auth-tarjeta { margin-top: 28px; }

        /* Compartir en pasos y buscar por paradas */
        .compartir-pasos { list-style: none; margin: 16px 0 18px; padding: 0; display: flex; flex-direction: column; gap: 10px; font-size: var(--texto-md); }
        .compartir-pasos li { display: flex; align-items: center; gap: 10px; }
        .compartir-pasos li span { width: 24px; height: 24px; border-radius: var(--radio-circulo); flex: none; background: var(--brand-primary); color: var(--text-on-brand); font-size: var(--texto-sm); font-weight: var(--peso-titulo); display: inline-flex; align-items: center; justify-content: center; }
        .codigo-linea { user-select: all; width: 100%; margin-top: 12px; padding: 9px 12px; border: 1px dashed var(--border-default); border-radius: var(--radio-md); background: var(--blue-50);
            font-family: var(--fuente-mono); font-size: var(--texto-xs); color: var(--text-secondary); text-overflow: ellipsis; }
        .parada-coincide { display: flex; align-items: center; gap: var(--espacio-1); font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--teal); }

        @media (max-width: 820px) {
            .volver-landing span { display: none; }
            .volver-landing { padding: 6px 9px; }
            .volver-inicio { top: 12px; left: 12px; padding: 7px 12px; }
            .toast-aviso { bottom: 76px; }
        }

        /* Pantallas bajas (laptops 1366x768 o con zoom de Windows): todo más compacto */
        @media (max-height: 760px) and (min-width: 821px) {
            .panel-scroll, .panel-comparar, .panel-rutas { padding: 16px 18px; }
            .titulo-seccion { font-size: var(--texto-3xl); line-height: 30px; }
            .encabezado-seccion { margin-bottom: 14px; }
            .sub-panel { font-size: var(--texto-base); margin-top: 4px; }
            .btn-buscar { margin-top: 14px; padding: 13px; font-size: var(--texto-lg); }
            .campo-wrap { margin-bottom: 8px; }
            .pagina { padding: 22px 28px 18px; }
            .pagina-cabecera h1 { font-size: var(--texto-3xl); }
            .barra-ubicacion { height: 42px; }
            .sidebar { padding: 16px; }
            .sidebar-menu { margin-top: 18px; }
            .nav-btn { padding: 9px 14px; margin-bottom: 4px; }
            .perfil-datos { padding: 20px; }
            .landing-contenido { padding-top: 8px; padding-bottom: 24px; }
            .landing-tarjeta { height: min(560px, calc(100vh - 150px)); }
            .landing-texto h1 { font-size: 44px; line-height: 54px; }
        }

        .explorar-sin-cuenta { margin-top: 10px !important; padding: 9px 16px; border: 1px solid var(--blue-200); border-radius: var(--radio-pill); background: var(--blue-50); }
        .explorar-sin-cuenta:hover { background: var(--blue-100); }


        /* Ajustes funcionales */
        .btn-peligro { background: var(--danger); color: var(--text-on-brand); box-shadow: 0 6px 16px rgba(220, 38, 38, .25); }
        .btn-peligro:hover:not(:disabled) { filter: brightness(1.08); }
        .tarjeta-peligro { border-color: #FCA5A5 !important; }
        .tarjeta-peligro .tarjeta-titulo { color: var(--danger); }
        .muestra-ruta { display: flex; align-items: center; gap: var(--espacio-3); padding: 8px 0; color: var(--text-primary); }
        .muestra-simbolo { width: 28px; height: 28px; border-radius: var(--radio-circulo); display: inline-flex; align-items: center; justify-content: center; background: var(--blue-100); font-size: var(--texto-md); font-weight: var(--peso-titulo); }
        .faq { cursor: pointer; font-weight: var(--peso-medio); }
        .faq:hover { color: var(--brand-primary); }
        .faq-resp { font-size: var(--texto-base); line-height: 20px; color: var(--text-secondary); padding: 4px 4px 14px; }
        """;
}
