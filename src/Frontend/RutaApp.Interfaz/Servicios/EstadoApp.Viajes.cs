using System.Globalization;

namespace RutaApp.Interfaz.Servicios;

// Viajes guardados, historial, lugares guardados e importación de códigos compartidos.
public partial class EstadoApp
{
    private async Task CargarDatosUsuarioAsync()
    {
        Historial.Clear();
        Trayectos.Clear();
        Lugares.Clear();
        if (!ConCuenta) return;

        var d = await _datos.CargarAsync(Usuario.Id);
        Historial.AddRange(d.Historial);
        Trayectos.AddRange(d.Trayectos);
        Lugares.AddRange(d.Lugares);
        bool habiaRepetidos = QuitarTrayectosRepetidos();
        SincronizarGuardados();
        if (habiaRepetidos) GuardarDatos();
    }

    // Dos viajes son el mismo si tienen igual origen, destino y camiones.
    public static string ClaveTrayecto(Trayecto t) => (t.Origen + ">" + t.Destino + "|" + t.Lineas).Trim().ToLowerInvariant();

    // Agrega el viaje a los guardados o devuelve el que ya estaba con la misma clave.
    private Trayecto GuardarSinRepetir(Trayecto t, out bool yaEstaba)
    {
        var existente = Trayectos.FirstOrDefault(x => ClaveTrayecto(x) == ClaveTrayecto(t));
        yaEstaba = existente != null;
        if (existente != null) return existente;
        Trayectos.Add(t);
        return t;
    }

    // Limpia los viajes repetidos que se hayan guardado antes. Devuelve true si quitó alguno.
    private bool QuitarTrayectosRepetidos()
    {
        var vistos = new Dictionary<string, Trayecto>();
        bool quito = false;
        foreach (var t in Trayectos.ToList())
        {
            string clave = ClaveTrayecto(t);
            if (vistos.TryAdd(clave, t)) continue;
            Trayectos.Remove(t);
            foreach (var v in Historial.Where(h => h.GuardadoId == t.Id)) v.GuardadoId = vistos[clave].Id;
            quito = true;
        }
        return quito;
    }

    private void GuardarDatos()
    {
        if (!ConCuenta) return;
        var instantanea = new DatosGuardados
        {
            Historial = Historial.ToList(),
            Trayectos = Trayectos.ToList(),
            Lugares = Lugares.ToList()
        };
        _ = _datos.GuardarAsync(Usuario.Id, instantanea);
    }

    // Guarda el viaje del historial, o lo quita de los guardados si ya lo estaba.
    public void AlternarGuardadoHistorial(ViajeHistorial v)
    {
        if (!RequiereCuenta("Inicia sesión para guardar tus viajes.")) return;
        if (v.EstaGuardado)
        {
            Trayectos.RemoveAll(t => t.Id == v.GuardadoId);
            SincronizarGuardados();
            GuardarDatos();
            return;
        }

        var t = new Trayecto
        {
            Alias = v.Destino,
            Recorrido = v.Origen + " > " + v.Destino,
            Detalle = v.Minutos + " min" + (v.Tarifa > 0 ? " · $" + v.Tarifa.ToString("0.00", CultureInfo.InvariantCulture) : ""),
            Origen = v.Origen, Destino = v.Destino,
            OrigenLat = v.OrigenLat, OrigenLng = v.OrigenLng, DestinoLat = v.DestinoLat, DestinoLng = v.DestinoLng,
            Minutos = v.Minutos, Tarifa = v.Tarifa, Lineas = v.Camiones,
            Tramos = CopiarTramos(v.Tramos), Escalas = v.Escalas.ToList()
        };
        var guardado = GuardarSinRepetir(t, out bool yaEstaba);
        v.GuardadoId = guardado.Id;
        Notificar();
        MostrarAviso(yaEstaba ? "Ese viaje ya estaba guardado" : "Viaje guardado en tu perfil");
        GuardarDatos();
    }

    private readonly List<Ruta> _viajesGuardados = new();

    // Guarda o quita una opción elegida en Comparar: una ruta normal se guarda como siempre y una opción de viaje,
    // como viaje personalizado en el perfil.
    public void AlternarFavoritoViaje(Ruta r)
    {
        if (!r.EsViaje) { AlternarGuardada(r); return; }
        if (!RequiereCuenta("Inicia sesión para guardar este viaje y verlo después en tu perfil.")) return;

        if (r.TrayectoFavorito != null)
        {
            Trayectos.Remove(r.TrayectoFavorito);
            _viajesGuardados.Remove(r);
            r.TrayectoFavorito = null;
            r.Guardada = false;
            Notificar();
            GuardarDatos();
            return;
        }

        var t = GuardarSinRepetir(TrayectoDeOpcion(r), out bool yaEstaba);
        r.TrayectoFavorito = t;
        r.Guardada = true;
        _viajesGuardados.Add(r);
        Notificar();
        MostrarAviso(yaEstaba ? "Ese viaje ya estaba guardado" : "Viaje guardado en tu perfil");
        GuardarDatos();
    }

    // El viaje de una opción de Comparar como Trayecto (para guardarlo o compartirlo), sin guardarlo todavía.
    public Trayecto TrayectoDeOpcion(Ruta r) => new Trayecto
    {
        Alias = Destino,
        Recorrido = Origen + " > " + Destino,
        Detalle = r.MinutosTexto + " · " + r.TarifaTexto + (string.IsNullOrWhiteSpace(r.Lineas) ? "" : " · " + r.Lineas),
        Origen = Origen, Destino = Destino,
        OrigenLat = OrigenLat, OrigenLng = OrigenLng, DestinoLat = DestinoLat, DestinoLng = DestinoLng,
        Minutos = r.Minutos, Tarifa = r.Tarifa, Lineas = r.Lineas,
        Tramos = CopiarTramos(r.Tramos), Escalas = (r.Escalas ?? Escalas).ToList()
    };

    // Quita un viaje guardado del perfil.
    public void QuitarTrayecto(Trayecto t)
    {
        Trayectos.Remove(t);
        SincronizarGuardados();
        GuardarDatos();
    }

    // Quita la marca de guardado del historial y de Comparar si su viaje ya no está en el perfil.
    private void SincronizarGuardados()
    {
        foreach (var v in Historial)
            if (v.EstaGuardado && !Trayectos.Any(t => t.Id == v.GuardadoId)) v.GuardadoId = null;
        foreach (var r in _viajesGuardados.ToList())
            if (!Trayectos.Contains(r.TrayectoFavorito)) { r.TrayectoFavorito = null; r.Guardada = false; _viajesGuardados.Remove(r); }
        Notificar();
    }

    private static List<Tramo> CopiarTramos(IEnumerable<Tramo> tramos) =>
        (tramos ?? Enumerable.Empty<Tramo>()).Select(t => new Tramo
        {
            EsBus = t.EsBus, Puntos = t.Puntos.Select(p => new[] { p[0], p[1] }).ToList(), TextoInicio = t.TextoInicio,
            Simbolo = t.Simbolo, Patron = t.Patron, Metros = t.Metros, Instrucciones = t.Instrucciones?.ToList(), ParadaInicio = t.ParadaInicio, ParadaFin = t.ParadaFin, EsperaMin = t.EsperaMin,
            TextoFin = t.TextoFin, Linea = t.Linea, Color = t.Color, Minutos = t.Minutos, AjustarACalles = t.AjustarACalles && !t.Ajustado, Reparar = t.Reparar && !t.Ajustado, Ajustado = t.Ajustado
        }).ToList();

    // Arma una Ruta de tipo viaje (para dibujarla en el mapa) a partir de un viaje del historial o guardado.
    public Ruta RutaDeViaje(IEnumerable<Tramo> tramos, List<Sugerencia> escalas, string origen, string destino,
                            int minutos, decimal tarifa, string lineas)
    {
        var copia = CopiarTramos(tramos);
        var r = new Ruta
        {
            Nombre = origen + " > " + destino,
            ModoTexto = "Autobús",
            Lineas = lineas,
            Minutos = minutos,
            Tarifa = tarifa,
            EsViaje = true,
            Tramos = copia,
            Escalas = escalas ?? new List<Sugerencia>(),
            OrigenNombre = origen,
            DestinoNombre = destino,
            Resumen = ""
        };
        if (copia.Count > 0 && copia[0].Puntos.Count > 0) { r.LatO = copia[0].Puntos[0][0]; r.LngO = copia[0].Puntos[0][1]; }
        if (copia.Count > 0 && copia[^1].Puntos.Count > 0)
        {
            var fin = copia[^1].Puntos[^1];
            r.LatD = fin[0]; r.LngD = fin[1];
        }
        return r;
    }

    public Ruta RutaDeViaje(ViajeHistorial v) =>
        RutaDeViaje(v.Tramos, v.Escalas, v.Origen, v.Destino, v.Minutos, v.Tarifa, v.Camiones);

    public Ruta RutaDeViaje(Trayecto t) =>
        RutaDeViaje(t.Tramos, t.Escalas, t.Origen, t.Destino, t.Minutos, t.Tarifa, t.Lineas);

    // Anota en el historial el viaje que acaba de empezar el usuario. Si salió de "Mi ubicación actual", busca en segundo plano
    // la calle donde estaba. Los invitados no tienen historial: devuelve null si no se anotó.
    public ViajeHistorial RegistrarViaje(Ruta r)
    {
        if (!ConCuenta) return null;

        bool desdeUbicacion = string.IsNullOrWhiteSpace(Origen) || Origen == MiUbicacion;
        double lat = OrigenLat, lng = OrigenLng;

        string camiones = r.Lineas ?? "";
        if (!string.IsNullOrWhiteSpace(r.Nombre) && !r.Nombre.StartsWith("Viaje en") && !r.Nombre.Contains(" + ") && r.Nombre != camiones)
            camiones = camiones.Length == 0 ? r.Nombre : camiones + " (" + r.Nombre + ")";

        var v = new ViajeHistorial
        {
            Id = Historial.Count == 0 ? 1 : Historial.Max(h => h.Id) + 1,
            Origen = desdeUbicacion ? "Ubicación actual" : Origen,
            Destino = Destino,
            OrigenLat = lat, OrigenLng = lng, DestinoLat = DestinoLat, DestinoLng = DestinoLng,
            Fecha = DateTime.Now,
            Minutos = r.Minutos,
            Tarifa = r.Tarifa,
            Camiones = camiones,
            Tramos = CopiarTramos(r.Tramos),
            Escalas = (r.Escalas ?? Escalas).ToList()
        };
        Historial.Insert(0, v);
        while (Historial.Count > 30) Historial.RemoveAt(Historial.Count - 1);
        GuardarDatos();

        if (desdeUbicacion && !double.IsNaN(lat))
            _ = CompletarCalleAsync(v, lat, lng);
        return v;
    }

    public void BorrarHistorial()
    {
        Historial.Clear();
        Notificar();
        GuardarDatos();
    }

    private async Task CompletarCalleAsync(ViajeHistorial v, double lat, double lng)
    {
        string calle = await _trazos.CalleDeAsync(lat, lng);
        if (string.IsNullOrEmpty(calle)) return;
        v.Origen = "Ubicación actual (" + calle + ")";
        Notificar();
        GuardarDatos();
    }

    // Lugares guardados (casa, escuela, referencias)
    public LugarGuardado AgregarLugar(string nombre, string tipo, string direccion, double lat, double lng)
    {
        if (!RequiereCuenta("Inicia sesión para guardar tus lugares (casa, escuela, trabajo…).")) return null;
        var l = new LugarGuardado { Nombre = nombre.Trim(), Tipo = tipo, Direccion = direccion ?? "", Lat = lat, Lng = lng };
        Lugares.Add(l);
        Notificar();
        MostrarAviso("Lugar guardado: " + l.Nombre);
        GuardarDatos();
        return l;
    }

    public void QuitarLugar(LugarGuardado l)
    {
        Lugares.Remove(l);
        Notificar();
        MostrarAviso("Lugar eliminado");
        GuardarDatos();
    }

    // Lee un código (RUTA1:...) y lo agrega al perfil. Devuelve el mensaje para mostrar.
    public string ImportarCodigo(string texto)
    {
        var c = CodigoCompartir.Leer(texto);
        if (c == null) return "Ese código no es válido. Cópialo completo, empieza con \"" + CodigoCompartir.Prefijo + "\".";
        if (!RequiereCuenta("Inicia sesión para guardar lo que te compartieron.")) return "";

        switch (c.Tipo)
        {
            case "lugar" when c.Lugar != null:
                if (Lugares.Any(l => Math.Abs(l.Lat - c.Lugar.Lat) < 1e-5 && Math.Abs(l.Lng - c.Lugar.Lng) < 1e-5))
                    return "Ese lugar ya está en tu perfil.";
                Lugares.Add(new LugarGuardado { Nombre = c.Lugar.Nombre, Tipo = c.Lugar.Tipo, Direccion = c.Lugar.Direccion, Lat = c.Lugar.Lat, Lng = c.Lugar.Lng });
                Notificar(); GuardarDatos();
                return "Se agregó el lugar \"" + c.Lugar.Nombre + "\" a tus lugares.";

            case "viaje" when c.Viaje != null:
                c.Viaje.Id = Guid.NewGuid().ToString("N");
                c.Viaje.Compartido = true;
                GuardarSinRepetir(c.Viaje, out bool yaEstaba);
                Notificar(); GuardarDatos();
                return yaEstaba ? "Ese viaje ya está en tus rutas guardadas." : "Se guardó el viaje \"" + c.Viaje.Alias + "\" en tus rutas guardadas.";

            case "ruta":
                var ruta = BuscarRuta(c.RutaId);
                if (ruta == null) return "Esa ruta no está disponible en este momento.";
                if (!ruta.Guardada) AlternarGuardada(ruta);
                return "Se guardó la ruta \"" + ruta.Nombre + "\".";
        }
        return "Ese código no se pudo leer.";
    }

    // Busca direcciones y lugares por nombre (para guardar un lugar escribiendo su dirección).
    public Task<List<Sugerencia>> BuscarLugaresAsync(string texto) => _trazos.BuscarLugaresAsync(texto);

    // Nombre de la calle de un punto del mapa (para ponerle nombre a un lugar elegido tocando el mapa).
    public Task<string> CalleDeAsync(double lat, double lng) => _trazos.CalleDeAsync(lat, lng);

    public static string Iniciales(string nombre)
    {
        var partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();
        return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
    }
}
