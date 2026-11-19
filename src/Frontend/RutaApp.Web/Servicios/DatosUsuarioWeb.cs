using Microsoft.Extensions.Logging.Abstractions;
using RutaApp.Nucleo.Servicios;

namespace RutaApp.Web.Servicios;

// Historial de viajes y lugares guardados: el archivo por usuario de la app de escritorio, aquí en el navegador.
public class DatosUsuarioWeb : DatosUsuarioService
{
    private readonly AlmacenLocal _almacen;

    public DatosUsuarioWeb(AlmacenLocal almacen) : base(NullLogger<DatosUsuarioService>.Instance) => _almacen = almacen;

    private static string Clave(int idUsuario) => "ruta.datos." + idUsuario;

    public override async Task<DatosGuardados> CargarAsync(int idUsuario) =>
        await _almacen.LeerAsync<DatosGuardados>(Clave(idUsuario)) ?? new DatosGuardados();

    public override async Task GuardarAsync(int idUsuario, DatosGuardados datos)
    {
        if (await _almacen.EscribirAsync(Clave(idUsuario), datos)) return;

        // El navegador limita lo que se puede guardar (cada viaje lleva su recorrido): si no cupo, se quedan los viajes más recientes
        var recortados = new DatosGuardados { Historial = datos.Historial.Take(8).ToList(), Trayectos = datos.Trayectos, Lugares = datos.Lugares };
        await _almacen.EscribirAsync(Clave(idUsuario), recortados);
    }

    public override Task BorrarAsync(int idUsuario) => _almacen.BorrarAsync(Clave(idUsuario));
}
