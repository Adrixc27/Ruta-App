// Puente con el navegador para lo que en escritorio hace Windows: ubicación y portapapeles.
window.rutaWeb = {
    // [lat, lng], o null si no hay permiso, no hay señal o tarda demasiado
    ubicacion: function () {
        return new Promise(function (resolver) {
            if (!navigator.geolocation) { resolver(null); return; }
            navigator.geolocation.getCurrentPosition(
                function (p) { resolver([p.coords.latitude, p.coords.longitude]); },
                function () { resolver(null); },
                { enableHighAccuracy: false, timeout: 8000, maximumAge: 300000 });
        });
    },

    // true si el texto quedó copiado
    copiar: async function (texto) {
        try {
            await navigator.clipboard.writeText(texto);
            return true;
        } catch (e) {
            // Respaldo para navegadores sin permiso del portapapeles
            var area = document.createElement('textarea');
            area.value = texto;
            area.setAttribute('readonly', '');
            area.style.position = 'fixed';
            area.style.opacity = '0';
            document.body.appendChild(area);
            area.select();
            var ok = false;
            try { ok = document.execCommand('copy'); } catch (e2) { }
            document.body.removeChild(area);
            return ok;
        }
    }
};
