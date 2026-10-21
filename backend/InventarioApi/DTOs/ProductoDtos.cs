using System.ComponentModel.DataAnnotations;

namespace InventarioApi.DTOs;

// Se usa tanto para crear como para editar un producto (el stock NO se edita aquí, solo con movimientos)
public class ProductoGuardarDto
{
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(30, ErrorMessage = "El código admite máximo 30 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción admite máximo 300 caracteres")]
    public string? Descripcion { get; set; }

    [Range(0, 999999999, ErrorMessage = "El precio no puede ser negativo")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
    public int StockMinimo { get; set; }
}
