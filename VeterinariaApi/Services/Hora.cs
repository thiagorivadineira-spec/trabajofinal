namespace VeterinariaApi.Services
{
    /// <summary>
    /// Hora de Argentina (UTC-3, sin horario de verano).
    /// El servidor del hosting puede estar en otra zona horaria, por eso no se usa DateTime.Now.
    /// </summary>
    public static class Hora
    {
        public static DateTime Ahora => DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-3), DateTimeKind.Unspecified);

        public static DateTime Hoy => Ahora.Date;
    }
}
