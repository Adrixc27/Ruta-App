namespace RutaApp.Interfaz.Estilos;

// Landing, sobre nosotros, inicio de sesión y registro.
public static class CssPaginasPublicas
{
    public const string Reglas = """
        .publico { min-height: 100vh; background: var(--bg-panel); }

        .landing { min-height: 100vh; display: flex; flex-direction: column; background: var(--bg-panel); }
        .landing-cabecera { display: flex; justify-content: space-between; align-items: center; padding: 22px 88px 0; }
        .landing-nav { display: flex; align-items: center; }
        .landing-contenido { flex: 1; display: grid; grid-template-columns: 1fr 640px; gap: var(--espacio-6); align-items: center; padding: 24px 48px 48px 112px; }
        .landing-texto h1 { font-size: 54px; font-weight: var(--peso-titulo); line-height: 70px; }
        .landing-texto p { font-size: var(--texto-xl); margin-top: 22px; }
        .landing-botones { display: flex; gap: var(--espacio-3); margin-top: 44px; }
        .landing-tarjeta { width: 640px; max-width: 100%; height: 620px; border-radius: 32px; overflow: hidden; border: 1px solid var(--border-default); background: var(--bg-card); }

        .sobre { min-height: 100vh; background: var(--bg-panel); }
        .sobre-cabecera { display: flex; justify-content: space-between; align-items: center; height: 80px; padding: 0 112px 0 88px; background: var(--bg-page); }
        .sobre-cuerpo { padding: 36px 112px 24px; }
        .sobre-cuerpo h1 { text-align: center; font-size: 40px; font-weight: var(--peso-titulo); }
        .sobre-sub { text-align: center; font-size: var(--texto-lg); margin: 10px 0 28px; }
        .sobre-pie { display: flex; justify-content: space-between; font-size: var(--texto-xs); margin: 40px 4px 0; }

        .no-encontrado { min-height: 100vh; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: var(--espacio-3); }

        /* Inicio de sesión / registro */
        .auth { display: grid; grid-template-columns: 640fr 800fr; min-height: 100vh; }
        .auth-carrusel { position: relative; min-height: 100vh; }
        .auth-panel { background: var(--bg-panel); display: flex; align-items: center; justify-content: center; padding: 32px 16px; }
        .auth-tarjeta { width: 440px; max-width: 100%; background: var(--bg-card); border: 1px solid var(--border-soft); border-radius: var(--radio-xl); padding: 41px; }
        .tarjeta-flotante { box-shadow: var(--sombra-elevada); }
        .auth-titulo { font-size: var(--texto-3xl); font-weight: var(--peso-titulo); }
        .auth-sub { color: var(--text-secondary); margin: 6px 0 24px; }
        .etiqueta-form { font-size: var(--texto-sm); font-weight: var(--peso-fuerte); color: var(--text-secondary); margin-bottom: 6px; }
        .etiqueta-form.dos { display: flex; justify-content: space-between; margin-top: 18px; }
        .etiqueta-form small { font-weight: var(--peso-normal); margin-left: 6px; font-size: var(--texto-xs); }
        .arriba-form { margin-top: 16px; }
        .auth-fila { display: flex; justify-content: space-between; align-items: center; margin: 18px 0; }
        .recordarme, .terminos { font-size: var(--texto-sm); color: var(--text-secondary); display: flex; align-items: center; gap: 6px; }
        .terminos { align-items: flex-start; margin: 18px 0; }
        .terminos input { margin-top: 2px; }
        .divisor { display: flex; align-items: center; gap: var(--espacio-3); margin: 22px 0; font-size: var(--texto-xs); color: var(--text-secondary); }
        .divisor::before, .divisor::after { content: ""; flex: 1; height: 1px; background: var(--border-soft); }
        .google { color: var(--brand-primary); font-size: var(--texto-lg); font-weight: var(--peso-titulo); }
        .auth-cambio { display: flex; align-items: center; justify-content: center; gap: 2px; margin-top: 14px; font-size: var(--texto-sm); color: var(--text-secondary); }
        .fuerza { display: grid; grid-template-columns: repeat(4, 1fr); gap: 6px; margin-top: 10px; }
        .fuerza span { height: 4px; border-radius: 2px; }
        .fuerza-texto { font-size: var(--texto-xs); color: var(--text-secondary); margin-top: 6px; }

        /* Entrada de la landing: bienvenida y bloques que aparecen uno tras otro (también al volver desde la app) */
        .bienvenida { display: inline-flex; align-items: center; gap: var(--espacio-2); padding: 7px 16px; margin-bottom: 18px; border-radius: var(--radio-pill); background: var(--brand-tint); color: var(--brand-primary); font-size: var(--texto-lg); font-weight: var(--peso-fuerte); }
        .landing-espera .landing-cabecera, .landing-espera .landing-texto, .landing-espera .landing-tarjeta { opacity: 0; }
        .landing-entra .landing-cabecera { animation: landing-baja .5s ease-out both; }
        .landing-entra .landing-texto { animation: landing-sube .6s .12s ease-out both; }
        .landing-entra .bienvenida { animation: bienvenida-entra .7s .35s cubic-bezier(.2, 1.4, .4, 1) both; }
        .landing-entra .landing-tarjeta { animation: landing-tarjeta-entra .7s .25s ease-out both; }
        @keyframes landing-baja { from { opacity: 0; transform: translateY(-14px); } to { opacity: 1; transform: none; } }
        @keyframes landing-sube { from { opacity: 0; transform: translateY(22px); } to { opacity: 1; transform: none; } }
        @keyframes landing-tarjeta-entra { from { opacity: 0; transform: translateX(34px) scale(.97); } to { opacity: 1; transform: none; } }
        @keyframes bienvenida-entra { from { opacity: 0; transform: translateY(-8px) scale(.85); } to { opacity: 1; transform: none; } }
        """;
}
