namespace VeterinariaApi.Seguridad
{
    /// <summary>
    /// Roles del sistema. El valor guardado en la base es el texto de cada constante.
    /// Web: Admin, Veterinario y Recepcionista. App móvil: Dueno.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Veterinario = "Veterinario";
        public const string Recepcionista = "Recepcionista";
        public const string Dueno = "Dueno";

        // Combinaciones usadas en [Authorize(Roles = ...)]
        public const string Personal = Admin + "," + Veterinario + "," + Recepcionista;
        public const string AdminORecepcion = Admin + "," + Recepcionista;
        public const string AdminOVeterinario = Admin + "," + Veterinario;

        /// <summary>Roles que un administrador puede crear desde la web.</summary>
        public static readonly string[] RolesDePersonal = { Admin, Veterinario, Recepcionista };
    }
}
