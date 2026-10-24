using System.IO;
using System.Windows;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RutaApp.Blazor.Services;

namespace Ruta_App;

/// Punto de entrada de la app de escritorio. No hay XAML: la ventana se arma en C# y la interfaz es Blazor
public static class Program
{
    // API en la web (registro, login, camiones y rutas). Para usar la local: http://localhost:5023/ con el proyecto MiApi corriendo.
    private const string UrlApi = "http://ruta-api.runasp.net/";

    [STAThread]
    public static void Main()
    {
        // WebView2 necesita una carpeta de datos escribible (por defecto usa la del .exe, que puede fallar)
        string datos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RutaApp", "WebView2");
        Directory.CreateDirectory(datos);
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", datos);

        var servicios = new ServiceCollection();
        servicios.AddWpfBlazorWebView();
        servicios.AddLogging(l => l.AddDebug());
        servicios.AgregarServiciosRuta(UrlApi);                                     // servicios de la app (están en RutaApp.Blazor)
        servicios.AddSingleton<IServicioUbicacion, UbicacionWindows>();            // ubicación del equipo (con respaldo por IP)
#if DEBUG
        servicios.AddBlazorWebViewDeveloperTools();
#endif

        var proveedor = servicios.BuildServiceProvider();

        var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
        app.Run(new MainWindow(proveedor));
    }
}
