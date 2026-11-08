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
    private readonly FavoritoService _favoritos;
    private readonly DatosUsuarioService _datos;
    private readonly ComunidadService _comunidad;
    private readonly AjustesService _ajustes;
    private readonly UsuarioService _usuarios;

    public EstadoApp(NavigationManager nav, RutaRepositorio repositorio, ServicioTrazos trazos, FavoritoService favoritos,
                     DatosUsuarioService datos, ComunidadService comunidad, AjustesService ajustes, UsuarioService usuarios)
    {
        _ajustes = ajustes;
        _usuarios = usuarios;
        _comunidad = comunidad;
        _nav = nav;
        _repositorio = repositorio;
        _trazos = trazos;
        _favoritos = favoritos;
        _datos = datos;
    }

    /// <summary>Los componentes se suscriben para volver a pintarse cuando algo del estado cambia.</summary>
    public event Action Cambio;
    public void Notificar() => Cambio?.Invoke();

    /// <summary>Se dispara al contraer o expandir el sidebar: los mapas cambian de ancho y deben recalcular su tamaño.</summary>
    public event Action BarraLateralCambio;
    public void NotificarBarraLateral() => BarraLateralCambio?.Invoke();

    /// <summary>Preferencias de este equipo (unidades, accesibilidad).</summary>
    public AjustesService Ajustes => _ajustes;

    // ── Avisos breves ("Ruta guardada", "Lugar guardado"…) ────────────────
    public string Aviso { get; private set; }
    private int _versionAviso;

    public async void MostrarAviso(string texto)
    {
        Aviso = texto;
        int version = ++_versionAviso;
        Notificar();
        await Task.Delay(2600);
        if (version != _versionAviso) return;
        Aviso = null;
        Notificar();
    }

    // ── Buscar por paradas ────────────────────────────────────────────────
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

    // ── Pantalla de carga ─────────────────────────────────────────────────
    public bool Cargando { get; private set; }
    public string MensajeCarga { get; private set; } = "";

    public void MostrarCarga(string mensaje)
    {
        Cargando = true;
        MensajeCarga = mensaje;
        Notificar();
    }

    public void OcultarCarga()
    {
        if (!Cargando) return;
        Cargando = false;
        Notificar();
    }

    // ── Sesión ────────────────────────────────────────────────────────────
    public bool HaIngresado { get; private set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public bool Invitado { get; set; }
    public Usuario Usuario { get; set; }

    /// <summary>true cuando hay una cuenta con sesión iniciada (los invitados no guardan nada).</summary>
    public bool ConCuenta => !Invitado && Usuario != null && Usuario.Id > 0;

    /// <summary>Pantalla a la que se va al entrar (las visitas desde la landing van directo a Planificar).</summary>
    public string DestinoTrasIngreso { get; set; } = "planificar";

    public void IniciarSesion(Usuario usuario)
    {
        Usuario = usuario;
        Nombre = usuario.Nombre;
        Correo = usuario.Correo;
        Invitado = false;
        HaIngresado = true;
    }

    public void EntrarComoInvitado()
    {
        Usuario = null;
        Nombre = "Invitado";
        Correo = "Sin cuenta";
        Invitado = true;
        HaIngresado = true;
    }

    /// <summary>Se dispara cuando se pide ver otra vez la animación de entrada estando ya en la landing.</summary>
    public event Action IntroSolicitada;

    /// <summary>Lleva a la landing page. Con animación (al tocar el logo) se vuelve a reproducir la animación de entrada.</summary>
    public void IrALanding(bool conAnimacion = false)
    {
        if (conAnimacion)
        {
            IntroVista = false;
            IntroSolicitada?.Invoke();
        }
        _nav.NavigateTo("/");
    }

    /// <summary>true cuando la animación de entrada (el camión que revela "RUTA") ya se vio en esta sesión.</summary>
    public bool IntroVista { get; set; }

    /// <summary>
    /// Después de iniciar sesión, registrarse o entrar como invitado: carga las rutas reales de la base de datos,
    /// los datos del usuario (historial, viajes y lugares guardados) y abre la primera pantalla.
    /// </summary>
    public async Task CompletarIngresoAsync()
    {
        await PrepararCuentaAsync(ConCuenta ? "Preparando tu cuenta…" : "Cargando rutas…");
        string destino = DestinoTrasIngreso;
        DestinoTrasIngreso = "planificar";
        Navegar(destino);
    }

    private async Task PrepararCuentaAsync(string mensaje)
    {
        MostrarCarga(mensaje);
        try
        {
            await CargarRutasRealesAsync();
            await CargarFavoritosAsync();
            await CargarDatosUsuarioAsync();
        }
        finally
        {
            OcultarCarga();
        }
    }

    // ── "Recordarme": la sesión sigue abierta aunque cierres y abras la app ──
    private bool _restauracionIntentada;

    /// <summary>Guarda (o quita) el recordatorio de la sesión de este usuario.</summary>
    public void RecordarSesion(bool recordar)
    {
        if (!ConCuenta) return;
        _ajustes.Datos.SesionUsuarioId = recordar ? Usuario.Id : null;
        _ajustes.Guardar();
    }

    /// <summary>Si el usuario pidió "Recordarme", vuelve a abrir su sesión al iniciar la app. Devuelve true si hay sesión.</summary>
    public async Task<bool> RestaurarSesionAsync()
    {
        if (HaIngresado) return ConCuenta;
        if (_restauracionIntentada) return false;
        _restauracionIntentada = true;

        int? id = _ajustes.Datos.SesionUsuarioId;
        if (id == null) return false;
        try
        {
            var u = await _usuarios.ObtenerAsync(id.Value);
            if (u == null)
            {
                _ajustes.Datos.SesionUsuarioId = null;
                _ajustes.Guardar();
                return false;
            }
            Sesion.Actual = u;
            IniciarSesion(u);
            await PrepararCuentaAsync("Abriendo tu sesión…");
            Notificar();
            return true;
        }
        catch (Exception)
        {
            return false;   // sin conexión: se queda sin sesión y puede iniciarla a mano
        }
    }

    private async Task CargarFavoritosAsync()
    {
        if (!ConCuenta) return;
        try
        {
            var ids = await _favoritos.ObtenerRutasAsync(Usuario.Id);
            foreach (var r in Rutas) r.Guardada = ids.Contains(r.Numero);
        }
        catch (Exception)
        {
            // sin conexión: las rutas quedan sin marcar
        }
    }

    // ── Cerrar sesión (siempre con confirmación) ──────────────────────────
    public bool ConfirmandoCierre { get; private set; }

    public void PedirCierreSesion()
    {
        if (!ConCuenta) { IrAIniciarSesion(); return; }
        ConfirmandoCierre = true;
        Notificar();
    }

    public void CancelarCierreSesion()
    {
        ConfirmandoCierre = false;
        Notificar();
    }

    public void ConfirmarCierreSesion()
    {
        ConfirmandoCierre = false;
        CerrarSesion();
        _nav.NavigateTo("/");
    }

    // ── Borrar la cuenta ──────────────────────────────────────────────────
    /// <summary>Borra la cuenta y todo lo suyo (base de datos y datos de este equipo). Devuelve el error, o null si salió bien.</summary>
    public async Task<string> BorrarCuentaAsync()
    {
        if (!ConCuenta) return "No hay una cuenta con sesión iniciada.";
        int id = Usuario.Id;
        try
        {
            if (!await _usuarios.EliminarAsync(id)) return "No se pudo borrar la cuenta. Inténtalo de nuevo.";
        }
        catch (Exception)
        {
            return "No se pudo conectar con la base de datos para borrar la cuenta. Revisa tu conexión.";
        }
        await _datos.BorrarAsync(id);
        _publicacionesCargadas = false;
        CerrarSesion();
        _nav.NavigateTo("/");
        MostrarAviso("Tu cuenta se borró");
        return null;
    }

    public void CerrarSesion()
    {
        Sesion.Actual = null;
        if (_ajustes.Datos.SesionUsuarioId != null)
        {
            _ajustes.Datos.SesionUsuarioId = null;
            _ajustes.Guardar();
        }
        HaIngresado = false;
        Invitado = false;
        Usuario = null;
        foreach (var r in Rutas) r.Guardada = false;
        Historial.Clear();
        Trayectos.Clear();
        Lugares.Clear();
        DestinoTrasIngreso = "planificar";
        NuevaBusqueda();
        Origen = MiUbicacion;
        Notificar();
    }

    // ── Aviso "inicia sesión para guardar" ────────────────────────────────
    /// <summary>Mensaje que pide iniciar sesión (lo muestra el diseño de la app). null = no hay aviso.</summary>
    public string AvisoSesion { get; private set; }

    /// <summary>Guardar rutas, viajes y lugares es solo para quien tiene cuenta. Devuelve true si se puede continuar.</summary>
    public bool RequiereCuenta(string motivo)
    {
        if (ConCuenta) return true;
        AvisoSesion = motivo;
        Notificar();
        return false;
    }

    public void CerrarAvisoSesion()
    {
        AvisoSesion = null;
        Notificar();
    }

    public void IrAIniciarSesion()
    {
        AvisoSesion = null;
        _nav.NavigateTo("/acceso");
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

    /// <summary>Vuelve a leer todo de la base de datos (después de cambiar la conexión en Configuración).</summary>
    public async Task RecargarDatosAsync()
    {
        _rutasReales = false;
        _publicacionesCargadas = false;
        _repositorio.LimpiarCache();
        MostrarCarga("Conectando con la base de datos…");
        try
        {
            await CargarRutasRealesAsync();
            await CargarFavoritosAsync();
        }
        finally
        {
            OcultarCarga();
        }
        Notificar();
    }

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

    // ── Comunidad (publicaciones de la base de datos) ─────────────────────
    private bool _publicacionesCargadas;
    public bool ErrorPublicaciones { get; private set; }

    /// <summary>Carga las publicaciones reales (las ve cualquiera, también un invitado). Con forzar vuelve a pedirlas.</summary>
    public async Task CargarPublicacionesAsync(bool forzar = false)
    {
        if (_publicacionesCargadas && !forzar) return;
        try
        {
            var posts = await _comunidad.ObtenerPublicacionesAsync(ConCuenta ? Usuario.Id : 0);
            Publicaciones.Clear();
            int orden = posts.Count;
            foreach (var d in posts)
            {
                var pub = ConvertirPublicacion(d, orden--);
                if (pub.Vence != null && pub.Vence < DateTime.Now) continue;   // ya caducó: no se muestra
                Publicaciones.Add(pub);
            }
            _publicacionesCargadas = true;
            ErrorPublicaciones = false;
        }
        catch (Exception)
        {
            ErrorPublicaciones = true;
        }
        Notificar();
    }

    private static readonly System.Text.RegularExpressions.Regex RegexVence = new(@"\s*\[\[vence:([0-9T:\-]+)\]\]\s*$");

    /// <summary>Agrega al texto la fecha en que la publicación deja de verse (dias = 0: no caduca).</summary>
    public static string AgregarCaducidad(string cuerpo, int dias) =>
        dias <= 0 ? cuerpo : cuerpo + "\n[[vence:" + DateTime.Now.AddDays(dias).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) + "]]";

    /// <summary>Problemas comunes para avisar (sirven al reportar desde el mapa y al publicar un aviso).</summary>
    public record ProblemaComun(string Clave, string Icono, string Texto, int Dias);

    public static readonly ProblemaComun[] ProblemasComunes =
    {
        new("trafico", "IcoClock", "Tráfico pesado", 1),
        new("accidente", "IcoAlert", "Accidente", 1),
        new("ponchadura", "IcoBus", "Ponchadura de llanta", 1),
        new("tirado", "IcoXCircle", "Se quedó tirado", 1),
        new("roto", "IcoWrench", "Se rompió algo", 2),
        new("llena", "IcoUsers", "Unidad llena", 1),
        new("obras", "IcoShuffle", "Obras o desvío", 3),
        new("parada", "IcoPin", "Parada sin servicio", 3),
        new("otro", "IcoInfo", "Otro problema", 1)
    };

    /// <summary>Publica un aviso (reporte de problema) que caduca solo. Devuelve (true, null) o (false, error).</summary>
    public async Task<(bool Ok, string Error)> PublicarAvisoAsync(string titulo, string detalle, Ruta ruta, int dias)
    {
        if (!RequiereCuenta("Inicia sesión para reportar problemas y ayudar a otras personas a viajar mejor.")) return (false, null);
        try
        {
            int id = await _comunidad.CrearAsync(Usuario.Id, "aviso", titulo, AgregarCaducidad(detalle, dias), ruta?.Numero, null);
            if (id == 0) return (false, "No se pudo enviar el reporte.");

            var p = new Publicacion
            {
                Id = id, IdAutor = Usuario.Id, Tipo = "aviso", Autor = Nombre, Tiempo = "ahora", Fecha = DateTime.Now,
                Titulo = titulo, Cuerpo = detalle, RutaNumero = ruta?.Numero ?? 0,
                Vence = dias > 0 ? DateTime.Now.AddDays(dias) : null,
                Orden = Publicaciones.Count > 0 ? Publicaciones.Max(x => x.Orden) + 1 : 1
            };
            if (ruta != null) p.Etiquetas.Add(new Etiqueta { Texto = ruta.Nombre, EsRuta = true });
            AgregarPublicacion(p);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, "No se pudo conectar con la base de datos. Intenta de nuevo.");
        }
    }

    /// <summary>Agrega una publicación recién creada sin volver a pedir todas.</summary>
    public void AgregarPublicacion(Publicacion p)
    {
        Publicaciones.Add(p);
        Notificar();
    }

    private Publicacion ConvertirPublicacion(PostDb d, int orden)
    {
        var p = new Publicacion
        {
            Id = d.Id,
            IdAutor = d.IdUsuario,
            Tipo = d.Tipo,
            Autor = d.Autor,
            Fecha = d.Fecha,
            Tiempo = Formato.Hace(d.Fecha),
            Titulo = d.Titulo ?? "",
            Cuerpo = d.Contenido,
            Comentarios = d.Comentarios,
            Likes = d.Likes,
            MeGusta = d.MeGusta > 0,
            Orden = orden,
            RutaNumero = d.IdRuta ?? 0
        };
        // La fecha de caducidad viaja al final del texto: "[[vence:2026-11-20T18:00]]"
        var vence = RegexVence.Match(p.Cuerpo ?? "");
        if (vence.Success)
        {
            if (DateTime.TryParse(vence.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaVence)) p.Vence = fechaVence;
            p.Cuerpo = p.Cuerpo.Substring(0, vence.Index).TrimEnd();
        }
        if (d.IdRuta != null)
        {
            var ruta = BuscarRuta(d.IdRuta.Value);
            p.Etiquetas.Add(new Etiqueta { Texto = ruta?.Nombre ?? "Ruta " + d.IdRuta, EsRuta = true });
        }
        return p;
    }

    /// <summary>Avisos de la comunidad de los últimos días sobre una ruta (se muestran como alertas en el mapa).</summary>
    public List<Publicacion> AvisosDe(Ruta ruta, int dias = 7) =>
        ruta == null ? new List<Publicacion>() :
        Publicaciones.Where(p => p.Tipo == "aviso" && p.RutaNumero == ruta.Numero && (DateTime.Now - p.Fecha).TotalDays <= dias).ToList();

    // ── Datos propios del usuario (historial, viajes y lugares guardados) ──
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
        SincronizarGuardados();
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

    // ── Recorridos de las rutas ───────────────────────────────────────────
    /// <summary>Recorrido por calles entre puntos "lng,lat;lng,lat;..." (OSRM).</summary>
    public Task<List<double[]>> TrazoOsrmAsync(string coordenadas) => _trazos.TrazoOsrmAsync(coordenadas);

    // ── Rutas guardadas (individuales) ────────────────────────────────────
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

    // ── Viajes guardados (personalizados) ─────────────────────────────────
    /// <summary>Guarda el viaje del historial, o lo quita de los guardados si ya lo estaba.</summary>
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
        Trayectos.Add(t);
        v.GuardadoId = t.Id;
        Notificar();
        MostrarAviso("Viaje guardado en tu perfil");
        GuardarDatos();
    }

    private readonly List<Ruta> _viajesGuardados = new();

    /// <summary>
    /// Guarda o quita una opción elegida en Comparar. Una ruta normal se guarda como siempre;
    /// una opción de viaje (de este origen a este destino) se guarda como viaje personalizado en el perfil.
    /// </summary>
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

        var t = TrayectoDeOpcion(r);
        Trayectos.Add(t);
        r.TrayectoFavorito = t;
        r.Guardada = true;
        _viajesGuardados.Add(r);
        Notificar();
        MostrarAviso("Viaje guardado en tu perfil");
        GuardarDatos();
    }

    /// <summary>El viaje de una opción de Comparar como Trayecto (para guardarlo o compartirlo), sin guardarlo todavía.</summary>
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

    /// <summary>Quita un viaje guardado del perfil y deja de verse como guardado en el historial y en Comparar.</summary>
    public void QuitarTrayecto(Trayecto t)
    {
        Trayectos.Remove(t);
        SincronizarGuardados();
        GuardarDatos();
    }

    // Si el usuario quita un viaje desde Perfil, deja de verse como guardado en el historial y en Comparar
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

    // ── Viajes en el mapa ─────────────────────────────────────────────────
    /// <summary>Arma una Ruta de tipo viaje (para dibujarla en el mapa) a partir de un viaje del historial o guardado.</summary>
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

    // ── Historial de viajes ───────────────────────────────────────────────
    /// <summary>
    /// Anota en el historial el viaje que acaba de empezar el usuario (con el origen, destino y camiones de la opción elegida).
    /// Si salió de "Mi ubicación actual", busca en segundo plano la calle donde estaba y la agrega al nombre.
    /// Los invitados no tienen historial. Devuelve null si no se anotó.
    /// </summary>
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

    // ── Lugares guardados (casa, escuela, referencias) ────────────────────
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

    // ── Importar lo que compartió otra persona ────────────────────────────
    /// <summary>Lee un código (RUTA1:...) y lo agrega al perfil. Devuelve el mensaje para mostrar.</summary>
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
                Trayectos.Add(c.Viaje);
                Notificar(); GuardarDatos();
                return "Se guardó el viaje \"" + c.Viaje.Alias + "\" en tus rutas guardadas.";

            case "ruta":
                var ruta = BuscarRuta(c.RutaId);
                if (ruta == null) return "Esa ruta no está disponible en este momento.";
                if (!ruta.Guardada) AlternarGuardada(ruta);
                return "Se guardó la ruta \"" + ruta.Nombre + "\".";
        }
        return "Ese código no se pudo leer.";
    }

    /// <summary>Busca direcciones y lugares por nombre (para guardar un lugar escribiendo su dirección).</summary>
    public Task<List<Sugerencia>> BuscarLugaresAsync(string texto) => _trazos.BuscarLugaresAsync(texto);

    /// <summary>Nombre de la calle de un punto del mapa (para ponerle nombre a un lugar elegido tocando el mapa).</summary>
    public Task<string> CalleDeAsync(double lat, double lng) => _trazos.CalleDeAsync(lat, lng);

    public static string Iniciales(string nombre)
    {
        var partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();
        return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
    }
}
