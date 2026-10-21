using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioApi.Models;

// Entidad que representa la tabla "productos" (creada con database/01_tablas.sql)
[Table("productos")]
public class Producto
{
    [Column("id")] public int Id { get; set; }
    [Column("codigo")] public string Codigo { get; set; } = string.Empty;
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("descripcion")] public string? Descripcion { get; set; }
    [Column("precio")] public decimal Precio { get; set; }
    [Column("stock_actual")] public int StockActual { get; set; }
    [Column("stock_minimo")] public int StockMinimo { get; set; }
    [Column("creado_en")] public DateTime CreadoEn { get; set; }
}
