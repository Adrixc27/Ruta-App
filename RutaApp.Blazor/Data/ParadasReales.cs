namespace RutaApp.Blazor.Data;

/// <summary>
/// Paradas de camión registradas en OpenStreetMap (highway=bus_stop) que quedan a menos de 60 m de alguna ruta oficial.
/// Datos © colaboradores de OpenStreetMap (licencia ODbL), descargados el 25 de octubre de 2026 con Overpass.
/// Junto con los puntos de control de los planos son las únicas paradas que usa el planificador.
/// </summary>
public static class ParadasReales
{
    public static readonly (string Nombre, double Lat, double Lng)[] Lista =
    {
        ("", 28.632975, -106.080961),
        ("Quinta Gameros", 28.630938, -106.073507),
        ("Independencia", 28.633615, -106.072782),
        ("Catedral", 28.636635, -106.076534),
        ("Niños Héroes", 28.639504, -106.077430),
        ("Trébol", 28.641282, -106.075498),
        ("Fuentes Danzarinas", 28.639445, -106.074522),
        ("Juárez", 28.637335, -106.076783),
        ("Justicia", 28.635450, -106.079851),
        ("Morelos", 28.633374, -106.076769),
        ("Parque Lerdo", 28.630769, -106.075135),
        ("IMSS", 28.643665, -106.077727),
        ("Deza y Ulloa", 28.646685, -106.080953),
        ("Sor Juana", 28.650015, -106.084536),
        ("División del Norte", 28.651611, -106.086292),
        ("Universidad", 28.654785, -106.089723),
        ("Arquitectura", 28.657333, -106.092458),
        ("Estacion ISSSTE Lazaro Cardenas", 28.660381, -106.095700),
        ("Fresno", 28.663992, -106.098013),
        ("Pino", 28.666839, -106.099345),
        ("Sicomoro", 28.670317, -106.100967),
        ("Zaragoza", 28.673224, -106.102311),
        ("Karike", 28.676813, -106.104024),
        ("Juan Escutia", 28.680773, -106.107351),
        ("La Villita", 28.686079, -106.110811),
        ("San José", 28.690128, -106.112336),
        ("CONALEP II", 28.692853, -106.115121),
        ("Centauro del Norte", 28.696989, -106.119089),
        ("Policía Federal", 28.698932, -106.120978),
        ("Santa Rita", 28.627889, -106.073696),
        ("Sagrado Corazón", 28.626221, -106.070808),
        ("Urquidi", 28.624876, -106.068447),
        ("Ochoa", 28.623038, -106.065227),
        ("Urueta", 28.621348, -106.062253),
        ("Zapata", 28.619356, -106.056960),
        ("Sexta", 28.620294, -106.054658),
        ("Diamante", 28.622646, -106.041372),
        ("Gemas", 28.621466, -106.034100),
        ("Nueva España", 28.620889, -106.030381),
        ("SCT", 28.620015, -106.025603),
        ("Terminal Sur", 28.624407, -106.028156),
        ("", 28.658962, -106.082072),
        ("", 28.703145, -106.124685),
        ("Estacion de Autobus", 28.638698, -106.072164),
        ("", 28.648013, -106.117476),
        ("", 28.651648, -106.119162),
        ("", 28.633108, -106.120921),
        ("", 28.638517, -106.122778),
        ("", 28.635529, -106.119154),
        ("", 28.652829, -106.084041),
        ("", 28.634872, -106.076751),
        ("", 28.633364, -106.078366),
        ("", 28.642915, -106.071221),
        ("Terminal Aldama", 28.641491, -106.076753),
        ("", 28.637736, -106.079136)
    };
}
