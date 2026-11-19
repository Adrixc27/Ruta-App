using RutaApp.Datos;

namespace RutaApp.Web.Servicios;

// Rutas guardadas (el marcador en Rutas): se quedan en el navegador en lugar de la tabla favorite_route.
public class FavoritoServiceWeb : FavoritoService
{
    private readonly AlmacenLocal _almacen;

    public FavoritoServiceWeb(AlmacenLocal almacen) => _almacen = almacen;

    private static string Clave(int idUser) => "ruta.favoritas." + idUser;

    public override async Task<List<int>> ObtenerRutasAsync(int idUser) =>
        await _almacen.LeerAsync<List<int>>(Clave(idUser)) ?? new();

    public override async Task AgregarAsync(int idUser, int idRoutes)
    {
        var ids = await ObtenerRutasAsync(idUser);
        if (ids.Contains(idRoutes)) return;
        ids.Add(idRoutes);
        await _almacen.EscribirAsync(Clave(idUser), ids);
    }

    public override async Task QuitarAsync(int idUser, int idRoutes)
    {
        var ids = await ObtenerRutasAsync(idUser);
        if (ids.Remove(idRoutes)) await _almacen.EscribirAsync(Clave(idUser), ids);
    }
}
