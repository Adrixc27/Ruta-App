using Microsoft.AspNetCore.Components;

namespace RutaApp.Interfaz.Servicios;

// Estado compartido entre pantallas. Es un servicio "scoped": cada sesión tiene el suyo.
public partial class EstadoApp
{
    private readonly NavigationManager _nav;
    private readonly RutaRepositorio _repositorio;
    private readonly ServicioTrazos _trazos;
    private readonly FavoritoService _favoritos;
    private readonly DatosUsuarioService _datos;
    private readonly ComunidadService _comunidad;
    private readonly AjustesService _ajustes;
    private readonly UsuarioService _usuarios;
    private readonly IServicioUbicacion _ubicacion;

    public EstadoApp(NavigationManager nav, RutaRepositorio repositorio, ServicioTrazos trazos, FavoritoService favoritos,
                     DatosUsuarioService datos, ComunidadService comunidad, AjustesService ajustes, UsuarioService usuarios,
                     IServicioUbicacion ubicacion)
    {
        _ubicacion = ubicacion;
        _ajustes = ajustes;
        _usuarios = usuarios;
        _comunidad = comunidad;
        _nav = nav;
        _repositorio = repositorio;
        _trazos = trazos;
        _favoritos = favoritos;
        _datos = datos;
    }

    // Los componentes se suscriben para volver a pintarse cuando algo del estado cambia.
    public event Action Cambio;
    public void Notificar() => Cambio?.Invoke();

    // Se dispara al contraer o expandir el sidebar: los mapas cambian de ancho y deben recalcular su tamaño.
    public event Action BarraLateralCambio;
    public void NotificarBarraLateral() => BarraLateralCambio?.Invoke();

    // Preferencias de este equipo (unidades, accesibilidad).
    public AjustesService Ajustes => _ajustes;

    // Avisos breves ("Ruta guardada", "Lugar guardado"…)
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

    // Pantalla de carga
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
}
