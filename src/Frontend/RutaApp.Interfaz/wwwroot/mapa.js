// Adaptador mínimo de Leaflet: C# manda lo que hay que dibujar (MapaLeaflet.cs) y aquí solo se llama a la librería.
window.rutaMapa = (function () {
    var mapas = {};
    var CENTRO = [28.6353, -106.0889];
    var ZOOM_PARADAS = 14;
    var ZOOM_PUNTOS = 15;

    function marcador(m, e) {
        // En mapas angostos los rótulos fijos tapan todo: salen al pasar el cursor o al tocar
        var fijo = !!m.perm && (!e || e.mapa.getSize().x >= 720);
        var mk = L.marker([m.lat, m.lng], {
            icon: L.divIcon({ className: '', html: m.html, iconSize: [m.w, m.h], iconAnchor: [m.ax, m.ay] }),
            zIndexOffset: m.z || 0,
            keyboard: false
        });
        if (m.rotulo) {
            mk.bindTooltip(m.rotulo, { permanent: fijo, direction: m.dir || 'top', offset: m.off || [0, 0], className: 'pill' });
        }
        if (m.popup) mk.bindPopup(m.popup);
        return mk;
    }

    function linea(l, destino) {
        L.polyline(l.pts, {
            color: l.color,
            weight: l.peso,
            opacity: l.opacidad,
            dashArray: l.dash || null,
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
            reportes: L.layerGroup().addTo(mapa),
            ubicaciones: {},
            yo: null,
            hayParadas: false,
            hayPuntos: false
        };
        // Paradas y puntos de interés solo se dibujan con el zoom cerca, para que el mapa no se llene ni se trabe
        e.revisarZoom = function () {
            var z = mapa.getZoom();
            alternar(mapa, e.paradas, e.hayParadas && z >= ZOOM_PARADAS);
            alternar(mapa, e.puntos, e.hayPuntos && z >= ZOOM_PUNTOS);
        };
        mapa.on('zoomend', e.revisarZoom);
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

    function encuadrarUbicaciones(e) {
        var pts = Object.keys(e.ubicaciones).map(function (k) { return e.ubicaciones[k].ll; });
        if (pts.length) encuadrar(e, pts, 110, 16);
    }

    function con(id, accion) {
        var e = mapas[id];
        if (e) accion(e);
    }

    return {
        // ref: DotNetObjectReference del componente; recibe los clics cuando la selección está activa
        iniciar: function (id, ref) {
            if (mapas[id]) return;
            var e = mapas[id] = nuevo(id);
            e.ref = ref || null;
            e.seleccion = false;
            e.mapa.on('click', function (ev) {
                if (e.seleccion && e.ref) e.ref.invokeMethodAsync('AlHacerClic', ev.latlng.lat, ev.latlng.lng);
            });
        },

        seleccion: function (id, activo) {
            con(id, function (e) {
                e.seleccion = !!activo;
                e.mapa.getContainer().classList.toggle('mapa-seleccion', !!activo);
            });
        },

        quitar: function (id) {
            con(id, function (e) {
                e.ref = null;
                e.mapa.remove();
                delete mapas[id];
            });
        },

        invalidar: function (id) { con(id, function (e) { e.mapa.invalidateSize(); }); },

        // spec: { lineas:[{pts,color,peso,opacidad,dash}], marcadores:[...], paradas:[...], relleno, zoomMax, encuadrar }
        dibujarRuta: function (id, spec) {
            con(id, function (e) {
                e.ruta.clearLayers();
                e.paradas.clearLayers();

                var todos = [];
                spec.lineas.forEach(function (l) { linea(l, e.ruta); todos = todos.concat(l.pts); });
                spec.paradas.forEach(function (m) { marcador(m, e).addTo(e.paradas); });
                spec.marcadores.forEach(function (m) { marcador(m, e).addTo(e.ruta); todos.push([m.lat, m.lng]); });

                e.hayParadas = spec.paradas.length > 0;
                e.revisarZoom();
                if (spec.encuadrar) encuadrar(e, todos, spec.relleno, spec.zoomMax);
            });
        },

        limpiarRuta: function (id) {
            con(id, function (e) {
                e.ruta.clearLayers();
                e.paradas.clearLayers();
                e.hayParadas = false;
                e.revisarZoom();
            });
        },

        // Un lugar elegido por el usuario (origen, destino o parada intermedia)
        ponerUbicacion: function (id, tipo, m) {
            con(id, function (e) {
                if (e.ubicaciones[tipo]) e.mapa.removeLayer(e.ubicaciones[tipo].capa);
                e.ubicaciones[tipo] = { capa: marcador(m, e).addTo(e.mapa), ll: [m.lat, m.lng] };
                encuadrarUbicaciones(e);
            });
        },

        quitarUbicacion: function (id, tipo) {
            con(id, function (e) {
                if (!e.ubicaciones[tipo]) return;
                e.mapa.removeLayer(e.ubicaciones[tipo].capa);
                delete e.ubicaciones[tipo];
                encuadrarUbicaciones(e);
            });
        },

        sugerencias: function (id, lista) {
            con(id, function (e) {
                e.sugerencias.clearLayers();
                if (!lista || !lista.length) return;
                lista.forEach(function (m) { marcador(m, e).addTo(e.sugerencias); });
                encuadrar(e, lista.map(function (m) { return [m.lat, m.lng]; }), 90, 16);
            });
        },

        reportes: function (id, lista) {
            con(id, function (e) {
                e.reportes.clearLayers();
                (lista || []).forEach(function (m) { marcador(m, e).addTo(e.reportes); });
            });
        },

        puntos: function (id, lista) {
            con(id, function (e) {
                e.puntos.clearLayers();
                (lista || []).forEach(function (m) { marcador(m, e).addTo(e.puntos); });
                e.hayPuntos = !!(lista && lista.length);
                e.revisarZoom();
            });
        },

        // Mi posición en vivo (punto azul que late)
        posicion: function (id, m, centrar) {
            con(id, function (e) {
                if (e.yo) e.mapa.removeLayer(e.yo);
                if (!m) { e.yo = null; return; }
                e.yo = marcador(m, e).addTo(e.mapa);
                if (centrar) e.mapa.setView([m.lat, m.lng], Math.max(e.mapa.getZoom(), 16), { animate: true });
            });
        },

        zoomA: function (id, lat, lng) {
            con(id, function (e) { e.mapa.setView([lat, lng], Math.max(e.mapa.getZoom(), 16), { animate: true }); });
        }
    };
})();
