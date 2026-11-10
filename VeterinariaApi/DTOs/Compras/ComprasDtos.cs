using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class CompraRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el distribuidor.")]
        public int DistribuidorId { get; set; }

        public List<DetalleCompraRequestDto> Detalles { get; set; } = new();
    }

    public class DetalleCompraRequestDto
    {
        public int MedicamentoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CompraResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int DistribuidorId { get; set; }
        public string DistribuidorNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public List<DetalleCompraResponseDto> Detalles { get; set; } = new();
    }

    public class DetalleCompraResponseDto
    {
        public int Id { get; set; }
        public int MedicamentoId { get; set; }
        public string MedicamentoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
