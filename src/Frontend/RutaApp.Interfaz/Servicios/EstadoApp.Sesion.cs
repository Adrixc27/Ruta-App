namespace RutaApp.Interfaz.Servicios;

// Sesión: inicio, "Recordarme", cierre, borrado de cuenta y aviso de "inicia sesión".
public partial class EstadoApp
{
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
    /// Tras iniciar sesión, registrarse o entrar como invitado: carga las rutas, los datos del usuario y abre la primera pantalla.
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

    // "Recordarme": la sesión sigue abierta aunque cierres y abras la app
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

    // Cerrar sesión (siempre con confirmación)
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

    // Borrar la cuenta
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

    // Aviso "inicia sesión para guardar"
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
}
