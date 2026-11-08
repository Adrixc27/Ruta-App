using System.Globalization;

namespace RutaApp.Interfaz.Servicios;

// Publicaciones de la comunidad y avisos.
public partial class EstadoApp
{
    private bool _publicacionesCargadas;
    public bool ErrorPublicaciones { get; private set; }

    // Carga las publicaciones reales (las ve cualquiera, también un invitado). Con forzar vuelve a pedirlas.
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

    // Agrega al texto la fecha en que la publicación deja de verse (dias = 0: no caduca).
    public static string AgregarCaducidad(string cuerpo, int dias) =>
        dias <= 0 ? cuerpo : cuerpo + "\n[[vence:" + DateTime.Now.AddDays(dias).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) + "]]";

    // Problemas comunes para avisar (sirven al reportar desde el mapa y al publicar un aviso). Dias es cuánto sigue
    // siendo útil el aviso; Movilidad indica que el camión tiene un problema y el reporte lleva la ubicación de quien avisa.
    public record ProblemaComun(string Clave, string Icono, string Texto, int Dias, bool Movilidad = false);

    public static readonly ProblemaComun[] ProblemasComunes =
    {
        new("trafico", "IcoClock", "Tráfico pesado", 1, true),
        new("accidente", "IcoAlert", "Accidente", 1, true),
        new("ponchadura", "IcoBus", "Ponchadura de llanta", 1, true),
        new("tirado", "IcoXCircle", "Se quedó tirado", 1, true),
        new("roto", "IcoWrench", "Se rompió algo", 2, true),
        new("llena", "IcoUsers", "Unidad llena", 1),
        new("obras", "IcoShuffle", "Obras o desvío", 3),
        new("parada", "IcoPin", "Parada sin servicio", 3),
        new("otro", "IcoInfo", "Otro problema", 1)
    };

    // Cuánto dura una publicación lo decide la app: un aviso según su problema, una pregunta 7 días y una propuesta 30.
    public static int DiasDeVida(string tipo, string titulo = null)
    {
        if (tipo == "aviso")
        {
            var problema = ProblemasComunes.FirstOrDefault(p => p.Texto == titulo || (titulo != null && titulo.StartsWith(p.Texto + " en ")));
            return problema?.Dias ?? 3;
        }
        return tipo == "pregunta" ? 7 : 30;
    }

    // Ubicación actual del equipo para marcar un reporte en el mapa; null si no se pudo obtener.
    public async Task<double[]> UbicacionDeReporteAsync()
    {
        try
        {
            var pos = await _ubicacion.ObtenerAsync();
            if (pos != null) MiPosicion = pos;
            return pos;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static readonly System.Text.RegularExpressions.Regex RegexUbicacion = new(@"\s*\[\[ubic:(-?[0-9.]+),(-?[0-9.]+)\]\]\s*$");

    private static string AgregarUbicacion(string cuerpo, double[] ubicacion) =>
        ubicacion == null ? cuerpo
        : cuerpo + "\n[[ubic:" + ubicacion[0].ToString("0.00000", CultureInfo.InvariantCulture) + "," + ubicacion[1].ToString("0.00000", CultureInfo.InvariantCulture) + "]]";

    // Publica un aviso (reporte de problema) que caduca solo. Con ubicación, el aviso se marca en el mapa.
    // Devuelve (true, null) o (false, error).
    public async Task<(bool Ok, string Error)> PublicarAvisoAsync(string titulo, string detalle, Ruta ruta, int dias, double[] ubicacion = null)
    {
        if (!RequiereCuenta("Inicia sesión para reportar problemas y ayudar a otras personas a viajar mejor.")) return (false, null);
        try
        {
            int id = await _comunidad.CrearAsync(Usuario.Id, "aviso", titulo, AgregarCaducidad(AgregarUbicacion(detalle, ubicacion), dias), ruta?.Numero, null);
            if (id == 0) return (false, "No se pudo enviar el reporte.");

            var p = new Publicacion
            {
                Id = id, IdAutor = Usuario.Id, Tipo = "aviso", Autor = Nombre, Tiempo = "ahora", Fecha = DateTime.Now,
                Titulo = titulo, Cuerpo = detalle, RutaNumero = ruta?.Numero ?? 0,
                Vence = dias > 0 ? DateTime.Now.AddDays(dias) : null,
                Lat = ubicacion?[0], Lng = ubicacion?[1],
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

    // Agrega una publicación recién creada sin volver a pedir todas.
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
        // La ubicación de un reporte también viaja al final del texto: "[[ubic:28.63530,-106.08890]]"
        var ubic = RegexUbicacion.Match(p.Cuerpo ?? "");
        if (ubic.Success)
        {
            p.Lat = double.Parse(ubic.Groups[1].Value, CultureInfo.InvariantCulture);
            p.Lng = double.Parse(ubic.Groups[2].Value, CultureInfo.InvariantCulture);
            p.Cuerpo = p.Cuerpo.Substring(0, ubic.Index).TrimEnd();
        }
        if (d.IdRuta != null)
        {
            var ruta = BuscarRuta(d.IdRuta.Value);
            p.Etiquetas.Add(new Etiqueta { Texto = ruta?.Nombre ?? "Ruta " + d.IdRuta, EsRuta = true });
        }
        return p;
    }

    // Reportes con ubicación que todavía no caducan: son los que se marcan en el mapa.
    public List<Publicacion> ReportesEnMapa() =>
        Publicaciones.Where(p => p.Tipo == "aviso" && p.TieneUbicacion && (p.Vence == null || p.Vence > DateTime.Now)).ToList();

    // Avisos de la comunidad de los últimos días sobre una ruta (se muestran como alertas en el mapa).
    public List<Publicacion> AvisosDe(Ruta ruta, int dias = 7) =>
        ruta == null ? new List<Publicacion>() :
        Publicaciones.Where(p => p.Tipo == "aviso" && p.RutaNumero == ruta.Numero && (DateTime.Now - p.Fecha).TotalDays <= dias).ToList();
}
