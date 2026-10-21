using System.ComponentModel.DataAnnotations;

namespace InventarioApi.DTOs;

// Cuerpo de la petición para registrar una entrada o una salida
public class MovimientoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un producto")]
    public int ProductoId { get; set; }

    [Range(1, 1000000, ErrorMessage = "La cantidad debe ser mayor a cero")]
    public int Cantidad { get; set; }

    [StringLength(200, ErrorMessage = "La observación admite máximo 200 caracteres")]
    public string? Observacion { get; set; }
}

// Respuesta tras registrar un movimiento
public class MovimientoResultadoDto
{
    public string Mensaje { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public int StockActual { get; set; }
}

// Fila del historial de movimientos
public class MovimientoRespuestaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string? Observacion { get; set; }
    public DateTime Fecha { get; set; }
}

// Fila que devuelve la función fn_inventario_actual()
public class InventarioItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal ValorTotal { get; set; }
    public bool BajoStock { get; set; }
}
