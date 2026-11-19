namespace RutaApp.Interfaz.Servicios;

// Interruptor de la muestra web (GitHub Pages). Con Activo = true la interfaz esconde lo que necesita servidor o cuenta
// (comunidad, perfil, configuración, inicio de sesión) y en su lugar dice "Disponible próximamente".
// Quien entra a la muestra no inicia sesión: usa un usuario local cuyos datos quedan solo en su navegador.
// La app de escritorio lo deja apagado.
public class ModoMuestra
{
    public bool Activo { get; init; }

    public Usuario NuevoUsuarioLocal() => new() { Id = 1, Nombre = "Visitante", Correo = "Muestra web" };
}
