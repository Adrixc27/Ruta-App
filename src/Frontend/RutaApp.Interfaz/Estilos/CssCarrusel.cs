namespace RutaApp.Interfaz.Estilos;

// Carrusel y animación de entrada.
public static class CssCarrusel
{
    public const string Reglas = """
        .carrusel { position: relative; width: 100%; height: 100%; min-height: inherit; overflow: hidden; background: var(--text-primary); }
        .carrusel-slide { position: absolute; inset: 0; opacity: 0; transition: opacity .6s; pointer-events: none; }
        .carrusel-slide.visible { opacity: 1; pointer-events: auto; z-index: 1; }
        .carrusel-puntos { position: absolute; bottom: 18px; left: 0; right: 0; display: flex; justify-content: center; z-index: 5; }
        .carrusel-puntos .punto { width: 8px; height: 8px; border-radius: 50%; margin: 0 4px; cursor: pointer; background: rgba(255, 255, 255, .47); box-shadow: 0 0 4px rgba(0, 0, 0, .5); }
        .carrusel-puntos .punto.activo { background: #fff; }

        .slide-foto { position: absolute; inset: 0; background-size: cover; background-position: center; }
        .slide-foto-velo { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(10, 25, 60, .59), rgba(10, 25, 60, 0) 28%, rgba(10, 25, 60, 0) 72%, rgba(10, 25, 60, .59)); }
        .slide-foto-titulo, .slide-foto-ciudad { position: absolute; left: 0; right: 0; text-align: center; color: #fff; font-size: 26px; font-weight: 700; text-shadow: 0 1px 8px rgba(0, 0, 0, .6); }
        .slide-foto-titulo { top: 36px; }
        .slide-foto-ciudad { bottom: 52px; }

        .slide-marca { position: absolute; inset: 0; background: var(--brand-primary); overflow: hidden; }
        .burbuja { position: absolute; border-radius: 50%; background: #fff; opacity: .07; }
        .burbuja-1 { width: 460px; height: 460px; left: 380px; top: -150px; }
        .burbuja-2 { width: 320px; height: 320px; left: -110px; bottom: -80px; }
        .slide-marca-contenido { position: absolute; inset: 0; padding: 56px; display: flex; flex-direction: column; justify-content: space-between; }
        .slide-ilustracion { flex: 1; display: flex; align-items: center; margin: 24px 0; min-height: 0; }
        .slide-ilustracion svg { max-width: 528px; width: 100%; max-height: 100%; }
        .slide-titular { font-size: 38px; font-weight: 900; line-height: 44px; color: #fff; }
        .slide-subtitular { font-size: 16px; line-height: 24px; color: rgba(255, 255, 255, .85); margin-top: 12px; }
        .landing-tarjeta .slide-marca-contenido { padding: 40px; }
        .landing-tarjeta .slide-titular { font-size: 32px; line-height: 38px; }

        /* Animación de entrada */
        .intro { position: fixed; inset: 0; z-index: 3000; background: var(--bg-panel); display: flex; align-items: center; justify-content: center; overflow: hidden; cursor: pointer; transition: opacity .5s; }
        .intro.saliendo { opacity: 0; }
        .intro-marca { text-align: center; color: var(--brand-primary); transition: opacity .7s; }
        .intro-marca.desvanece { opacity: 0; }
        .intro-nombre { font-size: 150px; font-weight: 900; line-height: 1.1; }
        .intro-lema { font-size: 24px; margin-top: -6px; }
        .intro-lema b { font-weight: 900; }
        .intro-letras.revelando { animation: intro-revela 3.6s cubic-bezier(.37, 0, .63, 1) forwards; }
        .intro-letras.revelado { clip-path: inset(-20px -20px -20px 0); }
        @keyframes intro-revela {
            from { clip-path: inset(-20px 100% -20px 0); }
            to { clip-path: inset(-20px -20px -20px 0); }
        }
        .intro-escena { position: absolute; left: 0; right: 0; top: calc(50% - 70px); height: 130px; pointer-events: none; }
        .intro-carretera { position: absolute; left: 0; right: 0; top: 118px; height: 6px; border-radius: 3px; background: var(--border-default); opacity: .9; }
        .intro-camion { position: absolute; top: 0; left: 0; width: 130px; height: 130px; color: var(--text-primary); animation: intro-camion 3.6s cubic-bezier(.37, 0, .63, 1) forwards; }
        .intro-camion svg { width: 130px; height: 130px; }
        @keyframes intro-camion {
            from { transform: translateX(-130px); }
            to { transform: translateX(100vw); }
        }
        .intro-pista { position: absolute; bottom: 56px; left: 0; right: 0; text-align: center; font-size: 16px; font-weight: 600; color: var(--brand-primary); opacity: 0; }
        .intro-pista.latido { animation: latido 2.2s linear infinite; }
        @keyframes latido { 0% { opacity: 0; } 35% { opacity: 1; } 75% { opacity: .45; } 100% { opacity: 1; } }
        """;
}
