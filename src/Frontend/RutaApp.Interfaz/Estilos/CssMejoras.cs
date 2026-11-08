namespace RutaApp.Interfaz.Estilos;

/// <summary>Acentos de color, carga, barra de ubicación, marcadores, viaje en curso, perfil y lugares.</summary>
public static class CssMejoras
{
    public const string Reglas = """
        /* Cada sección del menú tiene su color (el botón activo sigue siendo el degradado de la marca) */
        .sidebar-menu .nav-btn:nth-of-type(1):not(.activo) .icono { color: var(--brand-primary); }
        .sidebar-menu .nav-btn:nth-of-type(2):not(.activo) .icono { color: var(--violet); }
        .sidebar-menu .nav-btn:nth-of-type(3):not(.activo) .icono { color: var(--teal); }
        .sidebar-menu .nav-btn:nth-of-type(4):not(.activo) .icono { color: var(--orange); }
        .sidebar-menu .nav-btn:nth-of-type(5):not(.activo) .icono { color: var(--slate); }

        /* Números de ruta: cada ruta tiene su color (variable --c que pone la pantalla) */
        .ruta-numero { background: color-mix(in srgb, var(--c, #2563EB) 16%, #fff); color: var(--c, var(--brand-primary)); }
        .ruta-item.sel .ruta-numero { background: var(--c, var(--brand-primary)); color: #fff; }
        .ruta-item.sel { border-color: var(--c, var(--brand-primary)); }
        .ruta-mini { color: #fff; font-size: 14px; font-weight: 900; }
        .trayecto-icono[style*="background"] { background-image: none; }

        .chip-ruta { display: inline-flex; align-items: center; gap: 5px; padding: 4px 10px; border-radius: 8px; font-size: 11px; font-weight: 700;
            background: color-mix(in srgb, var(--c, #2563EB) 14%, #fff); color: var(--c, var(--brand-primary)); border: 1px solid color-mix(in srgb, var(--c, #2563EB) 30%, #fff); }
        .viaje-chips, .opcion-linea { display: flex; flex-wrap: wrap; gap: 6px; }
        .opcion-resumen { display: block; margin-top: 8px; font-size: 12px; color: var(--text-secondary); }

        /* Tipos de publicación y etiquetas con más color */
        .post-tipo { background: var(--amber-tint); color: var(--amber); }
        .post-tipo.pregunta { background: var(--blue-100); color: var(--blue-700); }
        .post-tipo.propuesta { background: var(--teal-tint); color: var(--teal); }
        .etiqueta-variantes { background: var(--violet-tint); color: var(--violet); }
        .importar-codigo { margin-top: 10px; align-self: flex-start; }

        /* Botones neutros y nuevos controles */
        .btn-suave { color: var(--text-primary); }
        .btn-suave:hover { background: var(--bg-white); border-color: var(--blue-300); }
        .titulo-mini { font-size: 14px; font-weight: 900; }
        .label-upper.sin-margen { margin: 0 0 6px; }

        /* Carrusel: flechas y puntos */
        .carrusel-flecha {
            position: absolute; top: 50%; z-index: 6; width: 42px; height: 42px; margin-top: -21px; border: 0; border-radius: 50%;
            background: rgba(255, 255, 255, .88); color: var(--text-primary); display: flex; align-items: center; justify-content: center;
            cursor: pointer; box-shadow: 0 4px 14px rgba(27, 54, 93, .3); transition: transform .15s, background .15s;
        }
        .carrusel-flecha:hover { background: #fff; transform: scale(1.08); }
        .carrusel-flecha.izquierda { left: 14px; }
        .carrusel-flecha.derecha { right: 14px; }
        .carrusel-puntos { gap: 2px; }
        .carrusel-puntos .punto { border: 0; padding: 0; }
        .carrusel-puntos .punto:focus-visible, .carrusel-flecha:focus-visible { outline: 2px solid #fff; outline-offset: 2px; }

        /* Animación de entrada: se reproduce una vez */
        .intro-pista { opacity: .75; font-size: 14px; }

        /* Pantalla de carga */
        .carga { position: fixed; inset: 0; z-index: 4000; display: flex; align-items: center; justify-content: center;
            background: linear-gradient(160deg, var(--blue-50) 0%, var(--bg-panel) 50%, var(--bg-panel-2) 100%); animation: carga-entra .2s ease-out; }
        @keyframes carga-entra { from { opacity: 0; } to { opacity: 1; } }
        .carga-caja { display: flex; flex-direction: column; align-items: center; gap: 14px; width: 260px; }
        .carga-camion { width: 84px; height: 84px; border-radius: 24px; background: var(--grad-marca); color: #fff; display: flex; align-items: center; justify-content: center;
            box-shadow: var(--sombra-azul); animation: carga-bote 1s ease-in-out infinite; }
        @keyframes carga-bote { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-6px); } }
        .carga-carretera { position: relative; width: 100%; height: 6px; border-radius: 3px; background: var(--blue-200); overflow: hidden; }
        .carga-carretera i { position: absolute; top: 0; bottom: 0; width: 40%; border-radius: 3px; background: var(--grad-marca); animation: carga-avanza 1.2s ease-in-out infinite; }
        @keyframes carga-avanza { 0% { left: -40%; } 100% { left: 100%; } }
        .carga-marca { font-size: 26px; font-weight: 900; color: var(--brand-primary); letter-spacing: .05em; }
        .carga-mensaje { font-size: 14px; color: var(--text-secondary); font-weight: 600; text-align: center; }

        /* Barra de ubicación: dónde estás */
        .contenido { display: flex; flex-direction: column; }
        .barra-ubicacion {
            flex: none; height: 48px; padding: 0 20px; display: flex; align-items: center; justify-content: space-between; gap: 12px;
            background: rgba(255, 255, 255, .6); backdrop-filter: blur(8px); border-bottom: 1px solid var(--blue-200);
        }
        .migas { display: flex; align-items: center; gap: 6px; min-width: 0; font-size: 13px; color: var(--text-secondary); }
        .migas .icono { flex: none; }
        .miga-inicio { display: inline-flex; align-items: center; gap: 5px; font-weight: 600; padding: 4px 8px; border-radius: 8px; }
        .miga-inicio:hover { background: var(--blue-100); color: var(--brand-primary); }
        .miga { font-weight: 600; white-space: nowrap; }
        .miga.actual { color: var(--text-primary); font-weight: 800; padding: 3px 10px; border-radius: 999px; background: var(--blue-100); }
        .barra-usuario { display: flex; align-items: center; gap: 8px; flex: none; }
        .usuario-chip { display: inline-flex; align-items: center; gap: 8px; padding: 3px 10px 3px 3px; border-radius: 999px; background: #fff; border: 1px solid var(--blue-200); font-size: 13px; font-weight: 700; }
        .usuario-chip:hover { border-color: var(--blue-300); background: var(--blue-50); }
        .usuario-avatar { width: 28px; height: 28px; border-radius: 50%; background: var(--grad-marca) center/cover; color: #fff; font-size: 11px; font-weight: 900; display: flex; align-items: center; justify-content: center; }
        .usuario-nombre { max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .chip-invitado { font-size: 11px; font-weight: 700; padding: 4px 10px; border-radius: 999px; background: var(--amber-tint); color: var(--amber); }
        .contenido-cuerpo { position: relative; flex: 1; min-height: 0; overflow: hidden; }
        .contenido-cuerpo > .split, .contenido-cuerpo > .pagina, .contenido-cuerpo > .marco-detalle { position: absolute; inset: 0; height: 100%; }
        .nav-btn.activo::before { content: ""; position: absolute; left: -24px; top: 10px; bottom: 10px; width: 4px; border-radius: 0 4px 4px 0; background: var(--brand-primary); }
        .sidebar.colapsado .nav-btn.activo::before { left: -24px; }

        /* Aviso "inicia sesión" */
        .aviso-sesion-icono { width: 52px; height: 52px; border-radius: 16px; background: var(--blue-100); display: flex; align-items: center; justify-content: center; margin-bottom: 14px; }
        .modal-dialogo .modal-acciones .btn { padding: 11px 18px; }

        /* Mapa: marcadores con icono */
        .m-o, .m-d, .m-n, .m-s, .m-p, .m-r { display: none; }
        .mk-pin { position: relative; width: 36px; height: 40px; --pc: var(--brand-primary); }
        .mk-pin-o { --pc: #16A34A; }
        .mk-pin-d { --pc: #E11D48; }
        .mk-pin-cuerpo {
            position: absolute; left: 3px; top: 0; width: 30px; height: 30px; border-radius: 50% 50% 50% 0; transform: rotate(-45deg);
            background: var(--pc); border: 3px solid #fff; box-shadow: 0 3px 8px rgba(0, 0, 0, .35); color: #fff; display: flex; align-items: center; justify-content: center;
        }
        .mk-pin-cuerpo svg { transform: rotate(45deg); }
        .mk-num { width: 26px; height: 26px; border-radius: 50%; background: var(--violet); color: #fff; font-weight: 800; font-size: 12px; border: 3px solid #fff;
            box-shadow: 0 2px 6px rgba(0, 0, 0, .35); display: flex; align-items: center; justify-content: center; }
        .mk-parada { width: 20px; height: 20px; border-radius: 50%; background: #fff; color: var(--c, var(--brand-primary)); border: 2px solid var(--c, var(--brand-primary));
            box-shadow: 0 1px 4px rgba(0, 0, 0, .3); display: flex; align-items: center; justify-content: center; }
        .mk-parada.grande { width: 28px; height: 28px; border-width: 3px; }
        .mk-poi { width: 22px; height: 22px; border-radius: 50%; background: var(--c); color: #fff; border: 2px solid #fff; box-shadow: 0 1px 4px rgba(0, 0, 0, .3); display: flex; align-items: center; justify-content: center; }
        .mk-yo { position: relative; width: 26px; height: 26px; display: flex; align-items: center; justify-content: center; }
        .mk-yo::before { content: ""; position: absolute; inset: 0; border-radius: 50%; background: rgba(37, 99, 235, .28); animation: pulso 1.8s ease-out infinite; }
        .mk-yo i { position: relative; width: 14px; height: 14px; border-radius: 50%; background: #2563EB; border: 3px solid #fff; box-shadow: 0 1px 4px rgba(0, 0, 0, .45); }
        @keyframes pulso { 0% { transform: scale(.4); opacity: 1; } 100% { transform: scale(1.7); opacity: 0; } }
        .leaflet-tooltip.pill { max-width: 260px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

        /* Banner, controles y leyenda */
        .banner-circ { display: flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-width: 2px; }
        .mapa-controles { position: absolute; z-index: 1000; top: 96px; right: 10px; display: flex; flex-direction: column; gap: 8px; }
        .mapa-control {
            width: 34px; height: 34px; border-radius: 8px; border: 2px solid rgba(0, 0, 0, .2); background: #fff; color: var(--text-primary);
            display: flex; align-items: center; justify-content: center; cursor: pointer; padding: 0;
        }
        .mapa-control:hover { background: var(--blue-50); color: var(--brand-primary); }
        .mapa-control.activo { background: var(--brand-primary); color: #fff; }
        .mapa-leyenda { width: 200px; right: 24px; bottom: 28px; padding: 14px 16px; }
        .leyenda-titulo { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
        .leyenda-cerrar { border: 0; background: transparent; color: var(--text-secondary); cursor: pointer; font-size: 12px; padding: 0 2px; }
        .leyenda-item { gap: 10px; }
        .leg-icono { width: 20px; height: 20px; border-radius: 50%; flex: none; display: inline-flex; align-items: center; justify-content: center; color: #fff; background: var(--pc, var(--slate)); }
        .leg-icono.mk-pin-o { --pc: #16A34A; }
        .leg-icono.mk-pin-d { --pc: #E11D48; }
        .leg-icono.leg-num { background: var(--violet); font-size: 11px; font-weight: 800; }
        .leg-icono.leg-parada { background: #fff; color: var(--brand-primary); border: 2px solid var(--brand-primary); }
        .leg-icono.leg-yo { background: rgba(37, 99, 235, .25); }
        .leg-icono.leg-yo i { width: 10px; height: 10px; border-radius: 50%; background: #2563EB; border: 2px solid #fff; display: block; }
        .leg-linea { width: 20px; height: 0; border-top: 4px solid var(--brand-primary); border-radius: 2px; flex: none; }
        .leg-linea.punteada { border-top: 4px dotted var(--slate); }
        .leyenda-item i { width: auto; height: auto; border-radius: 0; }

        /* Tarjeta del viaje y pasos */
        .mapa-tarjeta { max-height: calc(100% - 100px); overflow-y: auto; }
        .tarjeta-viaje { width: 440px; padding: 16px 18px; }
        .viaje-cabecera { display: flex; flex-direction: column; gap: 10px; }
        .viaje-cifras { display: flex; gap: 22px; }
        .viaje-cifras div { display: flex; flex-direction: column; }
        .viaje-cifras b { font-size: 16px; font-weight: 800; }
        .viaje-cifras small { font-size: 11px; color: var(--text-secondary); }
        .pasos { list-style: none; margin: 14px 0 0; padding: 0; display: flex; flex-direction: column; gap: 2px; }
        .pasos li { display: flex; align-items: center; gap: 12px; padding: 8px 10px; border-radius: 10px; cursor: pointer; position: relative; }
        .pasos li:hover { background: var(--blue-50); }
        .pasos li + li::before { content: ""; position: absolute; left: 24px; top: -6px; height: 10px; border-left: 2px dotted var(--border-default); }
        .paso-icono { width: 30px; height: 30px; border-radius: 50%; flex: none; display: flex; align-items: center; justify-content: center; background: var(--c, var(--slate)); color: #fff; }
        .pasos li div { display: flex; flex-direction: column; min-width: 0; }
        .pasos li b { font-size: 14px; font-weight: 700; }
        .pasos li small { font-size: 12px; color: var(--text-secondary); }
        .pasos li.hecho { opacity: .55; }
        .pasos li.actual { background: var(--grad-suave); }
        .tarjeta-viaje .tarjeta-boton { margin-top: 12px; }
        .alerta-ruta { display: flex; align-items: center; gap: 8px; margin-top: 12px; padding: 8px 12px; border-radius: 10px; background: var(--amber-tint); color: var(--amber); font-size: 12px; cursor: pointer; }
        .alerta-ruta span { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .alerta-ruta b { font-weight: 800; }

        /* Viaje en curso (navegación paso a paso) */
        .navegacion { display: flex; flex-direction: column; gap: 16px; overflow-y: auto; min-height: 0; }
        .nav-actual { display: flex; gap: 16px; align-items: center; padding: 18px; border-radius: 18px; color: #fff;
            background: linear-gradient(135deg, var(--c) 0%, color-mix(in srgb, var(--c) 70%, #000) 100%); box-shadow: 0 10px 24px rgba(27, 54, 93, .25); }
        .nav-icono { width: 54px; height: 54px; border-radius: 16px; background: rgba(255, 255, 255, .22); display: flex; align-items: center; justify-content: center; flex: none; }
        .nav-actual div { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
        .nav-actual small { font-size: 11px; font-weight: 700; letter-spacing: .08em; opacity: .85; }
        .nav-actual b { font-size: 19px; font-weight: 900; }
        .nav-actual span { font-size: 13px; opacity: .92; }
        .nav-actual em { font-style: normal; font-size: 12px; font-weight: 700; margin-top: 4px; padding: 3px 8px; border-radius: 6px; background: rgba(255, 255, 255, .22); align-self: flex-start; }
        .nav-datos { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; }
        .nav-datos div { display: flex; flex-direction: column; align-items: center; padding: 10px 6px; border-radius: 12px; background: #fff; border: 1px solid var(--blue-200); }
        .nav-datos b { font-size: 17px; font-weight: 900; }
        .nav-datos small { font-size: 11px; color: var(--text-secondary); }
        .nav-titulo { font-size: 15px; font-weight: 900; }
        .pasos-nav { margin: 0; }
        .pasos-nav li { padding: 10px; }
        .pasos-nav li.actual { border: 2px solid var(--c); background: #fff; padding: 8px; }
        .nav-acciones { display: flex; gap: 10px; margin-top: 4px; }
        .nav-acciones .btn { flex: 1; }

        /* Planificar: mis lugares e historial */
        .chips-lugares { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 8px; }
        .chip-lugar { display: inline-flex; align-items: center; gap: 6px; padding: 7px 13px; border-radius: 999px; border: 1px solid var(--blue-200); background: #fff;
            color: var(--text-primary); font-size: 13px; font-weight: 700; cursor: pointer; }
        .chip-lugar:hover { border-color: var(--brand-primary); background: var(--blue-50); color: var(--brand-primary); }
        .chip-lugar .icono { color: var(--brand-primary); }
        .guardar-lugar, .form-lugar { padding: 14px; margin-top: 8px; display: flex; flex-direction: column; gap: 10px; border-radius: 14px; }
        .tipos-lugar { display: flex; flex-wrap: wrap; gap: 8px; }
        .tipos-lugar .chip-radio { display: inline-flex; align-items: center; gap: 5px; }
        .viaje-historial { cursor: pointer; border: 1px solid transparent; }
        .viaje-historial.sel { border-color: var(--brand-primary); background: var(--grad-suave); }

        /* Registro */
        .fuerza-texto.coincide { color: var(--success); font-weight: 700; }
        .fuerza-texto.no-coincide { color: var(--danger); }

        /* Perfil */
        .avatar-grande { width: 96px; height: 96px; font-size: 32px; border: 3px solid #fff; box-shadow: 0 6px 18px rgba(37, 99, 235, .25); }
        .perfil-acciones .campo { margin-bottom: 4px; }
        .pestanas-perfil { margin: 0 0 20px; grid-template-columns: repeat(3, 1fr); }
        .pestanas-perfil .seg { flex-wrap: wrap; }
        .filtro-guardadas { margin: 0 0 16px; flex-wrap: wrap; }
        .vacio-sesion { display: flex; flex-direction: column; align-items: center; gap: 10px; text-align: center; padding: 40px 20px; }
        .vacio-sesion h3 { font-size: 18px; font-weight: 900; }
        .vacio-sesion p { color: var(--text-secondary); max-width: 360px; }
        .trayecto .btn-icono { flex: none; }
        .trayecto-texto b { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
        .form-lugar .campo-wrap { margin-bottom: 0; }

        /* Reportar y compartir */
        .modal-reporte, .modal-compartir { max-width: 520px; }
        .modal-reporte .etiqueta-form { margin-top: 16px; margin-bottom: 8px; }
        .reporte-tipos { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; }
        .reporte-tipo { display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 12px 6px; border-radius: 12px; border: 1px solid var(--border-default);
            background: #fff; color: var(--text-secondary); font-size: 12px; font-weight: 700; cursor: pointer; text-align: center; }
        .reporte-tipo:hover { border-color: var(--brand-primary); }
        .reporte-tipo.sel { border: 2px solid var(--rose); color: var(--rose); background: var(--rose-tint); padding: 11px 5px; }
        .compartir-ayuda { font-size: 13px; color: var(--text-secondary); margin: 14px 0 10px; }
        .textarea.codigo { font-family: Consolas, "Courier New", monospace; font-size: 12px; resize: none; background: var(--blue-50); word-break: break-all; }

        /* Tarjeta de aviso, invitaciones vacías */
        .invitacion a.btn-ghost { text-decoration: none; }
        """;
}
