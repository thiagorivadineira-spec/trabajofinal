using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaApi.Models
{
    /// <summary>Compra de medicamentos a un distribuidor. Suma stock automáticamente.</summary>
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = Services.Hora.Ahora;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public int DistribuidorId { get; set; }
        public Distribuidor Distribuidor { get; set; } = null!;

        // Usuario (admin) que registró la compra
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}
