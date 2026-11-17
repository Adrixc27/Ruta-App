namespace RutaApp.Interfaz.Estilos;

// Mapa Leaflet, marcadores y su tinte.
public static class CssMapa
{
    public const string Reglas = """
        .mapa-contenedor { position: relative; width: 100%; height: 100%; min-height: 0; background: var(--bg-page); }
        .mapa-leaflet { position: absolute; inset: 0; background: var(--bg-page); font-family: inherit; }
        /* Tinte azulado del mapa: ajusta estos valores a tu gusto */
        .mapa-leaflet .leaflet-tile-pane { filter: grayscale(1) contrast(.6) brightness(1.18) sepia(.15) hue-rotate(185deg) saturate(1.6); opacity: .9; }
        .leaflet-control-zoom a { color: var(--text-primary) !important; border-color: var(--border-default) !important; }
        .leaflet-control-attribution { font-size: var(--texto-xs); }

        .glass { position: absolute; z-index: 1000; background: rgba(230, 240, 250, .94); backdrop-filter: blur(10px); border: 1px solid var(--border-default); color: var(--text-primary); }
        .mapa-banner { top: 24px; left: 24px; border-radius: var(--radio-md); padding: 12px 20px; display: flex; align-items: center; gap: var(--espacio-3); font-size: var(--texto-md); font-weight: var(--peso-medio); max-width: calc(100% - 120px); }
        .banner-circ { width: 40px; height: 40px; border-radius: var(--radio-circulo); border: 3px solid var(--brand-primary); background: var(--bg-white); flex: none; }
        .banner-viaje { display: flex; flex-direction: column; }
        .banner-viaje b { font-size: var(--texto-md); font-weight: var(--peso-titulo); }
        .banner-viaje small { font-size: var(--texto-sm); font-weight: var(--peso-normal); color: var(--text-secondary); }
        .banner-punto { width: 10px; height: 10px; border-radius: var(--radio-circulo); background: var(--brand-primary); flex: none; }
        .banner-quitar { color: var(--text-secondary); font-size: var(--texto-sm); font-weight: var(--peso-fuerte); cursor: pointer; margin-left: 8px; }
        .banner-quitar:hover { color: var(--brand-primary); }

        .mapa-leyenda { right: 24px; bottom: 28px; width: 180px; border-radius: var(--radio-lg); padding: 16px; }
        .leyenda-titulo { font-size: var(--texto-sm); font-weight: var(--peso-fuerte); color: var(--text-secondary); margin-bottom: 12px; }
        .leyenda-item { display: flex; align-items: center; gap: var(--espacio-2); font-size: var(--texto-base); margin-bottom: 8px; }
        .leyenda-item:last-child { margin-bottom: 0; }
        .leyenda-item i { width: 10px; height: 10px; border-radius: var(--radio-circulo); display: block; }

        .mapa-tarjeta { left: 24px; bottom: 28px; width: 470px; max-width: calc(100% - 250px); border-radius: var(--radio-lg); padding: 19px; background: rgba(244, 248, 253, .97); }
        .tarjeta-cabecera { display: flex; align-items: center; gap: var(--espacio-3); }
        .tarjeta-numero { width: 38px; height: 38px; border-radius: var(--radio-md); background: var(--brand-primary); color: var(--text-on-brand); font-weight: var(--peso-titulo); font-size: var(--texto-xl); display: flex; align-items: center; justify-content: center; flex: none; }
        .tarjeta-titulo { flex: 1; display: flex; flex-direction: column; min-width: 0; }
        .tarjeta-titulo b { font-size: var(--texto-lg); font-weight: var(--peso-titulo); }
        .tarjeta-titulo small { font-size: var(--texto-sm); color: var(--text-secondary); }
        .tarjeta-guardar { font-size: var(--texto-sm); font-weight: var(--peso-fuerte); color: var(--brand-primary); background: var(--brand-tint); border-radius: var(--radio-sm); padding: 7px 10px; cursor: pointer; user-select: none; }
        .tarjeta-guardar.off { background: var(--bg-white); color: var(--text-secondary); border: 1px solid var(--border-default); }
        .tarjeta-variantes { display: flex; align-items: center; gap: var(--espacio-2); margin-top: 14px; flex-wrap: wrap; }
        .tarjeta-variantes small { font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--text-secondary); text-transform: uppercase; letter-spacing: .04em; margin-right: 2px; }
        .tarjeta-plano { font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 10px; }
        .tarjeta-datos { display: flex; gap: 28px; margin: 16px 0; }
        .tarjeta-datos div { display: flex; flex-direction: column; }
        .tarjeta-datos b { font-size: var(--texto-lg); font-weight: var(--peso-titulo); }
        .tarjeta-datos small { font-size: var(--texto-xs); color: var(--text-secondary); }
        .tarjeta-boton { display: block; text-align: center; background: var(--brand-primary); color: var(--text-on-brand); font-weight: var(--peso-fuerte); font-size: var(--texto-md); padding: 12px; border-radius: var(--radio-md); cursor: pointer; user-select: none; }
        .tarjeta-boton:hover { opacity: .9; }

        /* Marcadores */
        .m-o { width: 22px; height: 22px; border-radius: var(--radio-circulo); background: var(--accent-origin); border: 3px solid #fff; box-shadow: var(--sombra-marcador); }
        .m-d { width: 30px; height: 30px; border-radius: var(--radio-circulo); background: var(--bg-white); border: 4px solid var(--brand-primary); display: flex; align-items: center; justify-content: center; }
        .m-d i { width: 12px; height: 12px; border-radius: var(--radio-circulo); background: var(--brand-primary); display: block; }
        .m-n { width: 22px; height: 22px; border-radius: var(--radio-circulo); background: var(--text-primary); color: var(--text-on-brand); font-weight: var(--peso-titulo); font-size: var(--texto-xs); border: 2px solid #fff; box-shadow: var(--sombra-marcador); display: flex; align-items: center; justify-content: center; }
        .m-s { width: 14px; height: 14px; border-radius: var(--radio-circulo); background: var(--bg-white); border: 3px solid var(--brand-primary); }
        .m-p { width: 12px; height: 12px; border-radius: var(--radio-circulo); background: var(--accent-stop); border: 2px solid #fff; box-shadow: var(--sombra-marcador); }
        .m-r { width: 12px; height: 12px; border-radius: var(--radio-circulo); background: var(--accent-reference); border: 2px solid #fff; box-shadow: var(--sombra-marcador); }
        .leaflet-tooltip.pill { background: var(--bg-white); border: 0; border-radius: var(--radio-sm); color: var(--brand-primary); font-weight: var(--peso-fuerte); font-size: var(--texto-xs); padding: 4px 10px; box-shadow: 0 2px 8px rgba(27, 54, 93, .18); }
        .leaflet-tooltip.pill:before { display: none; }
        .leaflet-popup-content-wrapper { border-radius: var(--radio-md); color: var(--text-primary); }

        /* Problemas reportados por la comunidad */
        .mk-reporte { width: 32px; height: 32px; border-radius: var(--radio-circulo); background: var(--danger); color: var(--text-on-brand); border: 3px solid #fff; display: flex; align-items: center; justify-content: center; box-shadow: 0 3px 10px rgba(220, 38, 38, .55); animation: reporte-latido 2s ease-out infinite; }
        @keyframes reporte-latido { 0% { box-shadow: 0 0 0 0 rgba(220, 38, 38, .55); } 70% { box-shadow: 0 0 0 14px rgba(220, 38, 38, 0); } 100% { box-shadow: 0 0 0 0 rgba(220, 38, 38, 0); } }
        .nota-ubicacion { color: var(--danger); }
        """;
}
