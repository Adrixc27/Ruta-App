namespace RutaApp.Nucleo.Modelos;

public class Etiqueta
{
    public string Texto { get; set; } = "";
    public bool EsRuta { get; set; }
    public string Icono => EsRuta ? "IcoBus" : "IcoPin";
}

public class Publicacion
{
    public string Tipo { get; set; } = "aviso";   // aviso | pregunta | propuesta
    public string Autor { get; set; } = "";
    public bool Oficial { get; set; }
    public string Tiempo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Cuerpo { get; set; } = "";
    public List<Etiqueta> Etiquetas { get; set; } = new();
    public int Comentarios { get; set; }
    public int Orden { get; set; }        // mayor = más reciente
    public int RutaNumero { get; set; }   // 0 = no está ligada a una ruta
    public int Likes { get; set; }
    public bool MeGusta { get; set; }

    public string Inicial => Autor.Length > 0 ? Autor.Substring(0, 1).ToUpperInvariant() : "?";
    public string TipoTexto => Tipo.ToUpperInvariant();
    public string IconoTipo => Tipo == "aviso" ? "IcoAlert" : (Tipo == "pregunta" ? "IcoHelp" : "IcoBulb");
    public string Cta => Tipo == "pregunta" ? "Responder" : "Ver en el mapa";
    public DateTime Fecha { get; set; } = DateTime.Now;
    public DateTime? Vence { get; set; }  // null = no caduca

    /// <summary>Dónde estaba quien reportó un problema de movilidad (null si el reporte no lleva ubicación).</summary>
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public bool TieneUbicacion => Lat != null && Lng != null;

    public string VenceTexto
    {
        get
        {
            if (Vence == null) return "";
            var t = Vence.Value - DateTime.Now;
            if (t.TotalHours < 1) return "Vence en menos de 1 h";
            if (t.TotalHours < 24) return "Vence en " + (int)t.TotalHours + " h";
            return "Vence en " + (int)Math.Ceiling(t.TotalDays) + " d";
        }
    }
    public int Id { get; set; }        // id_post en la base de datos 
    public int IdAutor { get; set; }   // id del usuario que publicó 
}
