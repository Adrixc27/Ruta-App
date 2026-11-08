namespace RutaApp.Interfaz.Estilos;

// Adaptación a distintos anchos y detalles del mapa.
public static class CssResponsivo
{
    public const string Reglas = """
        @media (max-width: 1280px) {
            .split-380 { grid-template-columns: 340px 1fr; }
            .split-400 { grid-template-columns: 360px 1fr; }
            .landing-contenido { grid-template-columns: 1fr 520px; padding-left: 64px; }
            .landing-tarjeta { width: 520px; height: 560px; }
            .landing-cabecera { padding-left: 64px; padding-right: 48px; }
            .mapa-tarjeta { width: 420px; }
        }

        @media (max-width: 1100px) {
            .landing-contenido { grid-template-columns: 1fr; padding: 24px 32px 40px; }
            .landing-cabecera { padding: 22px 32px 0; }
            .landing-tarjeta { width: 100%; max-width: 640px; margin: 0 auto; }
            .landing-texto h1 { font-size: 44px; line-height: 54px; }
            .perfil-grid { grid-template-columns: 1fr; }
        }

        @media (max-width: 900px) {
            /* Pantallas con panel + mapa: el mapa arriba y el panel debajo */
            .split, .split-380, .split-400 { grid-template-columns: 1fr; grid-template-rows: minmax(260px, 48%) minmax(0, 1fr); }
            .split > .mapa-contenedor { order: -1; }
            .panel-lateral { border-right: 0; border-top: 1px solid var(--border-default); }
            .panel-scroll, .panel-comparar, .panel-rutas { padding: 16px; }
            .mapa-banner { top: 8px; left: 8px; max-width: calc(100% - 64px); padding: 8px 12px; font-size: 12px; }
            .banner-circ { width: 30px; height: 30px; }
            .mapa-tarjeta, .tarjeta-viaje { left: 8px; right: 8px; bottom: 8px; width: auto; max-width: none; max-height: 60%; padding: 12px; }
            .mapa-tarjeta .tarjeta-datos, .mapa-tarjeta .tarjeta-plano { display: none; }
            .mapa-leyenda { right: 56px; bottom: auto; top: 56px; width: 190px; }
            .mapa-controles { top: 86px; }
            .carrusel-flecha { width: 36px; height: 36px; margin-top: -18px; }
        }

        @media (max-width: 820px) {
            /* El menú lateral pasa a una barra inferior */
            .app-shell { flex-direction: column-reverse; height: 100dvh; }
            .sidebar, .sidebar.colapsado {
                width: 100%; height: 62px; flex-direction: row; align-items: center; padding: 4px 10px; overflow: visible;
                border-right: 0; border-top: 1px solid var(--border-default); box-shadow: 0 -2px 14px rgba(37, 99, 235, .12);
            }
            .sidebar-marca, .sidebar-etiqueta, .nav-contraer, .nav-btn span, .nav-btn.activo::before { display: none; }
            .sidebar-menu { margin: 0; flex: 1; display: flex; justify-content: space-around; align-items: center; }
            .nav-btn, .sidebar-pie .nav-btn { width: auto; margin: 0; padding: 10px 14px; justify-content: center; }
            .sidebar-pie { display: flex; }
            .sidebar-sesion { margin: 0; padding: 0; border: 0; }
            .sidebar-sesion .nav-btn { padding: 10px 12px; }
            .nav-badge, .sidebar.colapsado .nav-badge { position: absolute; top: 2px; left: 26px; }
            .barra-ubicacion { height: 42px; padding: 0 12px; }
            .miga-inicio span, .usuario-nombre { display: none; }
            .migas { font-size: 12px; }

            .landing-cabecera { padding: 16px; }
            .landing-contenido { padding: 16px 16px 32px; }
            .landing-texto h1 { font-size: 34px; line-height: 42px; }
            .landing-texto p { font-size: 16px; }
            .landing-botones { flex-wrap: wrap; margin-top: 28px; }
            .landing-tarjeta { height: 440px; border-radius: 24px; }
            .landing-tarjeta .slide-titular { font-size: 24px; line-height: 30px; }
            .landing-tarjeta .slide-marca-contenido { padding: 24px; }
            .btn-ghost-oscuro { margin-right: 4px; padding: 8px; }
            .btn-grande { padding: 14px 20px; font-size: 15px; }
            .intro-nombre { font-size: 88px; }
            .intro-lema { font-size: 15px; padding: 0 20px; }

            .auth-panel { padding: 16px; align-items: flex-start; }
            .auth-tarjeta { padding: 24px; }
            .sobre-cabecera { height: auto; padding: 14px 16px; }
            .sobre-cuerpo { padding: 20px 16px; }

            .pagina { padding: 18px 14px 20px; }
            .pagina-cabecera { flex-wrap: wrap; gap: 12px; }
            .pagina-cabecera h1 { font-size: 24px; }
            .busqueda-ancha { width: 100%; }
            .feed { grid-template-columns: 1fr; }
            .filtros { align-items: stretch; }
            .pills { flex-wrap: wrap; }
            .filtros-derecha { width: 100%; }
            .categorias { grid-template-columns: 1fr; }
            .perfil-datos { padding: 22px; }
            .pestanas-perfil { grid-template-columns: 1fr; }
            .marco-detalle { padding: 18px 14px 10px; }
            .marco-titulo h1 { font-size: 24px; }
            .dos-columnas { grid-template-columns: 1fr !important; }
            .tres-columnas, .dos-campos, .dos-columnas-iguales { grid-template-columns: 1fr; }
            .tipos, .reporte-tipos { grid-template-columns: repeat(2, 1fr); }
            .modal { padding: 20px; }
            .modal-sesion { padding: 28px 20px; }
            .modal-fondo { padding: 12px; align-items: flex-start; }
            .toast-ajuste { right: 14px; bottom: 76px; }
            .pie-config { flex-direction: column; align-items: flex-start; gap: 8px; }
        }

        @media (max-width: 480px) {
            .landing-texto h1 { font-size: 28px; line-height: 36px; }
            .landing-nav .btn-chico { padding: 8px 10px; font-size: 13px; }
            .viaje-cifras { gap: 14px; }
            .reporte-tipos { grid-template-columns: repeat(2, 1fr); }
            .nav-actual b { font-size: 16px; }
            .carga-caja { width: 220px; }
        }


        /* Título grande de la sección (reemplaza al indicador 1-2-3) */
        .encabezado-seccion { margin-bottom: 24px; }
        .titulo-seccion { font-size: 30px; line-height: 36px; font-weight: 900; color: var(--text-primary); }
        .encabezado-seccion .sub-panel { font-size: 15px; margin-top: 8px; }
        .resumen-viaje { margin-top: 0; }

        /* El mapa se adapta a su propio ancho (no al de la ventana): tarjeta y leyenda no tapan la ruta */
        .mapa-contenedor { container-type: inline-size; container-name: mapa; }
        @container mapa (max-width: 720px) {
            .mapa-tarjeta, .tarjeta-viaje { left: 8px; right: 8px; bottom: 8px; width: auto; max-width: none; max-height: 42%; padding: 12px; }
            .mapa-tarjeta .tarjeta-datos, .mapa-tarjeta .tarjeta-plano { display: none; }
            .mapa-leyenda { right: 56px; bottom: auto; top: 56px; width: 190px; }
            .mapa-banner { top: 8px; left: 8px; max-width: calc(100% - 64px); padding: 8px 12px; font-size: 12px; }
            .mapa-controles { top: 86px; }
            .tarjeta-boton { padding: 9px; font-size: 13px; }
        }

        /* Ventanas medianas: el menú lateral queda solo con iconos para dejar más espacio al mapa */
        @media (min-width: 821px) and (max-width: 1200px) {
            .sidebar, .sidebar.colapsado { width: 84px; padding: 20px 14px; }
            .sidebar .marca-texto, .sidebar .nav-btn span, .sidebar-etiqueta, .nav-contraer { display: none; }
            .sidebar .nav-btn { justify-content: center; padding: 12px; }
            .sidebar .nav-btn.activo::before { left: -14px; }
            .sidebar-marca { justify-content: center; }
            .split-380 { grid-template-columns: 320px 1fr; }
            .split-400 { grid-template-columns: 340px 1fr; }
        }

        /* Zona donde subir o bajar (el camión pasa más cerca que cualquier parada): halo para que se note */
        .mk-parada.zona { border-style: dashed; box-shadow: 0 0 0 9px color-mix(in srgb, var(--c, #2563EB) 22%, transparent), 0 1px 4px rgba(0, 0, 0, .3); }

        /* Elegir un lugar tocando el mapa */
        .mapa-seleccion, .mapa-seleccion .leaflet-interactive, .mapa-seleccion .leaflet-grab { cursor: crosshair !important; }
        .eligiendo-mapa { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 10px; border-radius: 12px; background: var(--grad-suave); border: 1px solid var(--brand-primary); font-size: 13px; font-weight: 700; }
        .eligiendo-mapa span { flex: 1; }
        .acciones-campos { display: flex; flex-wrap: wrap; gap: 4px 12px; }
        .lugar-elegido { display: flex; align-items: center; gap: 8px; font-size: 13px; }
        .chip-lugar-grupo { display: inline-flex; align-items: stretch; }
        .chip-lugar-grupo .chip-lugar { border-top-right-radius: 0; border-bottom-right-radius: 0; }
        .chip-lugar-mapa { display: inline-flex; align-items: center; justify-content: center; padding: 0 9px; border: 1px solid var(--blue-200); border-left: 0; border-radius: 0 999px 999px 0; background: var(--blue-50); color: var(--brand-primary); cursor: pointer; }
        .chip-lugar-mapa:hover { background: var(--blue-100); border-color: var(--brand-primary); }


        /* Mapa menos saturado */
        .banner-viaje { min-width: 0; }
        .banner-viaje b, .banner-viaje small { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .mapa-banner span { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .ver-pasos { display: inline-flex; align-items: center; gap: 4px; margin-top: 8px; padding: 4px 6px; border: 0; background: transparent; color: var(--brand-primary); font-size: 12px; font-weight: 700; cursor: pointer; border-radius: 8px; }
        .ver-pasos:hover { background: var(--blue-100); }
        .tarjeta-viaje .viaje-cabecera { flex-direction: row; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 8px 16px; }
        .tarjeta-viaje .viaje-cifras { gap: 16px; }
        .tarjeta-viaje .viaje-cifras b { font-size: 15px; }
        .tarjeta-viaje .pasos { margin-top: 6px; max-height: 38vh; overflow-y: auto; }
        @container mapa (max-width: 720px) {
            .mapa-tarjeta, .tarjeta-viaje { max-height: 38%; }
            .tarjeta-viaje .chip-ruta { font-size: 10px; padding: 3px 7px; }
            .mapa-leyenda { width: 170px; padding: 10px 12px; }
            .leyenda-item { font-size: 12px; margin-bottom: 6px; }
        }

        /* Viaje en curso: título con el icono de compartir y botones que caben */
        .nav-cabecera { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
        .nav-cabecera .nav-titulo { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .nav-cabecera .btn-icono { flex: none; }
        .nav-acciones { flex-wrap: wrap; }
        .nav-acciones .btn { flex: 1 1 130px; white-space: nowrap; padding: 12px 10px; }
        .panel-comparar { overflow-x: hidden; }
        .navegacion { overflow-x: hidden; }


        /* Instrucciones para caminar y detalles de la ruta */
        .instruccion { display: block; position: relative; margin-top: 4px; padding-left: 12px; font-size: 12px; color: var(--text-primary); line-height: 1.35; }
        .instruccion::before { content: ""; position: absolute; left: 0; top: .55em; width: 5px; height: 5px; border-radius: 50%; background: var(--slate); }
        .detalles-ruta { margin: 6px 0 12px; padding: 10px 12px; border-radius: 12px; background: var(--blue-50); border: 1px solid var(--blue-200); display: grid; gap: 6px; max-height: 30vh; overflow-y: auto; }
        .detalles-ruta div { display: grid; grid-template-columns: 96px 1fr; gap: 8px; font-size: 12px; }
        .detalles-ruta dt { color: var(--text-secondary); font-weight: 700; }
        .detalles-ruta dd { margin: 0; font-weight: 600; }


        /* Guardar un lugar: pasos claros */
        .paso-form { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 800; color: var(--text-primary); margin-top: 4px; }
        .paso-form span { width: 20px; height: 20px; border-radius: 50%; background: var(--brand-primary); color: #fff; font-size: 11px; display: inline-flex; align-items: center; justify-content: center; }
        .lugar-opciones { display: flex; flex-wrap: wrap; gap: 8px; }
        .lugar-opciones .btn { flex: 1 1 120px; }
        .lugar-elegido { padding: 8px 10px; border-radius: 10px; background: var(--bg-panel); }
        .lugar-elegido.ok { background: var(--success-tint); }
        .aviso-compartido { display: flex; align-items: center; gap: 8px; margin-top: 10px; padding: 10px 12px; border-radius: 12px; background: var(--blue-100); font-size: 12px; font-weight: 600; }
        .aviso-compartido span { flex: 1; }
        """;
}
