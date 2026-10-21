using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioApi.Models;

// Entidad de la tabla "movimientos". Solo la leemos: se escribe desde los stored procedures.
[Table("movimientos")]
public class Movimiento
{
    [Column("id")] public int Id { get; set; }
    [Column("producto_id")] public int ProductoId { get; set; }
    [Column("tipo")] public string Tipo { get; set; } = string.Empty;
    [Column("cantidad")] public int Cantidad { get; set; }
    [Column("observacion")] public string? Observacion { get; set; }
    [Column("fecha")] public DateTime Fecha { get; set; }

    public Producto? Producto { get; set; }
}
