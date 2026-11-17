namespace RutaApp.Interfaz.Estilos;

// Tokens de diseño: la única fuente de colores, tipografía, espaciado, bordes, sombras y movimiento.
// Toda la hoja los usa como var(--nombre); un valor nuevo se agrega aquí antes de usarlo en una regla.
public static class Tokens
{
    private static readonly (string Nombre, string Valor)[] Variables =
    {
        // ── Color: marca y texto
        ("brand-primary", "#2563EB"),
        ("brand-tint", "#DBEAFE"),
        ("text-primary", "#1B365D"),
        ("text-secondary", "#4A6FA5"),
        ("text-on-brand", "#FFFFFF"),        // texto sobre la marca o sobre un color lleno

        // ── Color: bordes y fondos
        ("border-default", "#B0CDEA"),
        ("border-soft", "#C8DCEF"),
        ("bg-sidebar", "#D0E3F5"),
        ("bg-page", "#C4DBF0"),
        ("bg-panel", "#E6F0FA"),
        ("bg-card", "#F4F8FD"),
        ("bg-white", "#FFFFFF"),

        // ── Color: estados y acentos
        ("warning", "#B45309"),
        ("warning-tint", "#FEF3C7"),
        ("success", "#16A34A"),
        ("success-tint", "#DCFCE7"),
        ("danger", "#DC2626"),
        ("danger-tint", "#FEE2E2"),
        ("accent-reference", "#5EEAD4"),
        ("accent-origin", "#F9A8A8"),
        ("accent-stop", "#FACC15"),
        ("star", "#FBBF24"),

        // ── Color: escala de azules y degradados de marca
        ("blue-50", "#EFF6FF"),
        ("blue-100", "#DBEAFE"),
        ("blue-200", "#BFDBFE"),
        ("blue-300", "#93C5FD"),
        ("blue-400", "#60A5FA"),
        ("blue-500", "#3B82F6"),
        ("blue-600", "#2563EB"),
        ("blue-700", "#1D4ED8"),
        ("blue-800", "#1E40AF"),
        ("bg-panel-2", "#DCEAF8"),
        ("bg-sidebar-2", "#BFD7F0"),
        ("grad-marca", "linear-gradient(135deg, var(--blue-500) 0%, var(--blue-600) 55%, var(--blue-700) 100%)"),
        ("grad-suave", "linear-gradient(180deg, var(--blue-50) 0%, var(--blue-100) 100%)"),

        // ── Color: acentos por sección
        ("violet", "#7C3AED"),
        ("violet-tint", "#EDE9FE"),
        ("teal", "#0D9488"),
        ("teal-tint", "#CCFBF1"),
        ("rose", "#E11D48"),
        ("rose-tint", "#FFE4E6"),
        ("amber", "#D97706"),
        ("amber-tint", "#FEF3C7"),
        ("orange", "#EA580C"),
        ("orange-tint", "#FFEDD5"),
        ("emerald", "#16A34A"),
        ("emerald-tint", "#DCFCE7"),
        ("slate", "#475569"),

        // ── Tipografía: familias, escala de tamaños, pesos e interlineado
        ("fuente", "'Source Sans 3', 'Source Sans Pro', 'Segoe UI', sans-serif"),
        ("fuente-mono", "Consolas, 'Courier New', monospace"),
        ("texto-xs", "11px"),     // notas, contadores, etiquetas mínimas
        ("texto-sm", "12px"),     // texto de apoyo
        ("texto-base", "13px"),   // controles compactos y datos
        ("texto-md", "14px"),     // cuerpo
        ("texto-lg", "15px"),     // botones y texto destacado
        ("texto-xl", "18px"),     // títulos de tarjeta
        ("texto-2xl", "22px"),    // títulos de modal y sección
        ("texto-3xl", "26px"),    // títulos de pantalla
        ("peso-normal", "400"),
        ("peso-medio", "600"),
        ("peso-fuerte", "700"),
        ("peso-titulo", "900"),
        ("linea-compacta", "1.25"),
        ("linea-texto", "1.45"),

        // ── Espaciado: escala de 4 px (rellenos, márgenes y separaciones)
        ("espacio-1", "4px"),
        ("espacio-2", "8px"),
        ("espacio-3", "12px"),
        ("espacio-4", "16px"),
        ("espacio-5", "20px"),
        ("espacio-6", "24px"),
        ("espacio-8", "32px"),
        ("espacio-10", "40px"),

        // ── Bordes: radios y trazos
        ("radio-sm", "8px"),      // chips, etiquetas y botones de icono
        ("radio-md", "12px"),     // botones y campos
        ("radio-lg", "16px"),     // tarjetas
        ("radio-xl", "20px"),     // modales y paneles grandes
        ("radio-pill", "999px"),
        ("radio-circulo", "50%"),
        ("borde", "1px solid var(--border-default)"),
        ("borde-suave", "1px solid var(--border-soft)"),
        ("borde-marca", "1px solid var(--brand-primary)"),
        ("borde-ancho", "2px"),

        // ── Sombras: tres niveles de elevación, la de marca y el anillo de foco
        ("overlay", "rgba(27, 54, 93, .4)"),
        ("sombra", "0 3px 18px rgba(27, 54, 93, .08)"),
        ("sombra-card", "0 2px 6px rgba(27, 54, 93, .05), 0 10px 24px rgba(37, 99, 235, .08)"),   // 1: tarjetas en reposo
        ("sombra-elevada", "0 8px 22px rgba(37, 99, 235, .16)"),                                    // 2: menús, sugerencias, elementos flotantes
        ("sombra-modal", "0 10px 40px rgba(27, 54, 93, .28)"),                                      // 3: modales, avisos y globos de ayuda
        ("sombra-azul", "0 6px 16px rgba(37, 99, 235, .28)"),
        ("sombra-azul-hover", "0 8px 20px rgba(37, 99, 235, .36)"),
        ("sombra-azul-activa", "0 3px 8px rgba(37, 99, 235, .30)"),
        ("sombra-marcador", "0 2px 6px rgba(0, 0, 0, .3)"),
        ("anillo-foco", "0 0 0 3px rgba(96, 165, 250, .35)"),
        ("anillo-error", "0 0 0 3px rgba(220, 38, 38, .18)"),

        // ── Movimiento
        ("dur-rapida", ".12s"),
        ("dur", ".18s"),
        ("dur-lenta", ".3s"),
        ("curva", "cubic-bezier(.2, .8, .2, 1)"),

        // ── Estados de interacción
        ("hover-bg", "var(--blue-100)"),
        ("activo-bg", "var(--blue-200)"),
        ("opacidad-deshabilitado", ".5"),

        // ── Medidas de controles y capas
        ("alto-control", "46px"),
        ("alto-control-chico", "38px"),
        ("capa-modal", "2000"),
        ("capa-aviso", "3500"),
        ("capa-carga", "4000"),
    };

    public static string Raiz() =>
        ":root {\n" + string.Join("\n", Variables.Select(v => $"    --{v.Nombre}: {v.Valor};")) + "\n}";
}
