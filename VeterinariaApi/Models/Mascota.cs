using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaApi.Models
{
    public class Mascota
    {
        public const string Perro = "Perro";
        public const string Gato = "Gato";

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        // Solo perros y gatos (alcance del proyecto)
        [Required]
        [MaxLength(10)]
        public string Especie { get; set; } = Perro;

        [MaxLength(60)]
        public string? Raza { get; set; }

        // "Macho" o "Hembra"
        [MaxLength(10)]
        public string? Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? Peso { get; set; }

        public bool Activo { get; set; } = true;

        public int DuenoId { get; set; }
        public Dueno Dueno { get; set; } = null!;

        public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
        public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
        public ICollection<Vacuna> Vacunas { get; set; } = new List<Vacuna>();
        public ICollection<Alergia> Alergias { get; set; } = new List<Alergia>();
        public ICollection<Cirugia> Cirugias { get; set; } = new List<Cirugia>();
    }
}
