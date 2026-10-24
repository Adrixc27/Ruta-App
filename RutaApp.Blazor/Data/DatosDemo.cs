namespace RutaApp.Blazor.Data;

// ─────────────────────────────────────────────────────────────
//  Datos de demostración (los mismos textos del diseño de Figma)
// ─────────────────────────────────────────────────────────────

public static class DatosDemo
{
    // Rutas de ejemplo: cada una cruza la ciudad pasando por el Centro (Plaza Mayor), como las líneas troncales.
    // Los puntos (Waypoints) son aproximados; el recorrido por calles se calcula con OSRM (ServicioTrazos.PrepararAsync).
    private static readonly double[] Centro = { 28.6357, -106.0877 };

    private static List<double[]> W(double lat1, double lng1, double lat2, double lng2)
    {
        return new List<double[]> { new[] { lat1, lng1 }, Centro, new[] { lat2, lng2 } };
    }

    public static List<Ruta> CrearRutas()
    {
        var lista = new List<Ruta>
        {
            new Ruta { Numero = 3, Nombre = "Ruta 3 - Exprés Centro", Modo = "Exprés", ModoTexto = "Exprés",
                FrecuenciaMin = 10, Tarifa = 1.50m, Horario = "6:00 - 22:00", Lineas = "Bus Exprés", Etiqueta = "Económico",
                OrigenNombre = "Terminal Norte", DestinoNombre = "Jardines de Oriente",
                Waypoints = W(28.7000, -106.1100, 28.6200, -106.0250) },

            new Ruta { Numero = 4, Nombre = "Ruta 4 - Zona Centro", FrecuenciaMin = 12, Tarifa = 2.75m,
                Desvio = true, Horario = "5:30 - 22:30", Lineas = "Bus 104",
                OrigenNombre = "Terminal Sur", DestinoNombre = "Bosque Azul",
                Waypoints = W(28.5900, -106.1000, 28.6900, -106.0450) },

            new Ruta { Numero = 6, Nombre = "Ruta 6 - Villas", FrecuenciaMin = 10, Tarifa = 2.75m,
                Horario = "5:30 - 23:00", Lineas = "Bus 102", Etiqueta = "Recomendado", Guardada = true,
                OrigenNombre = "Terminal Villas", DestinoNombre = "Colonia Avalos",
                Waypoints = W(28.6100, -106.1450, 28.6300, -106.0400) },

            new Ruta { Numero = 7, Nombre = "Ruta 7 - Bosque Azul", Modo = "Metro", ModoTexto = "Metro + Bus",
                FrecuenciaMin = 15, Tarifa = 3.50m, Horario = "5:00 - 22:30",
                Lineas = "Metro L + Bus 12", Etiqueta = "Más rápido", Guardada = true,
                OrigenNombre = "Komatsu", DestinoNombre = "Rancheria Juárez",
                Waypoints = W(28.7100, -106.0600, 28.6150, -106.0450) },

            new Ruta { Numero = 9, Nombre = "Ruta 9 - Plan de Ayala", FrecuenciaMin = 12, Tarifa = 2.75m, Lineas = "Bus 109",
                OrigenNombre = "Plan de Ayala", DestinoNombre = "Panamericana",
                Waypoints = W(28.6620, -106.1200, 28.6050, -106.0600) },

            new Ruta { Numero = 12, Nombre = "Ruta 12 - Circunvalación II", FrecuenciaMin = 20, Tarifa = 2.75m,
                Lineas = "Bus 112", Guardada = true,
                OrigenNombre = "Circunvalación II", DestinoNombre = "Sur",
                Waypoints = W(28.6800, -106.0950, 28.6000, -106.0850) },

            new Ruta { Numero = 15, Nombre = "Ruta 15 - Campesina", FrecuenciaMin = 18, Tarifa = 2.75m, Lineas = "Bus 115",
                OrigenNombre = "Poniente", DestinoNombre = "Campesina",
                Waypoints = W(28.6350, -106.1500, 28.6500, -106.0250) }
        };

        foreach (var r in lista)
        {
            var a = r.Waypoints[0]; var b = r.Waypoints[r.Waypoints.Count - 1];
            r.LatO = a[0]; r.LngO = a[1]; r.LatD = b[0]; r.LngD = b[1];
        }
        return lista;
    }

    public static List<Reciente> CrearRecientes(List<Ruta> rutas)
    {
        var lista = new List<Reciente>();
        foreach (var r in rutas) lista.Add(new Reciente { Ruta = r, Texto = r.TarifaTexto + " · " + r.MinutosTexto });
        return lista;
    }

    // Historial de EJEMPLO: calles, lugares, horas y camiones inventados (coordenadas dentro de Chihuahua capital)
    public static List<ViajeHistorial> CrearHistorial()
    {
        return new List<ViajeHistorial>
        {
            new ViajeHistorial { Id = 1, Origen = "Ubicación actual (Av. Universidad)", Destino = "Tecnológico", Cuando = "Hoy, 8:05", Minutos = 32,
                Camiones = "Ruta 9 → Ruta 4", OrigenLat = 28.6300, OrigenLng = -106.0400, DestinoLat = 28.6620, DestinoLng = -106.1200 },
            new ViajeHistorial { Id = 2, Origen = "Tecnológico", Destino = "Gimnasio", Cuando = "Ayer, 18:30", Minutos = 24,
                Camiones = "Ruta 7", OrigenLat = 28.6620, OrigenLng = -106.1200, DestinoLat = 28.7100, DestinoLng = -106.0600 },
            new ViajeHistorial { Id = 3, Origen = "Ubicación actual (Calle Aldama)", Destino = "Plaza Mayor", Cuando = "Hace 2 días", Minutos = 20,
                Camiones = "Ruta 6", OrigenLat = 28.6300, OrigenLng = -106.0400, DestinoLat = 28.6357, DestinoLng = -106.0877 },
            new ViajeHistorial { Id = 4, Origen = "Plaza Mayor", Destino = "Bosque Azul", Cuando = "Hace 4 días", Minutos = 27,
                Camiones = "Ruta 4", OrigenLat = 28.6357, OrigenLng = -106.0877, DestinoLat = 28.6900, DestinoLng = -106.0450 },
            new ViajeHistorial { Id = 5, Origen = "Ubicación actual (Av. Pacheco)", Destino = "Terminal Sur", Cuando = "Hace 1 semana", Minutos = 35,
                Camiones = "Ruta 12 → Ruta 4", OrigenLat = 28.6300, OrigenLng = -106.0400, DestinoLat = 28.5900, DestinoLng = -106.1000 }
        };
    }

    public static List<Trayecto> CrearTrayectos()
    {
        return new List<Trayecto>
        {
            new Trayecto { Alias = "Trabajo", Recorrido = "Kennedy > BOWI", Detalle = "18 min, de lunes a viernes" },
            new Trayecto { Alias = "Gimnasio", Recorrido = "Plan de Ayala > Komatsu", Detalle = "14 min" },
            new Trayecto { Alias = "Casa de mamá", Recorrido = "Circunvalacion II", Detalle = "25 min, domingos" }
        };
    }

    private static Etiqueta R(int n) { return new Etiqueta { Texto = "Ruta " + n, EsRuta = true }; }
    private static Etiqueta Z(string t) { return new Etiqueta { Texto = t, EsRuta = false }; }

    public static List<Publicacion> CrearPublicaciones()
    {
        return new List<Publicacion>
        {
            new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 2 h",
                Titulo = "Aviso de servicio",
                Cuerpo = "La Ruta 4 tendrá un desvío temporal por obras en Zona Centro. Comparte alternativas con la comunidad.",
                Etiquetas = new List<Etiqueta> { R(4), Z("Zona Centro") }, Likes = 12, Comentarios = 4, Orden = 100, RutaNumero = 4 },

            new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 5 h",
                Titulo = "Tu experiencia importa",
                Cuerpo = "¿Qué parada necesita mejor señalización? Cuéntanos cómo mejorar tus recorridos diarios.",
                Etiquetas = new List<Etiqueta> { Z("Paradas") }, Likes = 19, Comentarios = 5, Orden = 90 },

            new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "ayer",
                Titulo = "Nueva conexión",
                Cuerpo = "Se propone extender la Ruta 7 hasta Bosque Azul. Revisa el mapa y deja tu opinión.",
                Etiquetas = new List<Etiqueta> { R(7), Z("Bosque Azul") }, Likes = 26, Comentarios = 6, Orden = 80, RutaNumero = 7 },

            new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 1 d",
                Titulo = "Horario especial en Ruta 6",
                Cuerpo = "Este domingo la Ruta 6 - Villas circulará cada 20 minutos por mantenimiento de unidades.",
                Etiquetas = new List<Etiqueta> { R(6), Z("Villas") }, Likes = 8, Comentarios = 2, Orden = 70, RutaNumero = 6 },

            new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 1 d",
                Titulo = "¿Horario de la Ruta 3 Exprés?",
                Cuerpo = "Voy de Circunvalación II al centro y quiero saber a qué hora pasa el exprés por la mañana.",
                Etiquetas = new List<Etiqueta> { R(3) }, Likes = 5, Comentarios = 9, Orden = 60, RutaNumero = 3 },

            new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 2 d",
                Titulo = "Más frecuencia en Plan de Ayala",
                Cuerpo = "Sugerimos una unidad extra en hora pico, de 7 a 9 am, para reducir el tiempo de espera.",
                Etiquetas = new List<Etiqueta> { Z("Plan de Ayala") }, Likes = 31, Comentarios = 12, Orden = 50, RutaNumero = 9 }
        };
    }

    public static List<Publicacion> CrearPublicacionesExtra()
    {
        return new List<Publicacion>
        {
            new Publicacion { Tipo = "aviso", Autor = "Movilidad Chihuahua", Oficial = true, Tiempo = "hace 3 d",
                Titulo = "Mantenimiento en Ruta 12",
                Cuerpo = "La Ruta 12 - Circunvalación II no circulará el sábado de 2 a 5 am por trabajos de mantenimiento.",
                Etiquetas = new List<Etiqueta> { R(12), Z("Circunvalación II") }, Likes = 4, Comentarios = 1, Orden = 40, RutaNumero = 12 },

            new Publicacion { Tipo = "pregunta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 3 d",
                Titulo = "¿Aceptan tarjeta?",
                Cuerpo = "¿Las unidades de la Ruta 15 ya aceptan pago con tarjeta o solo efectivo?",
                Etiquetas = new List<Etiqueta> { R(15), Z("Komatsu") }, Likes = 7, Comentarios = 3, Orden = 30, RutaNumero = 15 },

            new Publicacion { Tipo = "propuesta", Autor = "Vecinos de Chihuahua", Tiempo = "hace 4 d",
                Titulo = "Paradas con techo",
                Cuerpo = "Proponemos instalar techo en las paradas más concurridas de Zona Centro para los días de lluvia y sol fuerte.",
                Etiquetas = new List<Etiqueta> { Z("Paradas"), Z("Zona Centro") }, Likes = 22, Comentarios = 8, Orden = 20 }
        };
    }
}
