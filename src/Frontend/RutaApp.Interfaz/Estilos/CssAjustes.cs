namespace RutaApp.Interfaz.Estilos;

// Bloques de las pantallas de ajustes.
public static class CssAjustes
{
    public const string Reglas = """
        .marco-detalle { height: 100%; overflow-y: auto; padding: 30px 40px 14px; display: flex; flex-direction: column; }
        .migas { display: flex; align-items: center; gap: var(--espacio-3); font-size: var(--texto-base); color: var(--text-secondary); }
        .migas .btn-ghost { padding: 2px 0; }
        .marco-titulo { display: flex; justify-content: space-between; align-items: flex-start; margin-top: 10px; }
        .marco-titulo h1 { font-size: 30px; font-weight: var(--peso-titulo); }
        .marco-titulo p { font-size: var(--texto-md); color: var(--text-secondary); margin-top: 8px; }
        .sync { display: inline-flex; align-items: center; gap: var(--espacio-2); background: var(--success-tint); color: var(--success); font-size: var(--texto-sm); padding: 6px 14px 6px 12px; border-radius: var(--radio-lg); margin-top: 4px; white-space: nowrap; }
        .sync i { width: 7px; height: 7px; border-radius: var(--radio-circulo); background: var(--success); }
        .marco-cuerpo { flex: 1; margin-top: 22px; padding-bottom: 12px; }
        .marco-barra { margin-top: 12px; position: relative; }
        .marco-pie { display: flex; justify-content: space-between; font-size: var(--texto-xs); color: var(--text-secondary); margin: 18px 4px 0; }
        .marco-pie .btn-ghost { font-size: var(--texto-xs); padding: 0; }

        .tarjeta { background: var(--bg-card); border: 1px solid var(--border-default); border-radius: var(--radio-lg); box-shadow: var(--sombra); }
        .tarjeta.separada { margin-top: 16px; }
        .tarjeta-titulo { font-size: var(--texto-xl); font-weight: var(--peso-titulo); }
        .tarjeta-sub { font-size: var(--texto-base); color: var(--text-secondary); margin-top: 6px; }
        .tarjeta-cuerpo { margin-top: 20px; }
        .tarjeta-cuerpo > .aviso, .tarjeta-cuerpo > .arriba { margin-top: 18px; }

        .dos-columnas { display: grid; gap: var(--espacio-6); align-items: start; }
        .col-lateral { align-self: start; }
        .tres-columnas { display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--espacio-4); }
        .tres-columnas.pequenas { gap: var(--espacio-3); margin-top: 16px; }
        .dos-columnas-iguales { display: grid; grid-template-columns: 1fr 1fr; gap: var(--espacio-3); }
        .dos-campos { display: grid; grid-template-columns: 1fr 1fr; gap: var(--espacio-4); }
        .arriba { margin-top: 14px; }
        .arriba-grande { margin-top: 16px; }
        .separado { margin-top: 18px; }
        .separador { height: 1px; background: var(--border-default); margin: 6px 0 0; }

        .caja-icono { display: inline-flex; align-items: center; justify-content: center; border-radius: var(--radio-md); background: var(--brand-tint); color: var(--brand-primary); flex: none; }
        .caja-icono.llena { background: var(--brand-primary); color: var(--text-on-brand); }

        .interruptor { position: relative; width: 44px; height: 24px; border: 0; border-radius: var(--radio-md); background: #B9D0EC; cursor: pointer; flex: none; padding: 0; transition: background .15s; }
        .interruptor span { position: absolute; top: 3px; left: 3px; width: 18px; height: 18px; border-radius: var(--radio-circulo); background: var(--bg-white); box-shadow: 0 1px 3px rgba(0, 0, 0, .25); transition: left .15s; }
        .interruptor.activo { background: var(--brand-primary); }
        .interruptor.activo span { left: 23px; }

        .fila-ajuste { display: flex; align-items: center; gap: 14px; padding: 12px 0; }
        .fila-ajuste.con-linea { border-bottom: 1px solid var(--border-default); }
        .fila-ajuste-texto { flex: 1; min-width: 0; display: flex; flex-direction: column; }
        .fila-ajuste-texto b { font-size: var(--texto-md); font-weight: var(--peso-fuerte); }
        .fila-ajuste-texto small { font-size: var(--texto-sm); color: var(--text-secondary); margin-top: 3px; }
        .fila-maestra { display: flex; align-items: center; gap: 14px; background: var(--brand-tint); border-radius: var(--radio-md); padding: 14px 16px; margin-bottom: 12px; }

        .opcion-radio { display: flex; align-items: center; gap: 14px; padding: 14px 16px; margin-bottom: 12px; background: var(--bg-panel); border: 1px solid var(--border-default); border-radius: var(--radio-md); cursor: pointer; }
        .opcion-radio.sel { background: var(--brand-tint); border: 2px solid var(--brand-primary); padding: 13px 15px; }
        .radio { position: relative; width: 20px; height: 20px; border-radius: var(--radio-circulo); border: 2px solid var(--brand-primary); flex: none; display: inline-block; }
        .radio i { display: none; position: absolute; inset: 3px; border-radius: var(--radio-circulo); background: var(--brand-primary); }
        .opcion-radio.sel .radio i, .modo-tarjeta.sel .radio i { display: block; }

        .chip { display: inline-block; font-weight: var(--peso-fuerte); padding: 4px 10px; border-radius: var(--radio-md); white-space: nowrap; }
        .chip-primario { background: var(--brand-primary); color: var(--text-on-brand); }
        .chip-tinte { background: var(--brand-tint); color: var(--brand-primary); }
        .chip-panel { background: var(--bg-panel); color: var(--text-secondary); }
        .chip-sidebar { background: var(--bg-sidebar); color: var(--text-secondary); }

        .aviso { display: flex; gap: var(--espacio-3); align-items: flex-start; border-radius: var(--radio-md); padding: 14px 16px; background: var(--brand-tint); color: var(--brand-primary); }
        .aviso > div { display: flex; flex-direction: column; }
        .aviso b { font-size: var(--texto-base); color: var(--text-primary); }
        .aviso p { font-size: var(--texto-sm); color: var(--text-secondary); }
        .aviso b + p { margin-top: 3px; }
        .aviso-ok { background: var(--success-tint); color: var(--success); }
        .aviso-alerta { background: var(--warning-tint); color: var(--warning); }
        .aviso .icono { margin-top: 1px; }
        .tarjeta-cuerpo > .aviso, .tarjeta-cuerpo > .par-valor + .aviso { margin-top: 4px; }

        .par-valor { display: flex; justify-content: space-between; gap: var(--espacio-3); font-size: var(--texto-base); padding: 11px 0; }
        .par-valor span { color: var(--text-secondary); }
        .par-valor b { text-align: right; }
        .par-valor.con-linea { border-bottom: 1px solid var(--border-default); }
        .par-valor.recuadro { background: var(--bg-panel); border-radius: var(--radio-sm); padding: 12px 14px; margin-bottom: 8px; }
        .par-valor.recuadro b { color: var(--brand-primary); }
        .total-distancia { display: flex; justify-content: space-between; align-items: center; margin-top: 12px; }
        .total-distancia h4 { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); }

        .btn-ajuste { padding: 11px 18px; font-size: var(--texto-md); }
        .btn-ajuste.btn-contorno { padding: 11px 18px; }
        .botones-fila { display: flex; gap: 10px; margin-top: 14px; flex-wrap: wrap; }

        .desplegable { display: flex; align-items: center; gap: 10px; background: var(--bg-card); border: 1px solid var(--border-default); border-radius: var(--radio-md); padding: 12px 14px; font-size: var(--texto-base); }
        .desplegable span { flex: 1; }
        .area-texto { position: relative; background: var(--bg-card); border: 1px solid var(--border-default); border-radius: var(--radio-md); padding: 12px 14px 10px; }
        .area-texto textarea { border: 0; padding: 0 0 22px; min-height: 96px; background: transparent; border-radius: 0; font-size: var(--texto-base); resize: none; }
        .area-texto small { position: absolute; right: 14px; bottom: 10px; font-size: var(--texto-xs); color: var(--text-secondary); }
        .etiquetado label { display: block; font-size: var(--texto-sm); font-weight: var(--peso-fuerte); margin-bottom: 6px; }
        .casilla-texto { display: inline-flex; align-items: center; gap: 10px; cursor: pointer; font-size: var(--texto-base); }
        .casilla-texto input { position: absolute; opacity: 0; width: 0; height: 0; }
        .casilla { width: 20px; height: 20px; border-radius: var(--radio-sm); border: 1.5px solid var(--brand-primary); background: var(--bg-card); display: flex; align-items: center; justify-content: center; color: transparent; }
        .casilla-texto input:checked + .casilla { background: var(--brand-primary); color: var(--text-on-brand); border-color: var(--brand-primary); }
        .adjuntar { display: flex; align-items: center; gap: 10px; padding: 14px 16px; border-radius: var(--radio-md); background: var(--bg-panel); border: 1px solid var(--border-default); color: var(--brand-primary); }
        .adjuntar b { flex: 1; font-size: var(--texto-base); }
        .adjuntar small { font-size: var(--texto-xs); color: var(--text-secondary); }
        .chequeo { display: flex; align-items: center; gap: 10px; font-size: var(--texto-base); margin-bottom: 10px; }
        .estrellas { display: flex; align-items: center; gap: var(--espacio-1); }
        .estrellas .btn-icono { padding: 2px; margin-right: 6px; }
        .paso-lista { display: flex; gap: var(--espacio-3); margin-bottom: 12px; }
        .paso-lista > span { width: 30px; height: 30px; border-radius: var(--radio-circulo); background: var(--brand-tint); color: var(--brand-primary); font-size: var(--texto-sm); font-weight: var(--peso-fuerte); display: flex; align-items: center; justify-content: center; flex: none; }
        .busqueda-ayuda { display: flex; align-items: center; gap: var(--espacio-3); font-size: var(--texto-md); color: var(--text-secondary); }
        .busqueda-ayuda span { flex: 1; }
        .categoria-ayuda { display: flex; align-items: center; gap: var(--espacio-3); background: var(--bg-panel); border-radius: var(--radio-md); padding: 14px 16px; }
        .faq { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid var(--border-default); font-size: var(--texto-base); }
        .ayuda-extra { display: flex; flex-direction: column; align-items: flex-start; background: var(--brand-tint); border-radius: var(--radio-lg); padding: 18px; margin-top: 16px; }
        .ayuda-extra b { font-size: var(--texto-base); }
        .ayuda-extra small { font-size: var(--texto-sm); color: var(--text-secondary); margin: 2px 0 12px; }

        .barra-inferior { display: flex; align-items: center; gap: 10px; }
        .barra-inferior span { flex: 1; font-size: var(--texto-base); color: var(--text-secondary); }
        .toast-ajuste { position: fixed; right: 40px; bottom: 90px; z-index: 1500; background: var(--text-primary); color: var(--text-on-brand); padding: 10px 18px; border-radius: var(--radio-md); font-weight: var(--peso-medio); box-shadow: 0 6px 20px rgba(27, 54, 93, .3); }

        .deslizador-cabeza { display: flex; justify-content: space-between; font-size: var(--texto-base); margin: 14px 0 12px; }
        .deslizador-extremos { display: flex; justify-content: space-between; font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 8px; }
        .deslizador { width: 100%; height: 6px; border-radius: 3px; appearance: none; outline: none; background: linear-gradient(to right, var(--brand-primary) var(--p), var(--bg-sidebar) var(--p)); }
        .deslizador::-webkit-slider-thumb { appearance: none; width: 18px; height: 18px; border-radius: var(--radio-circulo); background: var(--bg-white); border: 3px solid var(--brand-primary); cursor: pointer; }
        .deslizador::-moz-range-thumb { width: 12px; height: 12px; border-radius: var(--radio-circulo); background: var(--bg-white); border: 3px solid var(--brand-primary); cursor: pointer; }

        .modo-tarjeta { min-height: 250px; padding: 20px; border-radius: var(--radio-lg); background: var(--bg-panel); border: 1px solid var(--border-default); cursor: pointer; }
        .modo-tarjeta.sel { background: var(--brand-tint); border: 2px solid var(--brand-primary); padding: 19px; }
        .modo-cabecera { display: flex; justify-content: space-between; align-items: flex-start; }
        .modo-tarjeta h4 { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); margin-top: 14px; }
        .modo-tarjeta p { font-size: var(--texto-base); color: var(--text-secondary); margin: 4px 0 12px; }
        .modo-tarjeta:not(.sel) .caja-icono { background: var(--bg-white); }

        .opcion-ejemplo { position: relative; padding: 16px; border-radius: var(--radio-md); background: var(--bg-panel); margin-bottom: 12px; }
        .opcion-ejemplo.sel { background: var(--brand-tint); border: 1.5px solid var(--brand-primary); padding: 14.5px; }
        .opcion-ejemplo small { font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--text-secondary); }
        .opcion-ejemplo small.azul { color: var(--brand-primary); }
        .opcion-ejemplo h4 { font-size: var(--texto-lg); font-weight: var(--peso-titulo); margin-top: 6px; }
        .opcion-ejemplo p { font-size: var(--texto-sm); color: var(--text-secondary); margin-top: 8px; }
        .opcion-ejemplo-check { position: absolute; top: 14px; right: 14px; color: var(--brand-primary); }

        .proxima-ruta { background: var(--bg-panel); border-radius: var(--radio-md); padding: 14px 16px; }
        .proxima-ruta small { font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--brand-primary); }
        .proxima-ruta h4 { font-size: var(--texto-xl); font-weight: var(--peso-titulo); margin-top: 4px; }
        .proxima-ruta span { display: block; font-size: var(--texto-base); color: var(--text-secondary); margin-top: 4px; }
        .perfil-activo { display: flex; align-items: center; gap: 14px; background: var(--brand-tint); border-radius: var(--radio-md); padding: 14px; margin-bottom: 14px; }
        .enlace-flecha { display: inline-flex; align-items: center; gap: 6px; font-size: var(--texto-sm); font-weight: var(--peso-fuerte); color: var(--brand-primary); }

        .etiqueta-doc { font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--brand-primary); }
        .doc-titulo { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); margin-top: 4px; }
        .doc-h { font-size: var(--texto-lg); font-weight: var(--peso-titulo); margin-top: 14px; }
        .doc-p { font-size: var(--texto-base); line-height: 22px; color: var(--text-secondary); margin-top: 6px; }
        .indice-doc { margin-top: 14px; }
        .indice-item { display: flex; justify-content: space-between; align-items: center; padding: 9px 12px; border-radius: var(--radio-sm); margin-bottom: 4px; font-size: var(--texto-base); }
        .indice-item.activo { background: var(--brand-tint); color: var(--brand-primary); }
        .mosaico { display: flex; flex-direction: column; gap: 0; background: var(--bg-panel); border-radius: var(--radio-md); padding: 14px; }
        .mosaico .icono { color: var(--brand-primary); }
        .mosaico b { font-size: var(--texto-base); margin-top: 8px; }
        .mosaico small { font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 4px; }

        .mision { background: var(--brand-primary); border-radius: var(--radio-xl); padding: 28px; color: var(--text-on-brand); }
        .mision-logo { width: 54px; height: 54px; border-radius: var(--radio-lg); background: var(--bg-white); color: var(--brand-primary); font-size: var(--texto-3xl); font-weight: var(--peso-titulo); display: flex; align-items: center; justify-content: center; }
        .mision small { display: block; font-size: var(--texto-xs); font-weight: var(--peso-fuerte); opacity: .85; margin-top: 14px; }
        .mision h2 { font-size: var(--texto-3xl); font-weight: var(--peso-titulo); margin-top: 6px; }
        .mision p { font-size: var(--texto-md); line-height: 24px; opacity: .92; margin-top: 10px; }
        .version-ok { display: inline-flex; align-items: center; gap: 10px; background: var(--success-tint); color: var(--success); font-size: var(--texto-base); font-weight: var(--peso-fuerte); border-radius: var(--radio-md); padding: 12px 14px; margin-top: 18px; }
        .contacto-linea { display: flex; align-items: center; gap: 10px; font-size: var(--texto-base); margin-top: 8px; }
        .contacto-linea .icono { color: var(--brand-primary); }

        /* Comentarios para mejorar la app */
        .estrellas { display: flex; gap: var(--espacio-1); }
        .estrella { border: 0; background: transparent; padding: 2px; cursor: pointer; color: var(--border-default); transition: transform .15s, color .15s; }
        .estrella:hover { transform: scale(1.15); }
        .estrella.llena { color: var(--star); }
        .estrella.llena .icono { fill: currentColor; }
        .contador-texto { display: block; text-align: right; margin-top: 4px; color: var(--text-secondary); }
        .nota-ubicacion { color: var(--danger); display: flex; align-items: center; gap: 6px; }
        """;
}
