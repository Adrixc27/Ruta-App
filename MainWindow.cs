using System.Windows;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using RutaApp.Blazor.Components;

namespace Ruta_App;

/// Ventana principal: solo contiene un BlazorWebView que muestra las pantallas de RutaApp.Blazor
public class MainWindow : Window
{
    public MainWindow(IServiceProvider servicios)
    {
        Title = "R.U.T.A.";
        // El tamaño se adapta a la pantalla: nunca más grande que el área de trabajo (cabe en laptops de 1366x768)
        var area = SystemParameters.WorkArea;
        Width = Math.Min(1440, area.Width * 0.96);
        Height = Math.Min(900, area.Height * 0.96);
        MinWidth = Math.Min(380, area.Width);
        MinHeight = Math.Min(560, area.Height);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var web = new BlazorWebView
        {
            HostPage = @"wwwroot\index.html",
            Services = servicios
        };
        web.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Routes) });
        web.RootComponents.Add(new RootComponent { Selector = "head::after", ComponentType = typeof(HeadOutlet) });
        Content = web;
    }
}
