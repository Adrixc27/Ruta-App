using System.Globalization;

namespace RutaApp.Nucleo.Modelos;

public class Ruta
{
    public int Numero { get; set; }
    public string Nombre { get; set; } = "";
    public string Modo { get; set; } = "Autobús";        // para los filtros: Autobús / Exprés / Metro
    public string ModoTexto { get; set; } = "Autobús";   
    public int FrecuenciaMin { get; set; } = 10;
    public decimal Tarifa { get; set; } = 2.75m;
    public bool Desvio { get; set; }
    public string Horario { get; set; } = "5:30 - 23:00";
    public int Paradas { get; set; } = 20;
    public int Minutos { get; set; } = 20;
    public int Transbordos { get; set; }
    public string Descripcion { get; set; } = "";         // texto de la tabla routes
    public string Lineas { get; set; } = "";             // chip de la pantalla Comparar
    public string Etiqueta { get; set; } = "";           // Recomendado / Más rápido / Económico
    public Trayecto TrayectoFavorito { get; set; }       // si es una opción de viaje marcada como guardada (aparece en Perfil)
    public bool DatosReales { get; set; }                // true = Minutos/Tarifa vienen de la base de datos
    public bool Reparado { get; set; }                   // true = ya se corrigieron los tramos del recorrido que cortaban esquinas
    public bool TrazoEnCalles { get; set; } = true;      // false = el recorrido une las paradas en línea recta y hay que ajustarlo a las calles

    // Color propio de la ruta (lista, tarjeta y línea en el mapa), tomado de la paleta según su número.
    public string Color => Paleta.DeRuta(Numero);
    public string Simbolo => Paleta.Simbolo(Numero);     // identifica la ruta sin depender del color
    public string Patron => Paleta.Patron(Numero);       // trazo de la ruta en el mapa (modo daltonismo)

    public string OrigenNombre { get; set; } = "";
    public string DestinoNombre { get; set; } = "";
    public double LatO { get; set; }
    public double LngO { get; set; }
    public double LatD { get; set; }
    public double LngD { get; set; }

    // Textos ya formateados para la interfaz
    public string NumeroTexto => Numero.ToString();
    public string TarifaTexto => "$" + Tarifa.ToString("0.00", CultureInfo.InvariantCulture);
    public string MinutosTexto => Minutos + " min";
    public string Meta => ModoTexto + " · " + MinutosTexto + " · " + TarifaTexto;
    public string Subtitulo => string.IsNullOrEmpty(Descripcion) ? ModoTexto + " · " + Horario : Descripcion;
    public string FrecuenciaTexto =>
        string.IsNullOrEmpty(Resumen) ? "Cada " + FrecuenciaMin + " min desde parada central" : Resumen;

    // Cuando la ruta es una opción de viaje (camina → camión → camina) hacia un destino concreto
    public bool EsViaje { get; set; }
    public string Resumen { get; set; } = "";        // "Camina 4 min · Bus 12 min · Camina 3 min"
    public int CaminataMetros { get; set; }
    public List<double[]> Trazo { get; set; }        // recorrido de la ruta [lat, lng] en orden
    public List<Tramo> Tramos { get; set; }          // solo en opciones de viaje: caminar / bus / caminar...
    public List<Sugerencia> Escalas { get; set; }    // paradas intermedias del viaje (A → B → C)

    // Variantes (ramales) de la ruta, por ejemplo Komatsu Inverso / Directo. Con más de una, el mapa deja elegir.
    // Vienen de la base de datos (RutaService): cada variante es una fila de routes con el mismo "grupo".
    public List<RutaOficial> Variantes { get; set; }
    public int VarianteActual { get; set; }
    public bool TieneVariantes => Variantes != null && Variantes.Count > 1;
    public RutaOficial Oficial => Variantes != null && Variantes.Count > 0 ? Variantes[VarianteActual] : null;

    // Cambia a la variante indicada: recorrido, paradas, origen y destino.
    public void UsarVariante(int indice)
    {
        if (Variantes == null || indice < 0 || indice >= Variantes.Count) return;
        VarianteActual = indice;
        var o = Variantes[indice];

        Trazo = o.Trazo.Select(p => new[] { p[0], p[1] }).ToList();
        TrazoEnCalles = o.TrazoEnCalles;
        Reparado = o.Reparado;
        Paradas = o.Paradas.Count;

        if (o.Paradas.Count > 0)
        {
            OrigenNombre = o.Paradas[0].Nombre;
            DestinoNombre = o.Paradas[o.Paradas.Count - 1].Nombre;
            LatO = o.Paradas[0].Latitud; LngO = o.Paradas[0].Longitud;
            LatD = o.Paradas[o.Paradas.Count - 1].Latitud;
            LngD = o.Paradas[o.Paradas.Count - 1].Longitud;
        }
        else if (Trazo.Count > 0)
        {
            LatO = Trazo[0][0]; LngO = Trazo[0][1];
            LatD = Trazo[Trazo.Count - 1][0]; LngD = Trazo[Trazo.Count - 1][1];
        }
    }

    // Instrucciones del viaje paso a paso (camina, toma tal ruta...), armadas con sus tramos.
    public List<PasoViaje> Pasos()
    {
        var lista = new List<PasoViaje>();
        if (Tramos == null) return lista;
        for (int i = 0; i < Tramos.Count; i++)
        {
            var t = Tramos[i];
            if (t.Puntos == null || t.Puntos.Count == 0) continue;
            var ini = t.Puntos[0];
            bool ultimo = i == Tramos.Count - 1;
            if (!t.EsBus)
            {
                // A dónde se camina: la parada donde se sube (o el destino) y cuántos metros son
                string hacia = ultimo ? "tu destino"
                    : (i + 1 < Tramos.Count && !string.IsNullOrWhiteSpace(Tramos[i + 1].ParadaInicio) ? Tramos[i + 1].ParadaInicio : "la parada");
                string distancia = t.Metros > 0 ? Formato.Distancia(t.Metros) + " · " : "";
                lista.Add(new PasoViaje("IcoWalk", "Camina " + Math.Max(1, t.Minutos) + " min",
                    distancia + "hasta " + (ultimo || hacia == "la parada" ? hacia : "la parada " + hacia), false, "#475569", ini[0], ini[1],
                    t.Instrucciones?.Select(x => Formato.ConvertirTexto(x == "Llegas al punto" ? (ultimo ? "Llegas a tu destino" : "Llegas a la parada") : x)).ToList()));
            }
            else
            {
                string sube = string.IsNullOrWhiteSpace(t.ParadaInicio) ? "" : "Sube en " + t.ParadaInicio + " · ";
                string baja = string.IsNullOrWhiteSpace(t.ParadaFin) ? "" : " · baja en " + t.ParadaFin;
                string pasa = t.EsperaMin > 0 ? " · pasa cada " + t.EsperaMin + " min" : "";
                lista.Add(new PasoViaje("IcoBus", "Toma " + (string.IsNullOrWhiteSpace(t.Linea) ? "el camión" : t.Linea),
                    sube + Math.Max(1, t.Minutos) + " min a bordo" + baja + pasa, true, t.Color ?? "#2563EB", ini[0], ini[1], null, t.Simbolo));
            }
        }
        return lista;
    }

    // Largo del recorrido completo (ida y vuelta) en kilómetros.
    public double Kilometros
    {
        get
        {
            if (Trazo == null || Trazo.Count < 2) return 0;
            double m = 0, k = Math.Cos(Trazo[0][0] * Math.PI / 180);
            for (int i = 1; i < Trazo.Count; i++)
            {
                double dx = (Trazo[i][1] - Trazo[i - 1][1]) * 111320 * k, dy = (Trazo[i][0] - Trazo[i - 1][0]) * 110574;
                m += Math.Sqrt(dx * dx + dy * dy);
            }
            return m / 1000;
        }
    }

    public string EtiquetaMayus => Etiqueta.ToUpperInvariant();
    public string TransbordosTexto =>
        Transbordos == 0 ? "Sin transbordos" : Transbordos == 1 ? "1 transbordo" : Transbordos + " transbordos";

    public bool Guardada { get; set; }
    public bool Seleccionada { get; set; }
}
