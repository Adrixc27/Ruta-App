using System.Text.Json;

namespace RutaApp.Nucleo.Servicios;

// Preferencias de este equipo: unidades, accesibilidad y la sesión recordada.
public class AjustesData
{
    public string Unidades { get; set; } = "km";          // km | mi
    public string Idioma { get; set; } = "es";            // es | en
    public bool Daltonismo { get; set; }                  // colores neutros y símbolos en lugar de colores
    public bool GuiaVisual { get; set; }                  // navegación con teclado, "dónde estoy" y botones de ayuda (?)
    public bool SinAnimaciones { get; set; }
    public int? SesionUsuarioId { get; set; }             // usuario que pidió "Recordarme"
}

// Guarda y avisa los cambios de los ajustes (LocalApplicationData\RutaApp\ajustes.json). Un solo juego por equipo.
public class AjustesService
{
    private readonly string _archivo;

    public AjustesData Datos { get; private set; } = new();

    // Se dispara al guardar: la interfaz vuelve a aplicar la accesibilidad y las unidades.
    public event Action Cambio;

    public AjustesService()
    {
        _archivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp", "ajustes.json");
        try
        {
            if (File.Exists(_archivo))
                Datos = JsonSerializer.Deserialize<AjustesData>(File.ReadAllText(_archivo)) ?? new AjustesData();
        }
        catch (Exception)
        {
            Datos = new AjustesData();
        }
        AplicarFormato();
    }

    private void AplicarFormato()
    {
        Formato.UsaMillas = Datos.Unidades == "mi";
        Formato.Ingles = Datos.Idioma == "en";
    }

    public void Guardar()
    {
        AplicarFormato();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_archivo));
            File.WriteAllText(_archivo, JsonSerializer.Serialize(Datos));
        }
        catch (Exception)
        {
            // si no se puede escribir, los ajustes valen solo mientras la app está abierta
        }
        Cambio?.Invoke();
    }
}
