namespace RutaApp.Interfaz.Estilos;

// Diseño móvil de la muestra web (.ruta-app.muestra, pantallas de hasta 820 px). La app de escritorio no lo usa.
// Idea (Google Maps, Uber, Didi): el mapa ocupa toda la pantalla y lo único que se ve encima son tarjetas de vidrio
// (.glass: fondo translúcido con desenfoque) con lo esencial de cada pantalla:
//   Planificar → un campo de destino arriba; al tocarlo aparece el origen y el botón de buscar.
//   Rutas      → la lista flota en el centro; al elegir una ruta se cierra y se ve el recorrido en el mapa.
//   Comparar   → las opciones del viaje en una tarjeta abajo.
// Compartir códigos no existe en móvil y el historial vive en el menú de hamburguesa (MenuHistorial).
public static class CssMovil
{
    public const string Reglas = """
        .landing-nota { display: block; margin-top: var(--espacio-4); font-size: var(--texto-sm); color: var(--text-secondary); line-height: var(--linea-texto); }
        .flotante-boton, .menu-flotante, .panel-cerrar, .tarjeta-lista, .ubicacion-tarjeta { display: none; }
        .landing-nota b { color: var(--text-primary); }
        .sugerencias-titulo { padding: var(--espacio-2) var(--espacio-3) var(--espacio-1); font-size: var(--texto-sm); font-weight: var(--peso-medio); color: var(--text-secondary); }
        .chip-boton { border: 0; font: inherit; font-size: var(--texto-xs); font-weight: var(--peso-fuerte); cursor: pointer; }
        .chip-boton:hover { filter: brightness(.96); }
        .aviso-demo ul { margin: var(--espacio-3) 0 0; padding-left: var(--espacio-5); color: var(--text-secondary); font-size: var(--texto-md); line-height: var(--linea-texto); }
        .aviso-demo li + li { margin-top: var(--espacio-2); }

        /* Historial: solo vive en Rutas (menú del mapa). En Planificar no se muestra por ahora. */
        .ruta-app.muestra .panel-scroll > .label-upper, .ruta-app.muestra .panel-scroll > .nota-panel,
        .ruta-app.muestra .viaje-historial, .ruta-app.muestra .panel-scroll > .btn-ghost { display: none; }

        /* Botón y menú del historial (escritorio: arriba a la derecha del mapa; en móvil se reubican abajo) */
        .ruta-app.muestra .flotante-boton {
            position: absolute; z-index: 1150; align-items: center; justify-content: center; gap: var(--espacio-2); min-width: 44px; height: 44px; padding: 0 var(--espacio-4);
            border: 1px solid var(--border-default); border-radius: var(--radio-pill); background: var(--glass-fondo);
            backdrop-filter: var(--glass-desenfoque); -webkit-backdrop-filter: var(--glass-desenfoque);
            color: var(--text-primary); font: inherit; font-size: var(--texto-md); font-weight: var(--peso-fuerte); box-shadow: var(--sombra-flotante); cursor: pointer;
        }
        .ruta-app.muestra .flotante-boton:active { background: var(--blue-100); }
        .ruta-app.muestra .menu-flotante { display: contents; }   /* sin caja propia: así no ocupa una celda de la cuadrícula del escritorio */
        .ruta-app.muestra .menu-boton { display: inline-flex; top: 10px; right: 62px; width: 44px; padding: 0; border-radius: var(--radio-circulo); }
        .ruta-app.muestra .menu-fondo { position: absolute; inset: 0; z-index: 1140; }
        .ruta-app.muestra .menu-panel {
            position: absolute; z-index: 1160; top: 62px; right: 10px; width: 340px; max-height: min(60%, 420px); display: flex; flex-direction: column; overflow: hidden;
            background: var(--glass-fondo); backdrop-filter: var(--glass-desenfoque); -webkit-backdrop-filter: var(--glass-desenfoque);
            border: 1px solid var(--border-default); border-radius: var(--radio-xl); box-shadow: var(--sombra-flotante);
        }
        .ruta-app.muestra .menu-panel header { display: flex; align-items: center; justify-content: space-between; padding: var(--espacio-3) var(--espacio-4) var(--espacio-1); font-size: var(--texto-lg); }
        .ruta-app.muestra .menu-lista { overflow-y: auto; padding: 0 var(--espacio-2) var(--espacio-2); }
        .ruta-app.muestra .menu-viaje {
            display: flex; align-items: center; gap: var(--espacio-3); width: 100%; min-height: 56px; padding: var(--espacio-2); border: 0; border-bottom: var(--borde-suave);
            background: transparent; color: inherit; font: inherit; text-align: left; cursor: pointer;
        }
        .ruta-app.muestra .menu-viaje:last-child { border-bottom: 0; }
        .ruta-app.muestra .menu-viaje:hover { background: var(--blue-50); }
        .ruta-app.muestra .menu-viaje:active { background: var(--blue-100); }
        .ruta-app.muestra .menu-viaje .icono { flex: none; color: var(--text-secondary); }
        .ruta-app.muestra .menu-viaje span { min-width: 0; display: flex; flex-direction: column; }
        .ruta-app.muestra .menu-viaje b, .ruta-app.muestra .menu-viaje small { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .ruta-app.muestra .menu-viaje b { font-size: var(--texto-md); }
        .ruta-app.muestra .menu-viaje small { font-size: var(--texto-sm); color: var(--text-secondary); }

        @media (max-width: 820px) {
            /* ── Estructura: sin barra superior; el contenido llega hasta la barra de pestañas */
            .ruta-app.muestra .barra-ubicacion { display: none; }
            .ruta-app.muestra .app-shell { background: var(--bg-white); }
            .ruta-app.muestra .contenido { background: var(--bg-page); }
            .ruta-app.muestra .contenido:has(.proximamente) { background: var(--bg-white); }

            /* ── Barra de pestañas: 5 destinos con icono y nombre, el activo con una píldora suave */
            .ruta-app.muestra .sidebar, .ruta-app.muestra .sidebar.colapsado {
                height: calc(60px + env(safe-area-inset-bottom)); padding: 0 var(--espacio-2) env(safe-area-inset-bottom);
                background: var(--bg-white); border-top: var(--borde-suave); box-shadow: none;
            }
            .ruta-app.muestra .sidebar-pie { display: none; }
            .ruta-app.muestra .sidebar-menu { gap: 0; }
            .ruta-app.muestra .sidebar .nav-btn {
                position: relative; flex: 1 1 0; min-width: 0; height: 100%; flex-direction: column; justify-content: flex-start; gap: 3px;
                padding: 9px 2px 0; margin: 0; border-radius: 0; background: transparent; color: var(--text-secondary);
                font-size: 10.5px; line-height: 1.1; font-weight: var(--peso-medio); letter-spacing: 0;
            }
            .ruta-app.muestra .sidebar .nav-btn > * { position: relative; z-index: 1; }
            .ruta-app.muestra .sidebar .nav-btn span { display: block; flex: none; width: auto; max-width: 100%; opacity: 1; overflow: hidden; text-overflow: ellipsis; }
            .ruta-app.muestra .sidebar .nav-btn .icono { color: currentColor !important; }
            .ruta-app.muestra .sidebar .nav-btn.activo { background: transparent; box-shadow: none; color: var(--brand-primary); font-weight: var(--peso-fuerte); }
            .ruta-app.muestra .sidebar .nav-btn.activo::before {
                display: block; content: ""; position: absolute; z-index: 0; top: 6px; left: calc(50% - 26px); width: 52px; height: 30px;
                border-radius: var(--radio-pill); background: var(--brand-tint);
            }
            .ruta-app.muestra .sidebar .nav-btn:hover { background: transparent; }
            .ruta-app.muestra .sidebar .nav-btn:active { background: transparent; transform: none; }
            .ruta-app.muestra .nav-badge, .ruta-app.muestra .sidebar.colapsado .nav-badge { top: 6px; left: calc(50% + 6px); }

            /* ── Mapa a pantalla completa; todo lo demás flota encima */
            .ruta-app.muestra .split { position: absolute; inset: 0; display: block; }
            .ruta-app.muestra .split > .mapa-contenedor { position: absolute; inset: 0; order: 0; }
            .ruta-app.muestra .leaflet-control-zoom, .ruta-app.muestra .mapa-banner, .ruta-app.muestra .mapa-leyenda { display: none; }
            .ruta-app.muestra .leaflet-control-attribution { font-size: 9px; }

            /* Botones de vidrio: hamburguesa arriba a la izquierda y "Rutas" arriba a la derecha */
            .ruta-app.muestra .menu-boton { top: calc(env(safe-area-inset-top) + var(--margen-flotante)); left: var(--margen-flotante); right: auto; }
            .ruta-app.muestra .menu-panel { top: calc(env(safe-area-inset-top) + var(--margen-flotante) + 54px); left: var(--margen-flotante); right: var(--margen-flotante); width: auto; }

            /* "Mi ubicación": un solo botón, abajo a la derecha (sube si hay una tarjeta abajo) */
            .ruta-app.muestra .mapa-controles { top: auto; right: var(--margen-flotante); bottom: 28px; transition: bottom var(--dur-lenta) var(--curva); }
            .ruta-app.muestra .mapa-controles .mapa-control + .mapa-control { display: none; }
            .ruta-app.muestra .mapa-control { width: 44px; height: 44px; border-radius: var(--radio-circulo); border-color: transparent; background: var(--bg-white); box-shadow: var(--sombra-flotante); }
            .ruta-app.muestra .split:has(.mapa-tarjeta) .mapa-controles { bottom: 104px; }
            .ruta-app.muestra .split:has(.panel-comparar) .mapa-controles { bottom: calc(46% + 24px); }

            /* ── Tarjeta de vidrio flotante (misma receta que .glass del mapa) */
            .ruta-app.muestra .panel-lateral {
                position: absolute; z-index: 1100; display: flex; flex-direction: column; height: auto; min-height: 0; overflow: hidden;
                background: var(--glass-fondo); backdrop-filter: var(--glass-desenfoque); -webkit-backdrop-filter: var(--glass-desenfoque);
                border: 1px solid var(--border-default); border-radius: var(--radio-xl); box-shadow: var(--sombra-flotante);
            }
            .ruta-app.muestra .panel-scroll, .ruta-app.muestra .panel-comparar, .ruta-app.muestra .panel-rutas { overscroll-behavior: contain; }
            .ruta-app.muestra .campo { height: 50px !important; border-radius: var(--radio-md) !important; background: var(--bg-white) !important; border-color: transparent; }
            .ruta-app.muestra .campo.enfocado { border-color: var(--brand-primary); }
            .ruta-app.muestra .campo input { font-size: 16px; }   /* 16 px: iOS no hace zoom al enfocar */
            .ruta-app.muestra .btn, .ruta-app.muestra .btn-chico { min-height: 44px; }
            .ruta-app.muestra .btn-icono { min-width: 40px; min-height: 40px; }
            .ruta-app.muestra .btn-ghost { min-height: 40px; }

            /* ── Compartir no existe en móvil */
            .ruta-app.muestra .btn-icono[title="Compartir ruta"], .ruta-app.muestra .btn-icono[title="Compartir este viaje"],
            .ruta-app.muestra .nav-cabecera .btn-icono { display: none; }

            /* ── Planificar: tarjeta arriba, de lado a lado. Cerrada solo trae el destino. */
            .ruta-app.muestra .panel-lateral:has(> .panel-scroll) {
                top: calc(env(safe-area-inset-top) + var(--margen-flotante)); left: var(--margen-flotante); right: var(--margen-flotante);
                max-height: calc(100% - 2 * var(--margen-flotante));
            }
            .ruta-app.muestra .panel-scroll { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow-y: auto; padding: var(--espacio-2); }
            .ruta-app.muestra .encabezado-seccion, .ruta-app.muestra .panel-scroll > .codigo-bloque,
            .ruta-app.muestra .lugares-rapidos > :not(.eligiendo-mapa) { display: none; }
            .ruta-app.muestra .panel-scroll > .bloque { margin: 0; }
            .ruta-app.muestra .ubicacion-tarjeta { display: block; order: -1; padding: 2px var(--espacio-1) var(--espacio-3); }
            .ruta-app.muestra .ubicacion-tarjeta h2 { display: flex; align-items: center; gap: var(--espacio-2); margin: 0; font-size: var(--texto-lg); font-weight: var(--peso-titulo); line-height: var(--linea-compacta); color: var(--text-primary); }
            .ruta-app.muestra .ubicacion-tarjeta h2 .icono { flex: none; color: var(--brand-primary); }
            .ruta-app.muestra .ubicacion-tarjeta p { margin: var(--espacio-1) 0 0; font-size: var(--texto-sm); line-height: var(--linea-texto); color: var(--text-secondary); }
            .ruta-app.muestra .campo-wrap { margin-bottom: var(--espacio-2); }
            .ruta-app.muestra .bloque > .campo-wrap:nth-last-child(2) { margin-bottom: 0; }
            .ruta-app.muestra .eligiendo-mapa { margin: var(--espacio-2) 0 0; }
            /* Origen, "elegir en el mapa" y el botón solo aparecen al tocar el campo o cuando ya hay un destino escrito */
            .ruta-app.muestra .panel-lateral .bloque > .campo-wrap:first-child, .ruta-app.muestra .panel-lateral .campo-wrap.escala,
            .ruta-app.muestra .panel-lateral .acciones-campos, .ruta-app.muestra .panel-lateral .panel-scroll > .error-texto,
            .ruta-app.muestra .panel-lateral .panel-scroll > .btn-buscar { display: none; }
            .ruta-app.muestra .panel-lateral:is(:focus-within, :has(.bloque > .campo-wrap:not(:first-child):not(.escala) input:not(:placeholder-shown))) .bloque > .campo-wrap:first-child { display: block; }
            .ruta-app.muestra .panel-lateral:is(:focus-within, :has(.bloque > .campo-wrap:not(:first-child):not(.escala) input:not(:placeholder-shown))) .acciones-campos { display: flex; margin-top: var(--espacio-1); }
            .ruta-app.muestra .panel-lateral:is(:focus-within, :has(.bloque > .campo-wrap:not(:first-child):not(.escala) input:not(:placeholder-shown))) .panel-scroll > .error-texto { display: block; margin: var(--espacio-2) var(--espacio-1) 0; }
            .ruta-app.muestra .panel-lateral:is(:focus-within, :has(.bloque > .campo-wrap:not(:first-child):not(.escala) input:not(:placeholder-shown))) .panel-scroll > .btn-buscar { display: flex; margin: var(--espacio-2) 0 0; }
            .ruta-app.muestra .acciones-campos .btn-ghost:not(:last-child) { display: none; }
            .ruta-app.muestra .btn-buscar { padding: 14px; position: sticky; bottom: 0; z-index: 2; box-shadow: 0 0 0 var(--espacio-2) var(--glass-fondo); }   /* siempre a la vista, aunque la lista de lugares sea larga */

            /* Sugerencias dentro de la tarjeta: son una lista más */
            .ruta-app.muestra .sugerencias { position: static; max-height: none; margin: var(--espacio-1) 0 0; border: 0; border-radius: 0; background: transparent; box-shadow: none; }
            .ruta-app.muestra .sugerencia { align-items: center; padding: 10px 4px; border-bottom: var(--borde-suave); }
            .ruta-app.muestra .sugerencia:hover { background: transparent; }
            .ruta-app.muestra .sugerencia:active { background: var(--blue-50); }
            .ruta-app.muestra .sugerencias-titulo { padding: var(--espacio-1) var(--espacio-1) var(--espacio-1); }
            .ruta-app.muestra .sug-num { width: 32px; height: 32px; margin: 0; font-size: var(--texto-base); background: var(--blue-100); color: var(--brand-primary); }

            /* ── Rutas: la lista flota en el centro; al elegir una ruta se cierra y se ve el recorrido */
            .ruta-app.muestra .panel-lateral.panel-rutas {
                --tx: 0px; left: var(--margen-flotante); right: var(--margen-flotante); top: 50%; bottom: auto; max-height: min(72%, 560px);
                padding: var(--espacio-4) var(--espacio-4) var(--espacio-2); transform: translate(var(--tx), -50%);
                transition: opacity var(--dur-lenta) var(--curva), transform var(--dur-lenta) var(--curva), visibility 0s;
            }
            .ruta-app.muestra .panel-lateral.panel-rutas.cerrada {
                opacity: 0; visibility: hidden; pointer-events: none; transform: translate(var(--tx), -46%) scale(.96);
                transition: opacity var(--dur-lenta) var(--curva), transform var(--dur-lenta) var(--curva), visibility 0s linear var(--dur-lenta);
            }
            .ruta-app.muestra .panel-rutas > div:first-of-type { display: flex; align-items: center; justify-content: space-between; order: 0; margin-bottom: var(--espacio-3); }
            .ruta-app.muestra .split:has(.mapa-tarjeta) .panel-lateral.panel-rutas { top: calc(50% - 44px); max-height: min(64%, 480px); }
            .ruta-app.muestra .panel-rutas .titulo-panel { font-size: var(--texto-xl); }
            .ruta-app.muestra .panel-rutas .sub-panel { display: none; }
            .ruta-app.muestra .panel-cerrar { display: inline-flex; margin: -4px -8px -4px 0; order: 2; }
            .ruta-app.muestra .panel-rutas .busqueda-panel { display: none; }
            .ruta-app.muestra .panel-rutas .pestanas {
                order: 2; display: flex; gap: var(--espacio-2); margin: 0 0 var(--espacio-1); padding: 0; background: transparent; border: 0; box-shadow: none;
            }
            .ruta-app.muestra .panel-rutas .seg {
                flex: none; min-height: 40px; padding: 0 14px; border: var(--borde); border-radius: var(--radio-pill); background: var(--bg-white);
                font-size: var(--texto-base); color: var(--text-primary);
            }
            .ruta-app.muestra .panel-rutas .seg.activo { background: var(--brand-tint); border-color: var(--brand-primary); color: var(--brand-primary); }
            .ruta-app.muestra .panel-rutas .seg.activo .contador { background: var(--bg-white); }
            .ruta-app.muestra .panel-rutas .lista-rutas { order: 3; flex: 1; min-height: 0; margin-top: 0; overflow-y: auto; }
            .ruta-app.muestra .panel-rutas .nota-pie { display: none; }
            .ruta-app.muestra .ruta-item { margin: 0; padding: 12px 6px; border: 0; border-bottom: var(--borde-suave); border-radius: 0; background: transparent; }
            .ruta-app.muestra .ruta-item.sel { padding: 12px 6px; border: 0; border-radius: var(--radio-md); background: var(--blue-50); }
            .ruta-app.muestra .ruta-item:hover { background: transparent; }
            .ruta-app.muestra .ruta-item.sel:hover { background: var(--blue-50); }
            .ruta-app.muestra .ruta-numero { width: 42px; height: 42px; }
            .ruta-app.muestra .invitacion { margin-top: var(--espacio-3); border: 0; background: var(--bg-card); }

            /* ── Tarjeta de la ruta o del viaje que se ve en el mapa: abajo, solo lo esencial */
            .ruta-app.muestra .mapa-tarjeta, .ruta-app.muestra .tarjeta-viaje {
                top: auto; bottom: var(--margen-flotante); left: var(--margen-flotante); right: var(--margen-flotante); width: auto; max-width: none; max-height: none; overflow: visible;
                padding: var(--espacio-3) var(--espacio-4); border: 1px solid var(--border-default); border-radius: var(--radio-xl);
                background: var(--glass-fondo); box-shadow: var(--sombra-flotante);
            }
            .ruta-app.muestra .mapa-tarjeta > :not(.tarjeta-cabecera):not(.tarjeta-variantes):not(.viaje-cabecera) { display: none; }
            .ruta-app.muestra .tarjeta-viaje .viaje-chips { display: none; }
            .ruta-app.muestra .tarjeta-viaje .viaje-cabecera { justify-content: flex-start; }
            .ruta-app.muestra .tarjeta-viaje .viaje-cifras { width: 100%; justify-content: space-between; gap: var(--espacio-2); }
            .ruta-app.muestra .tarjeta-variantes { margin-top: var(--espacio-2); }
            .ruta-app.muestra .tarjeta-cabecera.tocable { cursor: pointer; }
            .ruta-app.muestra .tarjeta-cabecera.tocable .tarjeta-lista { display: inline-flex; flex: none; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: var(--radio-circulo); background: var(--brand-tint); color: var(--brand-primary); }
            .ruta-app.muestra .tarjeta-titulo b { font-size: var(--texto-md); }
            .ruta-app.muestra .tarjeta-numero { width: 34px; height: 34px; font-size: var(--texto-lg); }
            .ruta-app.muestra .split:has(.panel-comparar) .mapa-tarjeta { display: none; }

            /* ── Comparar: las opciones en una tarjeta abajo; el viaje en curso, en la misma */
            .ruta-app.muestra .panel-lateral.panel-comparar { left: var(--margen-flotante); right: var(--margen-flotante); bottom: var(--margen-flotante); top: auto; height: 46%; padding: var(--espacio-3); }
            .ruta-app.muestra .panel-comparar .volver, .ruta-app.muestra .panel-comparar .encabezado-seccion, .ruta-app.muestra .opcion-resumen,
            .ruta-app.muestra .opcion-linea { display: none; }
            .ruta-app.muestra .panel-comparar .resumen-viaje { flex: none; margin: 0 0 var(--espacio-2); padding: 0 0 var(--espacio-2); border: 0; border-radius: 0; background: transparent; box-shadow: none; border-bottom: var(--borde-suave); }
            .ruta-app.muestra .panel-comparar .lista-opciones { flex: 1; min-height: 0; margin-top: 0; overflow-y: auto; }
            .ruta-app.muestra .panel-comparar .btn-buscar { flex: none; margin: var(--espacio-2) 0 0; }
            .ruta-app.muestra .opcion-ruta { padding: 12px 14px; margin-bottom: var(--espacio-2); border: var(--borde-suave); border-radius: var(--radio-lg); background: var(--bg-white); }
            .ruta-app.muestra .opcion-ruta.sel { padding: 12px 14px; border-color: var(--brand-primary); background: var(--blue-50); box-shadow: 0 0 0 1px var(--brand-primary); }
            .ruta-app.muestra .opcion-nombre { font-size: var(--texto-md); }
            .ruta-app.muestra .opcion-datos { margin-top: var(--espacio-2); }
            .ruta-app.muestra .opcion-datos b { font-size: var(--texto-xl); font-weight: var(--peso-titulo); }
            .ruta-app.muestra .panel-comparar .navegacion { flex: 1; min-height: 0; overflow-y: auto; gap: var(--espacio-3); }
            .ruta-app.muestra .split:has(.navegacion) .mapa-tarjeta { display: none; }
            .ruta-app.muestra .nav-actual { padding: var(--espacio-4); border-radius: var(--radio-lg); }
            .ruta-app.muestra .nav-icono { width: 48px; height: 48px; }
            .ruta-app.muestra .nav-datos { gap: 0; padding-bottom: var(--espacio-3); border-bottom: var(--borde-suave); }
            .ruta-app.muestra .nav-datos div { padding: 0; border: 0; background: transparent; align-items: flex-start; }
            .ruta-app.muestra .pasos-nav li.actual { background: var(--blue-50); border: 0; padding: 10px; box-shadow: inset 0 0 0 1px var(--c); }

            /* ── Avisos de carga flotando en el centro del mapa */
            .ruta-app.muestra .split > .cargando-mapa { top: 36%; left: 50%; }

            /* ── Botones planos: sin degradados ni sombras de color */
            .ruta-app.muestra .btn-primario { background: var(--brand-primary); box-shadow: none; }
            .ruta-app.muestra .btn-primario:hover:not(:disabled) { filter: none; opacity: .92; box-shadow: none; }
            .ruta-app.muestra .btn-primario:active:not(:disabled) { filter: none; opacity: .8; box-shadow: none; }
            .ruta-app.muestra .btn-contorno, .ruta-app.muestra .btn-sinborde { background: var(--bg-white); box-shadow: none; }
            .ruta-app.muestra .btn-sinborde { border: var(--borde); color: var(--text-primary); }
            .ruta-app.muestra .btn-sinborde .icono { color: var(--brand-primary); }

            /* ── Landing: la foto de la ciudad arriba; texto y botones al alcance del pulgar */
            .ruta-app.muestra .landing { position: relative; height: 100dvh; min-height: 0; overflow-x: hidden; overflow-y: auto; background: var(--bg-white); }
            .ruta-app.muestra .landing-cabecera { position: absolute; z-index: 5; top: 0; left: 0; right: 0; padding: calc(env(safe-area-inset-top) + 14px) var(--espacio-4) 0; }
            .ruta-app.muestra .landing-cabecera .marca-texto b, .ruta-app.muestra .landing-cabecera .marca-texto small { color: var(--text-on-brand); text-shadow: 0 1px 6px rgba(10, 25, 60, .45); white-space: nowrap; }
            .ruta-app.muestra .landing-cabecera .marca-logo { width: 38px; height: 38px; }
            .ruta-app.muestra .landing-cabecera .btn-primario {
                min-height: 38px; padding: 0 var(--espacio-3); white-space: nowrap; font-size: var(--texto-base);
                background: rgba(255, 255, 255, .2); backdrop-filter: blur(8px); border: 1px solid rgba(255, 255, 255, .45); color: var(--text-on-brand);
            }
            .ruta-app.muestra .landing-contenido { flex: 1 0 auto; display: flex; flex-direction: column; align-items: stretch; gap: 0; padding: 0; min-height: 0; }
            .ruta-app.muestra .landing-tarjeta { order: -1; flex: 1 0 clamp(210px, 40dvh, 400px); width: 100%; max-width: none; height: auto; margin: 0; border: 0; border-radius: 0 0 28px 28px; box-shadow: none; }
            .ruta-app.muestra .landing-texto { flex: none; padding: var(--espacio-5) var(--espacio-5) calc(var(--espacio-5) + env(safe-area-inset-bottom)); }
            .ruta-app.muestra .bienvenida { display: none; }
            .ruta-app.muestra .landing-texto h1 { font-size: clamp(23px, 7.4vw, 30px); line-height: 1.2; background: none; color: var(--text-primary); -webkit-text-fill-color: currentColor; }
            .ruta-app.muestra .landing-texto p { margin-top: var(--espacio-2); font-size: var(--texto-md); color: var(--text-secondary); }
            .ruta-app.muestra .landing-botones { flex-direction: column; gap: var(--espacio-2); margin-top: var(--espacio-4); }
            .ruta-app.muestra .landing-botones .btn { width: 100%; min-height: 50px; padding-top: 12px; padding-bottom: 12px; justify-content: center; }
            .ruta-app.muestra .landing-nota { margin-top: var(--espacio-4); padding-top: var(--espacio-3); border-top: var(--borde-suave); text-align: left; font-size: var(--texto-sm); }
            .ruta-app.muestra .landing-tarjeta .slide-foto-titulo { top: 72px; font-size: var(--texto-xl); line-height: 1.25; }
            .ruta-app.muestra .landing-tarjeta .slide-foto-ciudad { bottom: 36px; font-size: var(--texto-xl); }
            .ruta-app.muestra .landing-tarjeta .slide-marca-contenido { padding: 76px var(--espacio-5) var(--espacio-8); }
            .ruta-app.muestra .landing-tarjeta .slide-titular { font-size: var(--texto-2xl); line-height: 1.2; }
            .ruta-app.muestra .landing-tarjeta .slide-subtitular { display: none; }
            .ruta-app.muestra .landing-tarjeta .carrusel-flecha { display: none; }
            .ruta-app.muestra .landing-tarjeta .carrusel-puntos { bottom: var(--espacio-3); }

            /* ── Pantallas "Disponible próximamente" */
            .ruta-app.muestra .proximamente { min-height: 100%; padding: var(--espacio-8) var(--espacio-5); }
            .ruta-app.muestra .proximamente h1 { font-size: var(--texto-2xl); }
            .ruta-app.muestra .proximamente .estado-accion { flex-direction: column; align-self: stretch; }
            .ruta-app.muestra .proximamente .btn { justify-content: center; }
        }

        @media (max-width: 380px) {
            .ruta-app.muestra .landing-cabecera .marca-texto small { display: none; }
        }

        /* ── Tableta vertical (600–820 px): las tarjetas no se estiran por todo el ancho */
        @media (min-width: 600px) and (max-width: 820px) {
            .ruta-app.muestra .panel-lateral:has(> .panel-scroll) { right: auto; width: min(440px, calc(100% - 24px)); }
            .ruta-app.muestra .panel-lateral.panel-rutas { --tx: -50%; left: 50%; right: auto; width: min(480px, calc(100% - 24px)); }
            .ruta-app.muestra .panel-lateral.panel-comparar { right: auto; width: min(440px, calc(100% - 24px)); }
            .ruta-app.muestra .mapa-tarjeta, .ruta-app.muestra .tarjeta-viaje { right: auto; width: min(440px, calc(100% - 24px)); }
            .ruta-app.muestra .split:has(.panel-comparar) .mapa-controles { bottom: 28px; }
            .ruta-app.muestra .landing-tarjeta .slide-foto-titulo { font-size: var(--texto-3xl); }
        }
        """;
}
