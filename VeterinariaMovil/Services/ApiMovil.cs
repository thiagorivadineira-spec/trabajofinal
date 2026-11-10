using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VeterinariaMovil.Models;

namespace VeterinariaMovil.Services
{
    public class ApiResultado<T>
    {
        public bool Ok { get; init; }
        public T? Datos { get; init; }
        public string? Error { get; init; }
    }

    /// <summary>
    /// Único lugar donde la app habla con la API. Agrega el token y traduce los errores
    /// a mensajes para mostrar. Si el token venció (401) cierra la sesión.
    /// </summary>
    public class ApiMovil
    {
        private readonly HttpClient _http;
        private readonly SesionMovil _sesion;

        public ApiMovil(HttpClient http, SesionMovil sesion)
        {
            _http = http;
            _sesion = sesion;
        }

        public Task<ApiResultado<AuthResponse>> LoginAsync(string username, string password) =>
            EnviarAsync<AuthResponse>(HttpMethod.Post, "api/Auth/Login",
                new LoginRequest { Username = username, Password = password }, conToken: false);

        public Task<ApiResultado<Perfil>> GetPerfilAsync() =>
            EnviarAsync<Perfil>(HttpMethod.Get, "api/Portal/perfil");

        public Task<ApiResultado<List<Mascota>>> GetMascotasAsync() =>
            EnviarAsync<List<Mascota>>(HttpMethod.Get, "api/Portal/mascotas");

        public Task<ApiResultado<FichaClinica>> GetHistorialAsync(int mascotaId) =>
            EnviarAsync<FichaClinica>(HttpMethod.Get, $"api/Portal/mascotas/{mascotaId}/historial");

        public Task<ApiResultado<List<Turno>>> GetTurnosAsync(bool incluirPasados) =>
            EnviarAsync<List<Turno>>(HttpMethod.Get, $"api/Portal/turnos?incluirPasados={(incluirPasados ? "true" : "false")}");

        public Task<ApiResultado<List<Receta>>> GetMedicamentosAsync() =>
            EnviarAsync<List<Receta>>(HttpMethod.Get, "api/Portal/medicamentos");

        public Task<ApiResultado<object>> CambiarPasswordAsync(string actual, string nueva) =>
            EnviarAsync<object>(HttpMethod.Post, "api/Auth/CambiarPassword",
                new CambiarPasswordRequest { PasswordActual = actual, PasswordNueva = nueva });

        private async Task<ApiResultado<T>> EnviarAsync<T>(HttpMethod metodo, string url, object? cuerpo = null, bool conToken = true)
        {
            try
            {
                using var peticion = new HttpRequestMessage(metodo, url);

                if (cuerpo != null)
                    peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType());

                if (conToken)
                {
                    if (_sesion.Actual == null)
                        return new ApiResultado<T> { Ok = false, Error = "Necesitás iniciar sesión." };

                    peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sesion.Actual.Token);
                }

                using var respuesta = await _http.SendAsync(peticion);

                if (respuesta.IsSuccessStatusCode)
                {
                    var texto = await respuesta.Content.ReadAsStringAsync();
                    var datos = string.IsNullOrWhiteSpace(texto)
                        ? default
                        : JsonSerializer.Deserialize<T>(texto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    return new ApiResultado<T> { Ok = true, Datos = datos };
                }

                if (respuesta.StatusCode == HttpStatusCode.Unauthorized && conToken)
                {
                    await _sesion.CerrarAsync();
                    return new ApiResultado<T> { Ok = false, Error = "Tu sesión expiró. Ingresá nuevamente." };
                }

                return new ApiResultado<T> { Ok = false, Error = await LeerErrorAsync(respuesta) };
            }
            catch (HttpRequestException)
            {
                return new ApiResultado<T> { Ok = false, Error = "Sin conexión con el servidor. Revisá tu internet." };
            }
            catch (TaskCanceledException)
            {
                return new ApiResultado<T> { Ok = false, Error = "El servidor tardó demasiado en responder." };
            }
            catch (JsonException)
            {
                return new ApiResultado<T> { Ok = false, Error = "Respuesta inesperada del servidor." };
            }
        }

        private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
        {
            try
            {
                var texto = await respuesta.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(texto);

                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("mensaje", out var mensaje) &&
                    mensaje.ValueKind == JsonValueKind.String)
                {
                    return mensaje.GetString()!;
                }
            }
            catch (Exception)
            {
                // se usa el mensaje genérico
            }

            return respuesta.StatusCode switch
            {
                HttpStatusCode.Forbidden => "Tu cuenta no tiene permiso para esto.",
                HttpStatusCode.NotFound => "No se encontró la información.",
                _ => $"Error del servidor ({(int)respuesta.StatusCode})."
            };
        }
    }
}
