using System.Text.Json;
using Microsoft.Maui.Storage;
using VeterinariaMovil.Models;

namespace VeterinariaMovil.Services
{
    /// <summary>
    /// Guarda la sesión del dueño (token JWT + datos) en el SecureStorage del celular
    /// (almacenamiento cifrado de Android). Si el token venció, la sesión se descarta.
    /// </summary>
    public class SesionMovil
    {
        private const string Clave = "veterinaria.sesion";

        public AuthResponse? Actual { get; private set; }

        /// <summary>Avisa a la pantalla principal cuando se inicia o se cierra sesión.</summary>
        public event Action? Cambio;

        public async Task CargarAsync()
        {
            try
            {
                var json = await SecureStorage.Default.GetAsync(Clave);
                Actual = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<AuthResponse>(json);
            }
            catch (Exception)
            {
                Actual = null;
            }

            if (Actual != null && Actual.Expira.ToUniversalTime() <= DateTime.UtcNow)
            {
                await CerrarAsync();
            }
        }

        public async Task IniciarAsync(AuthResponse datos)
        {
            Actual = datos;
            await SecureStorage.Default.SetAsync(Clave, JsonSerializer.Serialize(datos));
            Cambio?.Invoke();
        }

        public Task CerrarAsync()
        {
            Actual = null;
            SecureStorage.Default.Remove(Clave);
            Cambio?.Invoke();
            return Task.CompletedTask;
        }
    }
}
