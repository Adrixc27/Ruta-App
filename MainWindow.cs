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
        Width = 1440;
        Height = 900;
        MinWidth = 1100;
        MinHeight = 700;
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
