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
        // Marca con clave (un aviso): al tocarla se avisa al componente, que abre su publicación
        if (m.clave && e && e.ref) {
            mk.on('click', function () { e.ref.invokeMethodAsync('AlTocarMarca', m.clave); });
        }
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

    // En pantallas angostas las tarjetas de vidrio (panel, tarjeta de la ruta) flotan sobre el mapa: el encuadre deja libres las franjas
    // que tapan, arriba o abajo. Devuelve null cuando el panel no está sobre el mapa (escritorio).
    function franjas(e) {
        var cont = e.mapa.getContainer().parentElement;
        var split = cont && cont.parentElement;
        var panel = split && split.querySelector(':scope > .panel-lateral');
        if (!panel || getComputedStyle(panel).position !== 'absolute') return null;
        var c = cont.getBoundingClientRect(), alto = c.height;
        var f = { arriba: 72, abajo: 28, lado: 32 };
        var visible = getComputedStyle(panel).visibility !== 'hidden' && !panel.classList.contains('cerrada');
        if (visible) {
            var r = panel.getBoundingClientRect();
            if ((r.top + r.bottom) / 2 - c.top > alto / 2) f.abajo = c.bottom - r.top + 12;          // tarjeta abajo (Comparar)
            else if (r.top - c.top < alto * 0.3) f.arriba = r.bottom - c.top + 12;                   // tarjeta arriba (Planificar)
        }
        var tarjeta = split.querySelector('.mapa-tarjeta');
        if (tarjeta && getComputedStyle(tarjeta).display !== 'none') f.abajo = Math.max(f.abajo, c.bottom - tarjeta.getBoundingClientRect().top + 12);
        else if (split.querySelector('.panel-rutas.cerrada')) f.abajo = Math.max(f.abajo, 110);   // la tarjeta de la ruta aún no se pintó
        f.arriba = Math.min(f.arriba, alto * 0.4);
        f.abajo = Math.min(f.abajo, alto * 0.6);
        return f;
    }

    function encuadrar(e, pts, relleno, zoomMax) {
        if (!pts || !pts.length) return;
        e.mapa.invalidateSize();
        var f = franjas(e);
        if (pts.length === 1) {
            e.mapa.setView(pts[0], zoomMax || 15);
            if (f) e.mapa.panBy([0, (f.abajo - f.arriba) / 2], { animate: false });
            return;
        }
        var r = relleno || 90;
        var abajo = e.mapa.getSize().y > 480 ? r + 90 : r;   // deja libre abajo la tarjeta de la ruta
        var opciones = { maxZoom: zoomMax || 17, animate: true };
        if (f) { opciones.paddingTopLeft = [f.lado, f.arriba]; opciones.paddingBottomRight = [f.lado, f.abajo]; }
        else { opciones.paddingTopLeft = [r, r]; opciones.paddingBottomRight = [r, abajo]; }
        e.mapa.fitBounds(L.latLngBounds(pts), opciones);
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
