namespace RutaApp.Interfaz.Estilos;

// Comunidad, Perfil y Configuración.
public static class CssPaginasConEncabezado
{
    public const string Reglas = """
        .pagina { height: 100%; overflow-y: auto; padding: 40px 40px 28px; }
        .pagina-cabecera { display: flex; justify-content: space-between; align-items: center; gap: 24px; }
        .pagina-cabecera h1 { font-size: 28px; font-weight: 900; }
        .pagina-cabecera p { font-size: 15px; color: var(--text-secondary); margin-top: 6px; }
        .busqueda-ancha { width: 300px; }

        /* Comunidad */
        .pagina-comunidad { padding-top: 36px; }
        .aviso-fijo { display: flex; align-items: center; gap: 14px; background: var(--warning-tint); border: 1px solid var(--warning); border-radius: 14px; padding: 12px 18px; margin-top: 16px; }
        .aviso-fijo > div { flex: 1; display: flex; flex-direction: column; }
        .aviso-fijo small { font-size: 11px; font-weight: 700; color: var(--warning); }
        .aviso-fijo b { font-size: 14px; font-weight: 600; margin-top: 2px; }
        .aviso-fijo-icono { width: 36px; height: 36px; border-radius: 50%; background: #fff; display: flex; align-items: center; justify-content: center; }
        .filtros { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin: 16px 0 10px; flex-wrap: wrap; }
        .pills, .filtros-derecha { display: flex; gap: 8px; align-items: center; }
        .pill-radio { display: flex; align-items: center; padding: 8px 14px; border-radius: 999px; border: 1px solid var(--border-soft); background: var(--bg-card); color: var(--text-secondary); font-size: 14px; font-weight: 600; cursor: pointer; }
        .pill-radio:hover { border-color: var(--brand-primary); }
        .pill-radio.activo { background: var(--brand-primary); border-color: var(--brand-primary); color: #fff; font-weight: 700; }
        .pill-radio em { font-style: normal; font-size: 12px; opacity: .75; margin-left: 8px; }
        .pill-radio .pt { width: 8px; height: 8px; border-radius: 50%; margin-right: 8px; }
        .feed { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; margin-top: 4px; }
        .post { padding: 20px; min-height: 256px; display: flex; flex-direction: column; }
        .post header { display: flex; align-items: center; gap: 10px; }
        .post-avatar { width: 34px; height: 34px; border-radius: 50%; background: var(--brand-primary); color: #fff; font-size: 15px; font-weight: 900; display: flex; align-items: center; justify-content: center; flex: none; }
        .post-avatar.oficial { background: var(--accent-reference); color: var(--text-primary); }
        .post-autor { flex: 1; min-width: 0; display: flex; flex-direction: column; }
        .post-autor span { display: flex; align-items: center; gap: 4px; font-size: 13px; }
        .post-autor small { font-size: 12px; color: var(--text-secondary); }
        .post-tipo { display: flex; align-items: center; gap: 5px; font-size: 11px; font-weight: 700; padding: 4px 8px; border-radius: 8px; background: var(--warning-tint); color: var(--warning); }
        .post-tipo.pregunta { background: var(--brand-tint); color: var(--brand-primary); }
        .post-tipo.propuesta { background: var(--success-tint); color: var(--success); }
        .post h3 { font-size: 18px; font-weight: 900; margin-top: 14px; }
        .post p { font-size: 14px; line-height: 20px; color: var(--text-secondary); margin-top: 8px; }
        .post-etiquetas { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 10px; flex: 1; align-content: flex-start; }
        .etiqueta { display: inline-flex; align-items: center; gap: 5px; font-size: 12px; font-weight: 700; padding: 4px 8px; border-radius: 8px; background: var(--bg-panel); border: 1px solid var(--border-soft); color: var(--text-secondary); }
        .etiqueta.ruta { background: var(--brand-tint); border-color: var(--brand-tint); color: var(--brand-primary); }
        .post footer { display: flex; align-items: center; gap: 4px; border-top: 1px solid var(--border-soft); padding-top: 12px; margin-top: 12px; }
        .btn-pie { display: inline-flex; align-items: center; gap: 6px; padding: 6px 8px; border: 0; border-radius: 8px; background: transparent; color: var(--text-secondary); font-size: 13px; font-weight: 700; cursor: pointer; }
        .btn-pie:hover { background: var(--bg-panel); }
        .btn-pie.derecha { margin-left: auto; }
        .btn-pie.azul { color: var(--brand-primary); }

        /* Perfil */
        .perfil-grid { display: grid; grid-template-columns: 380px 1fr; gap: 24px; margin-top: 32px; align-items: start; }
        .perfil-datos { padding: 32px; border-radius: 20px; text-align: center; }
        .avatar { width: 44px; height: 44px; margin: 0 auto; border-radius: 50%; background: var(--brand-tint) center/cover; border: 1px solid var(--border-default); color: var(--brand-primary); font-size: 16px; font-weight: 900; display: flex; align-items: center; justify-content: center; }
        .perfil-datos h2 { font-size: 20px; font-weight: 900; margin: 20px 0 4px; }
        .miembro { font-size: 13px; color: var(--text-secondary); }
        .perfil-datos hr { border: 0; height: 1px; background: var(--border-soft); margin: 20px 0; }
        .dato { display: flex; justify-content: space-between; font-size: 13px; margin-top: 8px; }
        .dato span { color: var(--text-secondary); }
        .dato b { font-weight: 600; }
        .perfil-acciones { padding: 20px; border-radius: 20px; margin-top: 24px; display: flex; flex-direction: column; gap: 12px; }
        .perfil-acciones .btn-primario { font-size: 13px; padding: 12px 16px; }
        .perfil-acciones label.btn { cursor: pointer; }
        .perfil-trayectos { padding: 24px; border-radius: 20px; }
        .trayectos-cabecera { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        .trayectos-cabecera h3 { font-size: 17px; font-weight: 900; }
        .trayecto { display: flex; align-items: center; gap: 16px; background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: 14px; padding: 16px; margin-bottom: 12px; }
        .trayecto-icono { width: 40px; height: 40px; border-radius: 50%; background: var(--brand-tint); display: flex; align-items: center; justify-content: center; flex: none; }
        .trayecto-texto { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
        .trayecto-texto b { font-size: 15px; font-weight: 900; }
        .trayecto-texto span { font-size: 13px; font-weight: 700; }
        .trayecto-texto small { font-size: 12px; color: var(--text-secondary); }
        .trayecto .btn-icono { padding: 6px; }

        /* Configuración */
        .categorias { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-top: 28px; }
        .categoria { padding: 24px; }
        .categoria h3 { font-size: 14px; font-weight: 700; color: var(--brand-primary); margin-bottom: 16px; }
        .ajuste-item { display: flex; align-items: center; justify-content: space-between; width: 100%; padding: 12px 16px; margin-bottom: 4px; border: 0; border-radius: 8px; background: var(--bg-panel); color: var(--text-primary); font-size: 13px; font-weight: 600; cursor: pointer; text-align: left; }
        .ajuste-item:hover { background: var(--brand-tint); }
        .pie-config { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; margin-top: 24px; border-radius: 14px; font-size: 13px; color: var(--text-secondary); }

        /* Filtros del historial y de las rutas guardadas en el perfil */
        .filtros-perfil { display: flex; flex-direction: column; gap: 10px; margin-bottom: 12px; }
        .filtros-perfil .chips { display: flex; flex-wrap: wrap; gap: 8px; }
        .separador-chips { width: 1px; align-self: stretch; margin: 2px 4px; background: var(--border-default); }

        .orden-info { display: inline-flex; align-items: center; gap: 6px; font-size: 13px; font-weight: 600; color: var(--text-secondary); white-space: nowrap; }
        """;
}
