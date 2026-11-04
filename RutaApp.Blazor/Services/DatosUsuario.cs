using System.Text.Json;

namespace RutaApp.Blazor.Services;

/// <summary>Lo que se guarda de cada usuario en este equipo: su historial de viajes, viajes guardados y lugares.</summary>
public class DatosGuardados
{
    public List<ViajeHistorial> Historial { get; set; } = new();
    public List<Trayecto> Trayectos { get; set; } = new();
    public List<LugarGuardado> Lugares { get; set; } = new();
}

/// <summary>
/// Guarda en un archivo por usuario (LocalApplicationData\RutaApp\usuarios\{id}.json) el historial de viajes,
/// los viajes guardados y los lugares (casa, escuela...). No toca la base de datos: así funciona sin cambiar su estructura.
/// </summary>
public class DatosUsuarioService
{
    private readonly string _carpeta;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private readonly ILogger<DatosUsuarioService> _log;

    public DatosUsuarioService(ILogger<DatosUsuarioService> log)
    {
        _log = log;
        _carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp", "usuarios");
    }

    private string Archivo(int idUsuario) => Path.Combine(_carpeta, idUsuario + ".json");

    public async Task<DatosGuardados> CargarAsync(int idUsuario)
    {
        await _cerrojo.WaitAsync();
        try
        {
            string ruta = Archivo(idUsuario);
            if (!File.Exists(ruta)) return new DatosGuardados();
            await using var flujo = File.OpenRead(ruta);
            return await JsonSerializer.DeserializeAsync<DatosGuardados>(flujo) ?? new DatosGuardados();
        }
        catch (Exception ex)
        {
            _log.LogWarning("No se pudieron leer los datos del usuario: {Mensaje}", ex.Message);
            return new DatosGuardados();
        }
        finally { _cerrojo.Release(); }
    }

    public async Task GuardarAsync(int idUsuario, DatosGuardados datos)
    {
        await _cerrojo.WaitAsync();
        try
        {
            Directory.CreateDirectory(_carpeta);
            string ruta = Archivo(idUsuario);
            string temporal = ruta + ".tmp";
            await using (var flujo = File.Create(temporal))
                await JsonSerializer.SerializeAsync(flujo, datos);
            File.Move(temporal, ruta, true);   // se escribe aparte y se reemplaza: un cierre a mitad no deja el archivo roto
        }
        catch (Exception ex)
        {
            _log.LogWarning("No se pudieron guardar los datos del usuario: {Mensaje}", ex.Message);
        }
        finally { _cerrojo.Release(); }
    }
}
