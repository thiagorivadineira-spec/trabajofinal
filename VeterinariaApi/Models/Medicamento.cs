using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    public class Medicamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Ej: "Comprimidos 500 mg", "Frasco 100 ml"
        [MaxLength(100)]
        public string? Presentacion { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public int StockActual { get; set; }

        // Por debajo de este valor hay que reponer
        public int StockMinimo { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        public ICollection<Receta> Recetas { get; set; } = new List<Receta>();
    }
}
