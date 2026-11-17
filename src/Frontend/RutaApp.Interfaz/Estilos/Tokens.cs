namespace RutaApp.Interfaz.Estilos;

// Variables de diseño (colores, degradados y sombras): se usan en toda la hoja como var(--nombre).
public static class Tokens
{
    private static readonly (string Nombre, string Valor)[] Variables =
    {
        // Marca y texto
        ("brand-primary", "#2563EB"),
        ("brand-tint", "#DBEAFE"),
        ("text-primary", "#1B365D"),
        ("text-secondary", "#4A6FA5"),

        // Bordes y fondos
        ("border-default", "#B0CDEA"),
        ("border-soft", "#C8DCEF"),
        ("bg-sidebar", "#D0E3F5"),
        ("bg-page", "#C4DBF0"),
        ("bg-panel", "#E6F0FA"),
        ("bg-card", "#F4F8FD"),
        ("bg-white", "#FFFFFF"),

        // Estados y acentos
        ("warning", "#B45309"),
        ("warning-tint", "#FEF3C7"),
        ("success", "#16A34A"),
        ("success-tint", "#DCFCE7"),
        ("danger", "#DC2626"),
        ("accent-reference", "#5EEAD4"),
        ("accent-origin", "#F9A8A8"),
        ("accent-stop", "#FACC15"),
        ("star", "#FBBF24"),

        // Capas y sombras
        ("overlay", "rgba(27, 54, 93, .4)"),
        ("sombra", "0 3px 18px rgba(27, 54, 93, .08)"),

        // Escala de azules, degradados y sombras de marca
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
        ("sombra-azul", "0 6px 16px rgba(37, 99, 235, .28)"),
        ("sombra-card", "0 2px 6px rgba(27, 54, 93, .05), 0 10px 24px rgba(37, 99, 235, .08)"),

        // Acentos por sección
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
    };

    public static string Raiz() =>
        ":root {\n" + string.Join("\n", Variables.Select(v => $"    --{v.Nombre}: {v.Valor};")) + "\n}";
}
