using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RutaApp.Datos;
using RutaApp.Interfaz.Components;
using RutaApp.Interfaz.Servicios;
using RutaApp.Nucleo.Servicios;
using RutaApp.Web.Servicios;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Modo muestra: sin cuentas, comunidad, perfil ni configuración ("Disponible próximamente")
builder.Services.AddSingleton(new ModoMuestra { Activo = true });
builder.Services.AgregarServiciosRuta();

// En el navegador no hay MySQL, archivos ni Windows: cada servicio de la app de escritorio se cambia por su versión web
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<AlmacenLocal>();
builder.Services.Replace(ServiceDescriptor.Singleton<RutaService, RutaServiceWeb>());
builder.Services.Replace(ServiceDescriptor.Singleton<FavoritoService, FavoritoServiceWeb>());
builder.Services.Replace(ServiceDescriptor.Singleton<DatosUsuarioService, DatosUsuarioWeb>());
builder.Services.Replace(ServiceDescriptor.Singleton<IServicioUbicacion, UbicacionNavegador>());
builder.Services.AddSingleton<IPortapapeles, PortapapelesNavegador>();

await builder.Build().RunAsync();
