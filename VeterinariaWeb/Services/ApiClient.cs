using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using VeterinariaWeb.Models;

namespace VeterinariaWeb.Services
{
    /// <summary>Resultado de una llamada a la API: o trae datos, o trae un mensaje de error listo para mostrar.</summary>
    public class ApiResultado<T>
    {
        public bool Ok { get; init; }
        public T? Datos { get; init; }
        public string? Error { get; init; }

        public static ApiResultado<T> Exito(T? datos) => new() { Ok = true, Datos = datos };
        public static ApiResultado<T> Falla(string error) => new() { Ok = false, Error = error };
    }

    /// <summary>
    /// Único lugar donde el frontend habla con la API: arma las URL, agrega el token JWT,
    /// interpreta los errores y, si el token venció (401), cierra la sesión y vuelve al login.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly SesionService _sesion;
        private readonly JwtAuthStateProvider _auth;
        private readonly NavigationManager _nav;

        public ApiClient(HttpClient http, SesionService sesion, JwtAuthStateProvider auth, NavigationManager nav)
        {
            _http = http;
            _sesion = sesion;
            _auth = auth;
            _nav = nav;
        }

        // ---------------- Autenticación ----------------
        public Task<ApiResultado<AuthResponse>> LoginAsync(string username, string password) =>
            EnviarAsync<AuthResponse>(HttpMethod.Post, "api/Auth/Login",
                new LoginRequest { Username = username, Password = password }, requiereSesion: false);

        public Task<ApiResultado<bool>> CambiarPasswordAsync(string actual, string nueva) =>
            EnviarSinRespuestaAsync(HttpMethod.Post, "api/Auth/CambiarPassword",
                new CambiarPasswordRequest { PasswordActual = actual, PasswordNueva = nueva });

        // ---------------- Inicio ----------------
        public Task<ApiResultado<Resumen>> GetResumenAsync() =>
            EnviarAsync<Resumen>(HttpMethod.Get, "api/Resumen");

        // ---------------- Usuarios y personal (admin) ----------------
        public Task<ApiResultado<List<Usuario>>> GetUsuariosAsync() =>
            EnviarAsync<List<Usuario>>(HttpMethod.Get, "api/Usuarios");

        public Task<ApiResultado<Usuario>> CrearUsuarioAsync(UsuarioNuevo usuario) =>
            EnviarAsync<Usuario>(HttpMethod.Post, "api/Usuarios", usuario);

        public Task<ApiResultado<bool>> CambiarEstadoUsuarioAsync(int id, bool activo) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Usuarios/{id}/activo" + Query(("activo", Bool(activo))));

        public Task<ApiResultado<bool>> RestablecerPasswordAsync(int id, string password) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Usuarios/{id}/password", new { Password = password });

        public Task<ApiResultado<List<Especialidad>>> GetEspecialidadesAsync(bool incluirInactivas = false) =>
            EnviarAsync<List<Especialidad>>(HttpMethod.Get, "api/Especialidades" + Query(("incluirInactivas", Bool(incluirInactivas))));

        public Task<ApiResultado<IdRespuesta>> CrearEspecialidadAsync(EspecialidadRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Especialidades", dto);

        public Task<ApiResultado<bool>> EditarEspecialidadAsync(int id, EspecialidadRequest dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Especialidades/{id}", dto);

        public Task<ApiResultado<bool>> CambiarEstadoEspecialidadAsync(int id, bool activo) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Especialidades/{id}/activo" + Query(("activo", Bool(activo))));

        public Task<ApiResultado<List<Veterinario>>> GetVeterinariosAsync(int? especialidadId = null) =>
            EnviarAsync<List<Veterinario>>(HttpMethod.Get, "api/Veterinarios" + Query(("especialidadId", especialidadId?.ToString())));

        // ---------------- Dueños (recepción) ----------------
        public Task<ApiResultado<Paginado<Dueno>>> GetDuenosAsync(string? buscar, int pagina = 1, int tamano = 10) =>
            EnviarAsync<Paginado<Dueno>>(HttpMethod.Get, "api/Duenos" + Query(
                ("pagina", pagina.ToString()), ("tamanoPagina", tamano.ToString()), ("buscar", buscar)));

        public Task<ApiResultado<Dueno>> GetDuenoAsync(int id) =>
            EnviarAsync<Dueno>(HttpMethod.Get, $"api/Duenos/{id}");

        public Task<ApiResultado<IdRespuesta>> CrearDuenoAsync(DuenoRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Duenos", dto);

        public Task<ApiResultado<bool>> EditarDuenoAsync(int id, DuenoRequest dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Duenos/{id}", dto);

        public Task<ApiResultado<MensajeRespuesta>> CrearCuentaDuenoAsync(int id, CuentaRequest dto) =>
            EnviarAsync<MensajeRespuesta>(HttpMethod.Post, $"api/Duenos/{id}/cuenta", dto);

        // ---------------- Mascotas ----------------
        public Task<ApiResultado<Paginado<Mascota>>> GetMascotasAsync(string? buscar, int pagina = 1, int tamano = 10, int? duenoId = null) =>
            EnviarAsync<Paginado<Mascota>>(HttpMethod.Get, "api/Mascotas" + Query(
                ("pagina", pagina.ToString()), ("tamanoPagina", tamano.ToString()),
                ("buscar", buscar), ("duenoId", duenoId?.ToString())));

        public Task<ApiResultado<Mascota>> GetMascotaAsync(int id) =>
            EnviarAsync<Mascota>(HttpMethod.Get, $"api/Mascotas/{id}");

        public Task<ApiResultado<IdRespuesta>> CrearMascotaAsync(MascotaRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Mascotas", dto);

        public Task<ApiResultado<bool>> EditarMascotaAsync(int id, MascotaRequest dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Mascotas/{id}", dto);

        // ---------------- Turnos ----------------
        public Task<ApiResultado<List<Turno>>> GetTurnosAsync(DateTime desde, DateTime hasta, int? veterinarioId, string? estado) =>
            EnviarAsync<List<Turno>>(HttpMethod.Get, "api/Turnos" + Query(
                ("desde", Fecha(desde)), ("hasta", Fecha(hasta)),
                ("veterinarioId", veterinarioId?.ToString()), ("estado", estado)));

        public Task<ApiResultado<List<Turno>>> GetAgendaAsync(DateTime fecha) =>
            EnviarAsync<List<Turno>>(HttpMethod.Get, "api/Turnos/agenda" + Query(("fecha", Fecha(fecha))));

        public Task<ApiResultado<IdRespuesta>> CrearTurnoAsync(TurnoRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Turnos", dto);

        public Task<ApiResultado<bool>> ReprogramarTurnoAsync(int id, ReprogramarRequest dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Turnos/{id}/reprogramar", dto);

        public Task<ApiResultado<bool>> CancelarTurnoAsync(int id) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Turnos/{id}/cancelar");

        // ---------------- Ficha clínica y atención (veterinario) ----------------
        public Task<ApiResultado<FichaClinica>> GetFichaAsync(int mascotaId) =>
            EnviarAsync<FichaClinica>(HttpMethod.Get, $"api/Atenciones/ficha/{mascotaId}");

        public Task<ApiResultado<IdRespuesta>> RegistrarAtencionAsync(AtencionRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Atenciones", dto);

        // ---------------- Medicamentos, distribuidores y compras (admin) ----------------
        public Task<ApiResultado<List<Medicamento>>> GetMedicamentosAsync(string? buscar = null, bool soloReponer = false, bool incluirInactivos = false) =>
            EnviarAsync<List<Medicamento>>(HttpMethod.Get, "api/Medicamentos" + Query(
                ("buscar", buscar), ("soloReponer", Bool(soloReponer)), ("incluirInactivos", Bool(incluirInactivos))));

        public Task<ApiResultado<IdRespuesta>> CrearMedicamentoAsync(MedicamentoRequest dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Medicamentos", dto);

        public Task<ApiResultado<bool>> EditarMedicamentoAsync(int id, MedicamentoRequest dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Medicamentos/{id}", dto);

        public Task<ApiResultado<bool>> CambiarEstadoMedicamentoAsync(int id, bool activo) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Medicamentos/{id}/activo" + Query(("activo", Bool(activo))));

        public Task<ApiResultado<List<Distribuidor>>> GetDistribuidoresAsync(bool incluirInactivos = false) =>
            EnviarAsync<List<Distribuidor>>(HttpMethod.Get, "api/Distribuidores" + Query(("incluirInactivos", Bool(incluirInactivos))));

        public Task<ApiResultado<IdRespuesta>> CrearDistribuidorAsync(Distribuidor dto) =>
            EnviarAsync<IdRespuesta>(HttpMethod.Post, "api/Distribuidores", dto);

        public Task<ApiResultado<bool>> EditarDistribuidorAsync(int id, Distribuidor dto) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Distribuidores/{id}", dto);

        public Task<ApiResultado<bool>> CambiarEstadoDistribuidorAsync(int id, bool activo) =>
            EnviarSinRespuestaAsync(HttpMethod.Put, $"api/Distribuidores/{id}/activo" + Query(("activo", Bool(activo))));

        public Task<ApiResultado<CompraCreada>> CrearCompraAsync(CompraRequest pedido) =>
            EnviarAsync<CompraCreada>(HttpMethod.Post, "api/Compras", pedido);

        public Task<ApiResultado<Paginado<Compra>>> GetComprasAsync(int pagina = 1, int tamano = 10) =>
            EnviarAsync<Paginado<Compra>>(HttpMethod.Get, "api/Compras" + Query(
                ("pagina", pagina.ToString()), ("tamanoPagina", tamano.ToString()), ("ordenarPor", "fecha_desc")));

        // ---------------- Auditoría ----------------
        public Task<ApiResultado<Paginado<RegistroAuditoria>>> GetAuditoriaAsync(
            int pagina, int tamano, string? accion, DateTime? desde, DateTime? hasta, string? buscar) =>
            EnviarAsync<Paginado<RegistroAuditoria>>(HttpMethod.Get, "api/Auditoria" + Query(
                ("pagina", pagina.ToString()),
                ("tamanoPagina", tamano.ToString()),
                ("accion", accion),
                ("desde", Fecha(desde)),
                ("hasta", Fecha(hasta)),
                ("buscar", buscar)));

        // ---------------- Reportes ----------------
        public Task<ApiResultado<byte[]>> DescargarReporteAsync(string tipo, DateTime? desde, DateTime? hasta) =>
            EjecutarAsync<byte[]>(HttpMethod.Get,
                $"api/Reportes/{tipo}" + Query(("desde", Fecha(desde)), ("hasta", Fecha(hasta))),
                null, true,
                async contenido => await contenido.ReadAsByteArrayAsync());

        public Task<ApiResultado<MensajeRespuesta>> EnviarReporteAsync(
            string tipo, string email, DateTime? desde, DateTime? hasta) =>
            EnviarAsync<MensajeRespuesta>(HttpMethod.Post, "api/Reportes/enviar",
                new EnviarReporteRequest { Tipo = tipo, Email = email, Desde = desde, Hasta = hasta });

        // ---------------- Utilidades ----------------
        private static string? Fecha(DateTime? fecha) => fecha?.ToString("yyyy-MM-dd");

        private static string Bool(bool valor) => valor ? "true" : "false";

        private static string Query(params (string clave, string? valor)[] parametros) =>
            "?" + string.Join("&", parametros
                .Where(p => !string.IsNullOrWhiteSpace(p.valor))
                .Select(p => $"{p.clave}={Uri.EscapeDataString(p.valor!)}"));

        // ---------------- Núcleo de las llamadas ----------------
        private Task<ApiResultado<T>> EnviarAsync<T>(
            HttpMethod metodo, string url, object? cuerpo = null, bool requiereSesion = true) =>
            EjecutarAsync<T>(metodo, url, cuerpo, requiereSesion,
                async contenido => await contenido.ReadFromJsonAsync<T>());

        private async Task<ApiResultado<bool>> EnviarSinRespuestaAsync(
            HttpMethod metodo, string url, object? cuerpo = null)
        {
            var r = await EjecutarAsync<bool>(metodo, url, cuerpo, true, _ => Task.FromResult(true));
            return r.Ok ? ApiResultado<bool>.Exito(true) : ApiResultado<bool>.Falla(r.Error ?? "Error desconocido.");
        }

        private async Task<ApiResultado<T>> EjecutarAsync<T>(
            HttpMethod metodo,
            string url,
            object? cuerpo,
            bool requiereSesion,
            Func<HttpContent, Task<T?>> leerContenido)
        {
            try
            {
                using var peticion = new HttpRequestMessage(metodo, url);

                if (cuerpo != null)
                {
                    peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType());
                }

                if (requiereSesion)
                {
                    var sesion = await _sesion.ObtenerAsync();

                    if (sesion == null)
                    {
                        await _auth.CerrarSesionAsync();
                        _nav.NavigateTo("login");
                        return ApiResultado<T>.Falla("Necesitás iniciar sesión.");
                    }

                    peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);
                }

                using var respuesta = await _http.SendAsync(peticion);

                if (respuesta.IsSuccessStatusCode)
                {
                    if (respuesta.StatusCode == HttpStatusCode.NoContent)
                    {
                        return ApiResultado<T>.Exito(default);
                    }

                    return ApiResultado<T>.Exito(await leerContenido(respuesta.Content));
                }

                if (respuesta.StatusCode == HttpStatusCode.Unauthorized && requiereSesion)
                {
                    await _auth.CerrarSesionAsync();
                    _nav.NavigateTo("login");
                    return ApiResultado<T>.Falla("Tu sesión expiró. Iniciá sesión nuevamente.");
                }

                return ApiResultado<T>.Falla(await LeerErrorAsync(respuesta));
            }
            catch (HttpRequestException)
            {
                return ApiResultado<T>.Falla("No se pudo conectar con el servidor. Verificá que la API esté en ejecución.");
            }
            catch (JsonException)
            {
                return ApiResultado<T>.Falla("La respuesta del servidor no tiene el formato esperado.");
            }
        }

        /// <summary>Extrae un mensaje legible de las distintas formas de error que devuelve la API.</summary>
        private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
        {
            var texto = string.Empty;

            try
            {
                texto = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                // se usa el mensaje genérico de más abajo
            }

            if (!string.IsNullOrWhiteSpace(texto))
            {
                try
                {
                    using var documento = JsonDocument.Parse(texto);
                    var raiz = documento.RootElement;

                    if (raiz.ValueKind == JsonValueKind.Object)
                    {
                        // Errores propios de la API: { "mensaje": "..." }
                        if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                        {
                            return mensaje.GetString()!;
                        }

                        // Errores de validación: { "errors": { "Campo": ["..."] } }
                        if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                        {
                            var lista = new List<string>();

                            foreach (var campo in errores.EnumerateObject())
                            {
                                if (campo.Value.ValueKind != JsonValueKind.Array) continue;

                                foreach (var item in campo.Value.EnumerateArray())
                                {
                                    if (item.ValueKind == JsonValueKind.String) lista.Add(item.GetString()!);
                                }
                            }

                            if (lista.Count > 0) return string.Join(" ", lista);
                        }

                        if (raiz.TryGetProperty("title", out var titulo) && titulo.ValueKind == JsonValueKind.String)
                        {
                            return titulo.GetString()!;
                        }
                    }
                    else if (raiz.ValueKind == JsonValueKind.String)
                    {
                        return raiz.GetString()!;
                    }
                }
                catch (JsonException)
                {
                    if (texto.Length < 200) return texto;
                }
            }

            return respuesta.StatusCode switch
            {
                HttpStatusCode.Forbidden => "No tenés permiso para realizar esta acción.",
                HttpStatusCode.NotFound => "No se encontró el recurso solicitado.",
                _ => $"Error del servidor ({(int)respuesta.StatusCode})."
            };
        }
    }
}
