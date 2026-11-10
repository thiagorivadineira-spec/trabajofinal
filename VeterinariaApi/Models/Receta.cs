using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    /// <summary>Medicamento recetado en una atención. Descuenta stock al registrarse.</summary>
    public class Receta
    {
        [Key]
        public int Id { get; set; }

        public int AtencionId { get; set; }
        public Atencion Atencion { get; set; } = null!;

        public int MedicamentoId { get; set; }
        public Medicamento Medicamento { get; set; } = null!;

        // Unidades entregadas (se descuentan del stock)
        public int Cantidad { get; set; }

        [MaxLength(100)]
        public string? Dosis { get; set; }

        [MaxLength(300)]
        public string? Indicaciones { get; set; }
    }
}
