namespace RutaApp.Interfaz.Servicios;

// Catálogo de rutas, búsqueda por paradas, recorridos y rutas guardadas.
public partial class EstadoApp
{
    // Buscar por paradas
    /// <summary>Paradas de las rutas cuyo nombre contiene el texto (sin repetir la misma parada de varias rutas).</summary>
    public List<ParadaBuscada> BuscarParadas(string texto, int max = 6)
    {
        var lista = new List<ParadaBuscada>();
        string q = (texto ?? "").Trim();
        if (q.Length < 3) return lista;
        foreach (var r in Rutas)
        {
            if (r.Oficial == null) continue;
            foreach (var p in r.Oficial.Paradas)
            {
                string nombre = Formato.NombreParada(p.Nombre);
                if (!Formato.Coincide(nombre, q)) continue;
                bool repetida = lista.Any(x => Math.Abs(x.Lat - p.Latitud) < 0.0004 && Math.Abs(x.Lng - p.Longitud) < 0.0004 || x.Nombre == nombre);
                if (!repetida) lista.Add(new ParadaBuscada(nombre, p.Latitud, p.Longitud, r.Nombre, r.Numero));
                if (lista.Count >= max) return lista;
            }
        }
        return lista;
    }

    /// <summary>Nombre de la primera parada de la ruta que coincide con el texto, o null.</summary>
    public string ParadaQueCoincide(Ruta r, string texto)
    {
        if (r.Oficial == null || string.IsNullOrWhiteSpace(texto)) return null;
        foreach (var p in r.Oficial.Paradas)
            if (Formato.Coincide(Formato.NombreParada(p.Nombre), texto.Trim())) return Formato.NombreParada(p.Nombre);
        return null;
    }

    // Datos
    /// <summary>Rutas reales de la base de datos. Vacía mientras no se carguen (no hay rutas de ejemplo).</summary>
    public List<Ruta> Rutas { get; private set; } = new();

    /// <summary>Publicaciones de la comunidad (de la base de datos).</summary>
    public List<Publicacion> Publicaciones { get; } = new();

    /// <summary>Historial de viajes del usuario. Vacío para invitados y para quien aún no ha viajado.</summary>
    public List<ViajeHistorial> Historial { get; } = new();

    /// <summary>Viajes guardados por el usuario (Perfil → Rutas guardadas → Personalizadas).</summary>
    public List<Trayecto> Trayectos { get; } = new();

    /// <summary>Casa, escuela y otros puntos de referencia del usuario para planificar.</summary>
    public List<LugarGuardado> Lugares { get; } = new();

    /// <summary>Ruta que se está viendo en el mapa de la pantalla Rutas (se conserva al cambiar de pantalla).</summary>
    public Ruta RutaSeleccionada { get; set; }

    private bool _rutasReales;

    /// <summary>true si la última carga de rutas no pudo conectarse a la base de datos.</summary>
    public bool ErrorRutas { get; private set; }

    /// <summary>Qué falló en la conexión a MySQL (en español), o null si todo bien.</summary>
    public string ErrorBaseDatos { get; private set; }

    /// <summary>Carga las rutas de la base de datos. Devuelve false si no se pudo.</summary>
    public async Task<bool> CargarRutasRealesAsync()
    {
        if (_rutasReales) return true;
        var reales = await _repositorio.CargarAsync();
        ErrorRutas = reales == null;
        if (reales == null)
        {
            // Se explica qué falla (MySQL apagado, contraseña, falta la base de datos…)
            var (ok, mensaje) = await Connection.ProbarAsync();
            ErrorBaseDatos = ok ? "Hay conexión con MySQL, pero no se encontraron rutas con recorrido en la base de datos." : mensaje;
            return false;
        }
        ErrorBaseDatos = null;

        Rutas = reales;
        _rutasReales = true;
        RutaSeleccionada = null;
        return true;
    }

    public async Task ReintentarRutasAsync()
    {
        MostrarCarga("Cargando rutas…");
        try
        {
            await CargarRutasRealesAsync();
            await CargarFavoritosAsync();
        }
        finally
        {
            OcultarCarga();
        }
    }

    // Recorridos de las rutas
    /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM).</summary>
    public Task<List<double[]>> TrazoOsrmAsync(string coordenadas) => _trazos.TrazoOsrmAsync(coordenadas);

    // Rutas guardadas (individuales)
    public int TotalGuardadas => Rutas.Count(r => r.Guardada);

    public Ruta BuscarRuta(int numero) => Rutas.FirstOrDefault(r => r.Numero == numero);

    public void AlternarGuardada(Ruta ruta)
    {
        if (!RequiereCuenta("Inicia sesión para guardar rutas y encontrarlas siempre en tu perfil.")) return;
        ruta.Guardada = !ruta.Guardada;
        Notificar();
        MostrarAviso(ruta.Guardada ? "Ruta guardada: " + ruta.Nombre : "Ruta quitada de guardadas");
        _ = GuardarFavoritoAsync(ruta, ruta.Guardada);
    }

    private async Task GuardarFavoritoAsync(Ruta ruta, bool guardar)
    {
        if (!ConCuenta || ruta.EsViaje) return;
        try
        {
            if (guardar) await _favoritos.AgregarAsync(Usuario.Id, ruta.Numero);
            else await _favoritos.QuitarAsync(Usuario.Id, ruta.Numero);
        }
        catch (Exception)
        {
            ruta.Guardada = !guardar;   // no se pudo guardar: se deshace
            Notificar();
        }
    }
}
