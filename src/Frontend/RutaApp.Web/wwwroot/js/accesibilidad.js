/* R.U.T.A. — accesibilidad.
   aplicar(opciones): activa o quita el modo daltonismo, la guía visual y "sin animaciones" en toda la app.
   Siempre (no solo con la guía) las listas que se tocan con el mouse también se pueden usar con el teclado. */
window.rutaA11y = (function () {
    var SELECTORES = '.ruta-item,.opcion-ruta,.viaje-historial,.sugerencia,.pasos li,.faq,.categoria-ayuda';
    var iniciado = false, guia = false, barra = null, temporizador = null;

    // Teclado: lo que es clicable (un div) recibe foco y responde a Enter y Espacio
    function parchar() {
        document.querySelectorAll(SELECTORES).forEach(function (el) {
            if (!el.hasAttribute('tabindex')) { el.setAttribute('tabindex', '0'); el.setAttribute('role', 'button'); }
        });
    }

    function alPulsar(e) {
        var t = e.target;
        if ((e.key === 'Enter' || e.key === ' ') && t && t.matches && t.matches(SELECTORES)) {
            e.preventDefault();
            t.click();
            return;
        }
        if (!guia || !e.altKey) return;
        var rutas = { '1': 'planificar', '2': 'rutas', '3': 'comunidad', '4': 'perfil', '5': 'configuracion' };
        if (rutas[e.key]) {
            var enlace = document.querySelector('a.nav-btn[href="' + rutas[e.key] + '"]');
            if (enlace) { e.preventDefault(); enlace.click(); }
        } else if (e.key === '0') {
            var cuerpo = document.getElementById('contenido-principal');
            if (cuerpo) { e.preventDefault(); cuerpo.focus(); }
        }
    }

    function describir(el) {
        if (!el || el === document.body) return '';
        var texto = el.getAttribute('aria-label') || el.getAttribute('title') || el.getAttribute('placeholder') ||
            (el.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 60);
        var tag = el.tagName.toLowerCase();
        var tipo = tag === 'a' ? 'enlace' : tag === 'input' || tag === 'textarea' ? 'campo' : tag === 'select' ? 'lista' : tag === 'button' || el.getAttribute('role') === 'button' ? 'botón' : '';
        return (tipo ? tipo + ' ' : '') + (texto ? '«' + texto + '»' : '');
    }

    function actualizarBarra() {
        if (!barra) return;
        var seccion = (document.querySelector('.miga.actual') || {}).innerText || (document.title || '').split('·')[0].trim();
        var foco = describir(document.activeElement);
        barra.title = 'Tab: avanzar · Enter: elegir · Alt+1…5: cambiar de sección · Alt+0: ir al contenido';
        barra.innerHTML = '<b>Estás en: ' + seccion + '</b>' + (foco ? ' · Ahora: ' + foco : '<span> · Tab: avanzar · Enter: elegir · Alt+1…5: secciones</span>');
    }

    function programar() { clearTimeout(temporizador); temporizador = setTimeout(actualizarBarra, 60); }

    function iniciarGuia() {
        if (barra) return;
        barra = document.createElement('div');
        barra.id = 'guia-estado';
        barra.setAttribute('role', 'status');
        barra.setAttribute('aria-live', 'polite');
        document.body.appendChild(barra);
        document.addEventListener('focusin', programar);
        window.addEventListener('popstate', programar);
        new MutationObserver(programar).observe(document.querySelector('title') || document.head, { childList: true, subtree: true, characterData: true });
        actualizarBarra();
    }

    function detenerGuia() {
        if (!barra) return;
        document.removeEventListener('focusin', programar);
        barra.remove();
        barra = null;
    }

    return {
        aplicar: function (o) {
            var c = document.documentElement.classList;
            c.toggle('a11y-dalton', !!o.daltonismo);
            c.toggle('a11y-guia', !!o.guiaVisual);
            c.toggle('a11y-sin-anim', !!o.sinAnimaciones);
            guia = !!o.guiaVisual;
            if (!iniciado) {
                iniciado = true;
                parchar();
                document.addEventListener('keydown', alPulsar);
                new MutationObserver(function () { clearTimeout(parchar._t); parchar._t = setTimeout(parchar, 80); })
                    .observe(document.body, { childList: true, subtree: true });
            }
            if (guia) iniciarGuia(); else detenerGuia();
        }
    };
})();
