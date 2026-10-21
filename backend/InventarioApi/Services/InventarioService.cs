using InventarioApi.Data;
using InventarioApi.DTOs;
using InventarioApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Services;

public interface IInventarioService
{
    Task<List<InventarioItemDto>> ObtenerInventarioAsync();
    Task<MovimientoResultadoDto> RegistrarEntradaAsync(MovimientoCrearDto dto);
    Task<MovimientoResultadoDto> RegistrarSalidaAsync(MovimientoCrearDto dto);
    Task<List<MovimientoRespuestaDto>> ListarMovimientosAsync(int? productoId, string? tipo);
}

public class InventarioService : IInventarioService
{
    private readonly AppDbContext _db;

    public InventarioService(AppDbContext db) => _db = db;

    // Llama a la función de PostgreSQL. Los alias "Nombre" hacen que cada columna calce con la propiedad del DTO.
    public Task<List<InventarioItemDto>> ObtenerInventarioAsync() =>
        _db.Database.SqlQueryRaw<InventarioItemDto>(
                """
                SELECT id AS "Id", codigo AS "Codigo", nombre AS "Nombre", precio AS "Precio",
                       stock_actual AS "StockActual", stock_minimo AS "StockMinimo",
                       valor_total AS "ValorTotal", bajo_stock AS "BajoStock"
                FROM fn_inventario_actual()
                """)
            .OrderBy(i => i.Nombre)
            .ToListAsync();

    public async Task<MovimientoResultadoDto> RegistrarEntradaAsync(MovimientoCrearDto dto)
    {
        // CALL ejecuta el procedure. Los valores van como parámetros (no se concatenan: evita SQL injection)
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"CALL sp_registrar_entrada({dto.ProductoId}, {dto.Cantidad}, {dto.Observacion})");

        return await ArmarResultadoAsync(dto.ProductoId, "Entrada registrada correctamente");
    }

    public async Task<MovimientoResultadoDto> RegistrarSalidaAsync(MovimientoCrearDto dto)
    {
        // Si no hay stock suficiente, el procedure lanza una excepción y el middleware la convierte en 400
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"CALL sp_registrar_salida({dto.ProductoId}, {dto.Cantidad}, {dto.Observacion})");

        return await ArmarResultadoAsync(dto.ProductoId, "Salida registrada correctamente");
    }

    public async Task<List<MovimientoRespuestaDto>> ListarMovimientosAsync(int? productoId, string? tipo)
    {
        var consulta = _db.Movimientos.AsNoTracking().AsQueryable();

        if (productoId.HasValue)
            consulta = consulta.Where(m => m.ProductoId == productoId.Value);

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            if (tipoNormalizado != "ENTRADA" && tipoNormalizado != "SALIDA")
                throw new BusinessException("El tipo debe ser ENTRADA o SALIDA");
            consulta = consulta.Where(m => m.Tipo == tipoNormalizado);
        }

        return await consulta
            .OrderByDescending(m => m.Fecha)
            .Take(200)
            .Select(m => new MovimientoRespuestaDto
            {
                Id = m.Id,
                ProductoId = m.ProductoId,
                CodigoProducto = m.Producto!.Codigo,
                NombreProducto = m.Producto.Nombre,
                Tipo = m.Tipo,
                Cantidad = m.Cantidad,
                Observacion = m.Observacion,
                Fecha = m.Fecha
            })
            .ToListAsync();
    }

    private async Task<MovimientoResultadoDto> ArmarResultadoAsync(int productoId, string mensaje)
    {
        var stock = await _db.Productos.AsNoTracking()
            .Where(p => p.Id == productoId)
            .Select(p => p.StockActual)
            .FirstAsync();

        return new MovimientoResultadoDto { Mensaje = mensaje, ProductoId = productoId, StockActual = stock };
    }
}
