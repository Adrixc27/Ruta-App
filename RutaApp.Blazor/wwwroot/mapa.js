/* R.U.T.A. — mapa Leaflet por lotes.
   Cada función recibe TODO lo que hay que dibujar en una sola llamada desde C# (una ruta con 80 paradas son 1 llamada, no 400):
   por eso el mapa responde rápido aunque la ruta sea larga. Las paradas solo se pintan al acercar el zoom. */
window.rutaMapa = (function () {
    var mapas = {};
    var CENTRO = [28.6353, -106.0889];
    var ZOOM_PARADAS = 14;
    var ZOOM_PUNTOS = 15;

    function esc(t) {
        return String(t == null ? '' : t).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    function icono(m) {
        return L.divIcon({
            className: '',
            html: m.html,
            iconSize: [m.w, m.h],
            iconAnchor: [m.ax, m.ay]
        });
    }

    // Un marcador a partir de su descripción {lat,lng,html,w,h,ax,ay,rotulo,dir,off,perm,popup,z}
    function marcador(m, e) {
        // Los rótulos fijos solo caben en mapas anchos; en uno angosto salen al pasar el cursor o al tocar
        var fijo = !!m.perm && (!e || e.mapa.getSize().x >= 720);
        var mk = L.marker([m.lat, m.lng], { icon: icono(m), zIndexOffset: m.z || 0, keyboard: false });
        if (m.rotulo) {
            mk.bindTooltip(esc(m.rotulo), {
                permanent: fijo,
                direction: m.dir || 'top',
                offset: m.off || [0, 0],
                className: 'pill'
            });
        }
        if (m.popup) mk.bindPopup(m.popup);
        return mk;
    }

    function linea(l, destino) {
        if (l.borde) {
            L.polyline(l.pts, { color: '#ffffff', weight: (l.peso || 5) + 4, opacity: 0.95, lineJoin: 'round', lineCap: 'round', interactive: false }).addTo(destino);
        }
        L.polyline(l.pts, {
            color: l.color || '#2563EB',
            weight: l.peso || 5,
            opacity: l.opacidad == null ? 1 : l.opacidad,
            dashArray: l.dash ? l.dash : (l.punteada ? '2,9' : null),
            lineJoin: 'round',
            lineCap: 'round',
            interactive: false
        }).addTo(destino);
    }

    function nuevo(id) {
        var mapa = L.map(id, { zoomControl: false, preferCanvas: true, zoomSnap: 0.5, zoomDelta: 1, wheelPxPerZoomLevel: 80 });
        mapa.setView(CENTRO, 13);
        L.control.zoom({ position: 'topright' }).addTo(mapa);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
            maxZoom: 19,
            updateWhenIdle: true,
            keepBuffer: 4
        }).addTo(mapa);

        var e = {
            mapa: mapa,
            ruta: L.layerGroup().addTo(mapa),
            paradas: L.layerGroup(),
            puntos: L.layerGroup(),
            sugerencias: L.layerGroup().addTo(mapa),
            ubicaciones: {},
            yo: null,
            hayParadas: false,
            hayPuntos: false
        };
        // Las paradas y los puntos de interés solo se dibujan con el zoom cerca: así el mapa no se llena ni se traba
        function revisarZoom() {
            var z = mapa.getZoom();
            alternar(e.mapa, e.paradas, e.hayParadas && z >= ZOOM_PARADAS);
            alternar(e.mapa, e.puntos, e.hayPuntos && z >= ZOOM_PUNTOS);
        }
        mapa.on('zoomend', revisarZoom);
        e.revisarZoom = revisarZoom;
        return e;
    }

    function alternar(mapa, capa, mostrar) {
        var esta = mapa.hasLayer(capa);
        if (mostrar && !esta) capa.addTo(mapa);
        else if (!mostrar && esta) mapa.removeLayer(capa);
    }

    function encuadrar(e, pts, relleno, zoomMax) {
        if (!pts || !pts.length) return;
        e.mapa.invalidateSize();
        if (pts.length === 1) { e.mapa.setView(pts[0], zoomMax || 15); return; }
        var r = relleno || 90;
        var abajo = e.mapa.getSize().y > 480 ? r + 90 : r;   // deja libre abajo la tarjeta de la ruta
        e.mapa.fitBounds(L.latLngBounds(pts), { paddingTopLeft: [r, r], paddingBottomRight: [r, abajo], maxZoom: zoomMax || 17, animate: true });
    }

    function get(id) { return mapas[id]; }

    return {
        /* ref: DotNetObjectReference del componente; recibe los clics cuando la selección está activa */
        iniciar: function (id, ref) {
            if (mapas[id]) return;
            var e = mapas[id] = nuevo(id);
            e.ref = ref || null;
            e.seleccion = false;
            e.mapa.on('click', function (ev) {
                if (e.seleccion && e.ref) e.ref.invokeMethodAsync('AlHacerClic', ev.latlng.lat, ev.latlng.lng);
            });
        },

        /* Activa o desactiva "tocar el mapa para elegir un lugar" (cursor en cruz) */
        seleccion: function (id, activo) {
            var e = get(id);
            if (!e) return;
            e.seleccion = !!activo;
            e.mapa.getContainer().classList.toggle('mapa-seleccion', !!activo);
        },

        quitar: function (id) {
            var e = get(id);
            if (!e) return;
            e.ref = null;
            e.mapa.remove();
            delete mapas[id];
        },

        ancho: function () { return window.innerWidth; },

        invalidar: function (id) { var e = get(id); if (e) e.mapa.invalidateSize(); },

        /* spec: { lineas:[{pts,color,peso,punteada,borde}], marcadores:[...], paradas:[...], relleno } */
        dibujarRuta: function (id, spec) {
            var e = get(id);
            if (!e) return;
            e.ruta.clearLayers();
            e.paradas.clearLayers();

            var todos = [];
            (spec.lineas || []).forEach(function (l) { linea(l, e.ruta); todos = todos.concat(l.pts); });
            (spec.paradas || []).forEach(function (m) { marcador(m, e).addTo(e.paradas); });
            (spec.marcadores || []).forEach(function (m) { marcador(m, e).addTo(e.ruta); todos.push([m.lat, m.lng]); });

            e.hayParadas = (spec.paradas || []).length > 0;
            e.revisarZoom();
            if (spec.encuadrar !== false) encuadrar(e, todos, spec.relleno, spec.zoomMax);
        },

        limpiarRuta: function (id) {
            var e = get(id);
            if (!e) return;
            e.ruta.clearLayers();
            e.paradas.clearLayers();
            e.hayParadas = false;
            e.revisarZoom();
        },

        /* Un lugar elegido por el usuario (origen, destino o parada intermedia) */
        ponerUbicacion: function (id, tipo, m) {
            var e = get(id);
            if (!e) return;
            if (e.ubicaciones[tipo]) e.mapa.removeLayer(e.ubicaciones[tipo].capa);
            var mk = marcador(m, e).addTo(e.mapa);
            e.ubicaciones[tipo] = { capa: mk, ll: [m.lat, m.lng] };
            this.encuadrarUbicaciones(id);
        },

        quitarUbicacion: function (id, tipo) {
            var e = get(id);
            if (!e || !e.ubicaciones[tipo]) return;
            e.mapa.removeLayer(e.ubicaciones[tipo].capa);
            delete e.ubicaciones[tipo];
            this.encuadrarUbicaciones(id);
        },

        encuadrarUbicaciones: function (id) {
            var e = get(id);
            if (!e) return;
            var pts = Object.keys(e.ubicaciones).map(function (k) { return e.ubicaciones[k].ll; });
            if (pts.length) encuadrar(e, pts, 110, 16);
        },

        sugerencias: function (id, lista) {
            var e = get(id);
            if (!e) return;
            e.sugerencias.clearLayers();
            if (!lista || !lista.length) return;
            lista.forEach(function (m) { marcador(m, e).addTo(e.sugerencias); });
            encuadrar(e, lista.map(function (m) { return [m.lat, m.lng]; }), 90, 16);
        },

        puntos: function (id, lista) {
            var e = get(id);
            if (!e) return;
            e.puntos.clearLayers();
            (lista || []).forEach(function (m) { marcador(m, e).addTo(e.puntos); });
            e.hayPuntos = !!(lista && lista.length);
            e.revisarZoom();
        },

        /* Mi posición en vivo (punto azul que late) */
        posicion: function (id, m, centrar) {
            var e = get(id);
            if (!e) return;
            if (e.yo) e.mapa.removeLayer(e.yo);
            if (!m) { e.yo = null; return; }
            e.yo = marcador(m, e).addTo(e.mapa);
            if (centrar) e.mapa.setView([m.lat, m.lng], Math.max(e.mapa.getZoom(), 16), { animate: true });
        },

        centrar: function (id, lat, lng, zoom) {
            var e = get(id);
            if (!e) return;
            e.mapa.invalidateSize();
            e.mapa.setView([lat, lng], zoom || 16, { animate: true });
        },

        zoomA: function (id, lat, lng) {
            var e = get(id);
            if (e) e.mapa.setView([lat, lng], Math.max(e.mapa.getZoom(), 16), { animate: true });
        }
    };
})();
