using System.Text.Json;
using RutaApp.Datos;

// Copia las rutas y paradas de la base de datos MySQL a un JSON estático para la muestra web (GitHub Pages).
// Uso: dotnet run --project tools/ExportarRutas -- <archivo de salida>
// Usa la misma conexión que la app (conexion.json o RUTA_DB_CONNECTION). No exporta usuarios ni nada de la comunidad.
string salida = args.Length > 0 ? args[0] : "rutas.json";

var rutas = await new RutaService().ObtenerRutasAsync();
var conTrazo = rutas.Where(r => r.Trazo.Count >= 2).ToList();

var opciones = new JsonSerializerOptions { WriteIndented = false };
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(salida))!);
await File.WriteAllTextAsync(salida, JsonSerializer.Serialize(conTrazo, opciones));

Console.WriteLine($"{conTrazo.Count} rutas con recorrido (de {rutas.Count}), " +
                  $"{conTrazo.Sum(r => r.Paradas.Count)} paradas, {conTrazo.Sum(r => r.Trazo.Count)} puntos de trazo.");
Console.WriteLine($"Escrito en {Path.GetFullPath(salida)} ({new FileInfo(salida).Length / 1024} KB)");
