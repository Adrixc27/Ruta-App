using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace RutaApp.Blazor.Services;

/// <summary>
/// Estado compartido entre pantallas (lo que en la app WPF era la clase estática Estado).
/// Es un servicio "scoped": cada sesión del navegador tiene el suyo.
/// </summary>
public class EstadoApp
{
    private readonly NavigationManager _nav;
    private readonly RutaRepositorio _repositorio;
    private readonly ServicioTrazos _trazos;

    public EstadoApp(NavigationManager nav, RutaRepositorio repositorio, ServicioTrazos trazos)
    {
        _nav = nav;
        _repositorio = repositorio;
        _trazos = trazos;
    }

    /// <summary>Los componentes se suscriben para volver a pintarse cuando algo del estado cambia.</summary>
    public event Action Cambio;
    public void Notificar() => Cambio?.Invoke();

    /// <summary>Se dispara al contraer o expandir el sidebar: los mapas cambian de ancho y deben recalcular su tamaño.</summary>
    public event Action BarraLateralCambio;
    public void NotificarBarraLateral() => BarraLateralCambio?.Invoke();

    // ── Sesión ────────────────────────────────────────────────────────────
    public bool HaIngresado { get; private set; }
    public string Nombre { get; set; } = "Laura Guerrero";
    public string Correo { get; set; } = "laura.g@ruta.com";
    public bool Invitado { get; set; }
    public UsuarioApi Usuario { get; set; }

    /// <summary>Pantalla a la que se va al entrar (las visitas como invitado desde la landing van directo a Rutas).</summary>
    public string DestinoTrasIngreso { get; set; } = "planificar";

    public void IniciarSesion(UsuarioApi usuario)
    {
        Usuario = usuario;
        Nombre = usuario.Name;
        Correo = usuario.Email;
        Invitado = false;
        HaIngresado = true;
    }

    public void EntrarComoInvitado()
    {
        Nombre = "Invitado";
        Correo = "Sin cuenta";
        Invitado = true;
        HaIngresado = true;
    }

    /// <summary>true cuando la animación de entrada (el camión que revela "RUTA") ya se vio en esta sesión.</summary>
    public bool IntroVista { get; set; }

    /// <summary>
    /// Después de iniciar sesión, registrarse o entrar como invitado: carga las rutas reales (si la API no responde se
    /// quedan las de ejemplo), calcula los recorridos por calles en segundo plano y abre la primera pantalla.
    /// </summary>
    public async Task CompletarIngresoAsync()
    {
        await CargarRutasRealesAsync();
        _ = PrepararTrazosAsync();
        string destino = DestinoTrasIngreso;
        DestinoTrasIngreso = "planificar";
        Navegar(destino);
    }

    public void CerrarSesion()
    {
        HaIngresado = false;
        Invitado = false;
        Usuario = null;
        DestinoTrasIngreso = "planificar";
        NuevaBusqueda();
        Origen = MiUbicacion;
        Notificar();
    }

    // ── Navegación ────────────────────────────────────────────────────────
    /// <summary>Parámetro de la última navegación (por ejemplo una Ruta para abrirla en el mapa). Se consume una vez.</summary>
    public object ParametroNavegacion { get; private set; }

    /// <summary>Claves: planificar, comparar, rutas, comunidad, perfil, config y ajuste (parametro = nombre del ajuste).</summary>
    public void Navegar(string clave, object parametro = null)
    {
        ParametroNavegacion = parametro;
        string url = clave switch
        {
            "config" => "configuracion",
            "ajuste" => "configuracion/ajuste?nombre=" + Uri.EscapeDataString(parametro as string ?? ""),
            _ => clave
        };
        _nav.NavigateTo("/" + url);
    }

    public T TomarParametro<T>() where T : class
    {
        var p = ParametroNavegacion as T;
        if (p != null) ParametroNavegacion = null;
        return p;
    }

    // ── Búsqueda actual (Planificar → Comparar) ───────────────────────────
    public const string MiUbicacion = "Mi ubicación actual";

    public string Origen { get; set; } = MiUbicacion;
    public string Destino { get; set; } = "";

    // Coordenadas de la búsqueda actual (las llena Planificar; NaN = desconocidas)
    public double OrigenLat { get; set; } = double.NaN;
    public double OrigenLng { get; set; }
    public double DestinoLat { get; set; } = double.NaN;
    public double DestinoLng { get; set; }

    public List<Sugerencia> Escalas { get; set; } = new();   // paradas intermedias de la búsqueda

    public bool HayCoordenadas => !double.IsNaN(OrigenLat) && !double.IsNaN(DestinoLat);

    /// <summary>Olvida la búsqueda actual (destino, paradas y coordenadas), por ejemplo al empezar el viaje.</summary>
    public void NuevaBusqueda()
    {
        Destino = "";
        Escalas = new List<Sugerencia>();
        OrigenLat = double.NaN;
        DestinoLat = double.NaN;
    }

    /// <summary>Última ubicación del equipo obtenida (null si aún no se pide o no se pudo).</summary>
    public double[] MiPosicion { get; set; }

    // ── Datos ─────────────────────────────────────────────────────────────
    public List<Ruta> Rutas { get; private set; } = DatosDemo.CrearRutas();   // ejemplo; se reemplaza con las de la base de datos si la API responde
    public List<Reciente> Recientes { get; } = new();
    public List<Trayecto> Trayectos { get; } = DatosDemo.CrearTrayectos();
    public List<Publicacion> Publicaciones { get; } = DatosDemo.CrearPublicaciones();
    public List<Publicacion> PublicacionesExtra { get; } = DatosDemo.CrearPublicacionesExtra();

    /// <summary>Historial de viajes del usuario (de ejemplo por ahora).</summary>
    public List<ViajeHistorial> Historial { get; } = DatosDemo.CrearHistorial();

    /// <summary>Ruta que se está viendo en el mapa de la pantalla Rutas (se conserva al cambiar de pantalla).</summary>
    public Ruta RutaSeleccionada { get; set; }

    private bool _rutasReales;

    /// <summary>Carga las rutas de la base de datos (vía API). Devuelve false si no se pudo.</summary>
    public async Task<bool> CargarRutasRealesAsync()
    {
        if (_rutasReales) return true;
        var reales = await _repositorio.CargarAsync();
        if (reales == null) return false;

        Rutas = reales;
        _rutasReales = true;
        _preparando = null;
        RutaSeleccionada = null;

        Recientes.Clear();
        foreach (var r in reales.Take(2))
            Recientes.Add(new Reciente { Ruta = r, Texto = "Disponible" });
        return true;
    }

    // ── Recorridos de las rutas ───────────────────────────────────────────
    private Task _preparando;

    /// <summary>Calcula el recorrido por calles de las rutas (si ya se está calculando, espera esa misma tarea).</summary>
    public Task PrepararTrazosAsync()
    {
        if (_preparando == null || _preparando.IsFaulted)
            _preparando = PrepararInterno();
        return _preparando;
    }

    private async Task PrepararInterno()
    {
        bool completo = await _trazos.PrepararAsync(Rutas);
        if (!completo) _preparando = null;   // sin internet: se reintenta la próxima vez
    }

    /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM).</summary>
    public Task<List<double[]>> TrazoOsrmAsync(string coordenadas) => _trazos.TrazoOsrmAsync(coordenadas);

    // ── Rutas guardadas ───────────────────────────────────────────────────
    public int TotalGuardadas => Rutas.Count(r => r.Guardada);

    public Ruta BuscarRuta(int numero) => Rutas.FirstOrDefault(r => r.Numero == numero);

    public void AlternarGuardada(Ruta ruta)
    {
        ruta.Guardada = !ruta.Guardada;
        Notificar();
    }

    /// <summary>Guarda el viaje del historial como trayecto (Perfil → Rutas Guardadas), o lo quita si ya estaba guardado.</summary>
    public void AlternarGuardadoHistorial(ViajeHistorial v)
    {
        if (v.Guardado != null)
        {
            Trayectos.Remove(v.Guardado);
            SincronizarGuardados();
            return;
        }
        var t = new Trayecto { Alias = v.Destino, Recorrido = v.Origen + " > " + v.Destino, Detalle = v.Minutos + " min" };
        Trayectos.Add(t);
        v.Guardado = t;
        Notificar();
    }

    // ── Favoritos de viaje y registro del historial ───────────────────────
    private readonly List<Ruta> _viajesFavoritos = new();

    /// <summary>
    /// Marca o desmarca como favorita una opción elegida en Comparar. Una ruta normal se guarda como siempre;
    /// una opción de viaje (de este origen a este destino) se guarda como trayecto en Perfil → Rutas Guardadas.
    /// </summary>
    public void AlternarFavoritoViaje(Ruta r)
    {
        if (!r.EsViaje) { AlternarGuardada(r); return; }

        if (r.TrayectoFavorito != null)
        {
            Trayectos.Remove(r.TrayectoFavorito);
            _viajesFavoritos.Remove(r);
            r.TrayectoFavorito = null;
            r.Guardada = false;
            Notificar();
            return;
        }

        var t = new Trayecto
        {
            Alias = Destino,
            Recorrido = Origen + " > " + Destino,
            Detalle = r.MinutosTexto + (string.IsNullOrWhiteSpace(r.Lineas) ? "" : " · " + r.Lineas)
        };
        Trayectos.Add(t);
        r.TrayectoFavorito = t;
        r.Guardada = true;
        _viajesFavoritos.Add(r);
        Notificar();
    }

    /// <summary>Quita un trayecto de Perfil y deja de verse como guardado en el historial y en Comparar.</summary>
    public void QuitarTrayecto(Trayecto t)
    {
        Trayectos.Remove(t);
        SincronizarGuardados();
    }

    // Si el usuario quita un trayecto desde Perfil, el viaje del historial deja de verse como guardado
    private void SincronizarGuardados()
    {
        foreach (var v in Historial)
            if (v.Guardado != null && !Trayectos.Contains(v.Guardado)) v.Guardado = null;
        foreach (var r in _viajesFavoritos.ToList())
            if (!Trayectos.Contains(r.TrayectoFavorito)) { r.TrayectoFavorito = null; r.Guardada = false; _viajesFavoritos.Remove(r); }
        Notificar();
    }

    /// <summary>
    /// Anota en el historial el viaje que acaba de empezar el usuario (con el origen, destino y camiones de la opción elegida).
    /// Si salió de "Mi ubicación actual", busca en segundo plano la calle donde estaba y la agrega al nombre.
    /// </summary>
    public ViajeHistorial RegistrarViaje(Ruta r)
    {
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
            Cuando = "Hoy, " + DateTime.Now.ToString("H:mm", CultureInfo.InvariantCulture),
            Minutos = r.Minutos,
            Camiones = camiones
        };
        Historial.Insert(0, v);
        while (Historial.Count > 30) Historial.RemoveAt(Historial.Count - 1);

        if (desdeUbicacion && !double.IsNaN(lat))
            _ = CompletarCalleAsync(v, lat, lng);
        return v;
    }

    private async Task CompletarCalleAsync(ViajeHistorial v, double lat, double lng)
    {
        string calle = await _trazos.CalleDeAsync(lat, lng);
        if (string.IsNullOrEmpty(calle)) return;
        v.Origen = "Ubicación actual (" + calle + ")";
        Notificar();
    }

    public static string Iniciales(string nombre)
    {
        var partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();
        return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
    }
}
