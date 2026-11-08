using System.Text.Json.Serialization;

namespace RutaApp.Nucleo.Modelos;

// Un lugar que el usuario guarda para planificar (Casa, Escuela, Trabajo u otro punto de referencia).
public class LugarGuardado
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "otro";    // casa | escuela | trabajo | otro
    public string Direccion { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }

    [JsonIgnore]
    public string Icono => Tipo switch { "casa" => "IcoHome", "escuela" => "IcoSchool", "trabajo" => "IcoBriefcase", _ => "IcoFlag" };
    [JsonIgnore]
    public string TipoTexto => Tipo switch { "casa" => "Casa", "escuela" => "Escuela", "trabajo" => "Trabajo", _ => "Punto de referencia" };
}
