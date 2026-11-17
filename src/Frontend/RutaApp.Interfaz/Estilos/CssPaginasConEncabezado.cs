namespace RutaApp.Interfaz.Estilos;

// Comunidad, Perfil y Configuración.
public static class CssPaginasConEncabezado
{
    public const string Reglas = """
        .pagina { height: 100%; overflow-y: auto; padding: 40px 40px 28px; }
        .pagina-cabecera { display: flex; justify-content: space-between; align-items: center; gap: var(--espacio-6); }
        .pagina-cabecera h1 { font-size: var(--texto-3xl); font-weight: var(--peso-titulo); }
        .pagina-cabecera p { font-size: var(--texto-lg); color: var(--text-secondary); margin-top: 6px; }
        .busqueda-ancha { width: 300px; }

        /* Comunidad */
        .pagina-comunidad { padding-top: 36px; }
        .aviso-fijo { display: flex; align-items: center; gap: 14px; background: var(--warning-tint); border: 1px solid var(--warning); border-radius: var(--radio-lg); padding: 12px 18px; margin-top: 16px; }
        .aviso-fijo > div { flex: 1; display: flex; flex-direction: column; }
        .aviso-fijo small { font-size: var(--texto-xs); font-weight: var(--peso-fuerte); color: var(--warning); }
        .aviso-fijo b { font-size: var(--texto-md); font-weight: var(--peso-medio); margin-top: 2px; }
        .aviso-fijo-icono { width: 36px; height: 36px; border-radius: var(--radio-circulo); background: var(--bg-white); display: flex; align-items: center; justify-content: center; }
        .filtros { display: flex; justify-content: space-between; align-items: center; gap: var(--espacio-4); margin: 16px 0 10px; flex-wrap: wrap; }
        .pills, .filtros-derecha { display: flex; gap: var(--espacio-2); align-items: center; }
        .pill-radio { display: flex; align-items: center; padding: 8px 14px; border-radius: var(--radio-pill); border: 1px solid var(--border-soft); background: var(--bg-card); color: var(--text-secondary); font-size: var(--texto-md); font-weight: var(--peso-medio); cursor: pointer; }
        .pill-radio:hover { border-color: var(--brand-primary); }
        .pill-radio.activo { background: var(--brand-primary); border-color: var(--brand-primary); color: var(--text-on-brand); font-weight: var(--peso-fuerte); }
        .pill-radio em { font-style: normal; font-size: var(--texto-sm); opacity: .75; margin-left: 8px; }
        .pill-radio .pt { width: 8px; height: 8px; border-radius: var(--radio-circulo); margin-right: 8px; }
        .feed { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: var(--espacio-5); margin-top: 4px; }
        .post { padding: 20px; min-height: 256px; display: flex; flex-direction: column; }
        .post header { display: flex; align-items: center; gap: 10px; }
        .post-avatar { width: 34px; height: 34px; border-radius: var(--radio-circulo); background: var(--brand-primary); color: var(--text-on-brand); font-size: var(--texto-lg); font-weight: var(--peso-titulo); display: flex; align-items: center; justify-content: center; flex: none; }
        .post-avatar.oficial { background: var(--accent-reference); color: var(--text-primary); }
        .post-autor { flex: 1; min-width: 0; display: flex; flex-direction: column; }
        .post-autor span { display: flex; align-items: center; gap: var(--espacio-1); font-size: var(--texto-base); }
        .post-autor small { font-size: var(--texto-sm); color: var(--text-secondary); }
        .post-tipo { display: flex; align-items: center; gap: 5px; font-size: var(--texto-xs); font-weight: var(--peso-fuerte); padding: 4px 8px; border-radius: var(--radio-sm); background: var(--warning-tint); color: var(--warning); }
        .post-tipo.pregunta { background: var(--brand-tint); color: var(--brand-primary); }
        .post-tipo.propuesta { background: var(--success-tint); color: var(--success); }
        .post h3 { font-size: var(--texto-xl); font-weight: var(--peso-titulo); margin-top: 14px; }
        .post p { font-size: var(--texto-md); line-height: 20px; color: var(--text-secondary); margin-top: 8px; }
        .post-etiquetas { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 10px; flex: 1; align-content: flex-start; }
        .etiqueta { display: inline-flex; align-items: center; gap: 5px; font-size: var(--texto-sm); font-weight: var(--peso-fuerte); padding: 4px 8px; border-radius: var(--radio-sm); background: var(--bg-panel); border: 1px solid var(--border-soft); color: var(--text-secondary); }
        .etiqueta.ruta { background: var(--brand-tint); border-color: var(--brand-tint); color: var(--brand-primary); }
        .post footer { display: flex; align-items: center; gap: var(--espacio-1); border-top: 1px solid var(--border-soft); padding-top: 12px; margin-top: 12px; }
        .btn-pie { display: inline-flex; align-items: center; gap: 6px; padding: 6px 8px; border: 0; border-radius: var(--radio-sm); background: transparent; color: var(--text-secondary); font-size: var(--texto-base); font-weight: var(--peso-fuerte); cursor: pointer; }
        .btn-pie:hover { background: var(--bg-panel); }
        .btn-pie.derecha { margin-left: auto; }
        .btn-pie.azul { color: var(--brand-primary); }

        /* Perfil */
        .perfil-grid { display: grid; grid-template-columns: 380px 1fr; gap: var(--espacio-6); margin-top: 32px; align-items: start; }
        .perfil-datos { padding: 32px; border-radius: var(--radio-xl); text-align: center; }
        .avatar { width: 44px; height: 44px; margin: 0 auto; border-radius: var(--radio-circulo); background: var(--brand-tint) center/cover; border: 1px solid var(--border-default); color: var(--brand-primary); font-size: var(--texto-lg); font-weight: var(--peso-titulo); display: flex; align-items: center; justify-content: center; }
        .perfil-datos h2 { font-size: var(--texto-2xl); font-weight: var(--peso-titulo); margin: 20px 0 4px; }
        .miembro { font-size: var(--texto-base); color: var(--text-secondary); }
        .perfil-datos hr { border: 0; height: 1px; background: var(--border-soft); margin: 20px 0; }
        .dato { display: flex; justify-content: space-between; font-size: var(--texto-base); margin-top: 8px; }
        .dato span { color: var(--text-secondary); }
        .dato b { font-weight: var(--peso-medio); }
        .perfil-acciones { padding: 20px; border-radius: var(--radio-xl); margin-top: 24px; display: flex; flex-direction: column; gap: var(--espacio-3); }
        .perfil-acciones .btn-primario { font-size: var(--texto-base); padding: 12px 16px; }
        .perfil-acciones label.btn { cursor: pointer; }
        .perfil-trayectos { padding: 24px; border-radius: var(--radio-xl); }
        .trayectos-cabecera { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        .trayectos-cabecera h3 { font-size: var(--texto-xl); font-weight: var(--peso-titulo); }
        .trayecto { display: flex; align-items: center; gap: var(--espacio-4); background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: var(--radio-lg); padding: 16px; margin-bottom: 12px; }
        .trayecto-icono { width: 40px; height: 40px; border-radius: var(--radio-circulo); background: var(--brand-tint); display: flex; align-items: center; justify-content: center; flex: none; }
        .trayecto-texto { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
        .trayecto-texto b { font-size: var(--texto-lg); font-weight: var(--peso-titulo); }
        .trayecto-texto span { font-size: var(--texto-base); font-weight: var(--peso-fuerte); }
        .trayecto-texto small { font-size: var(--texto-sm); color: var(--text-secondary); }
        .trayecto .btn-icono { padding: 6px; }

        /* Configuración */
        .categorias { display: grid; grid-template-columns: 1fr 1fr; gap: var(--espacio-6); margin-top: 28px; }
        .categoria { padding: 24px; }
        .categoria h3 { font-size: var(--texto-md); font-weight: var(--peso-fuerte); color: var(--brand-primary); margin-bottom: 16px; }
        .ajuste-item { display: flex; align-items: center; justify-content: space-between; width: 100%; padding: 12px 16px; margin-bottom: 4px; border: 0; border-radius: var(--radio-sm); background: var(--bg-panel); color: var(--text-primary); font-size: var(--texto-base); font-weight: var(--peso-medio); cursor: pointer; text-align: left; }
        .ajuste-item:hover { background: var(--brand-tint); }
        .pie-config { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; margin-top: 24px; border-radius: var(--radio-lg); font-size: var(--texto-base); color: var(--text-secondary); }

        /* Filtros del historial y de las rutas guardadas en el perfil */
        .filtros-perfil { display: flex; flex-direction: column; gap: 10px; margin-bottom: 12px; }
        .filtros-perfil .chips { display: flex; flex-wrap: wrap; gap: var(--espacio-2); }
        .separador-chips { width: 1px; align-self: stretch; margin: 2px 4px; background: var(--border-default); }

        .orden-info { display: inline-flex; align-items: center; gap: 6px; font-size: var(--texto-base); font-weight: var(--peso-medio); color: var(--text-secondary); white-space: nowrap; }
        """;
}
