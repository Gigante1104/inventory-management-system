using InventarioApi.Data;
using InventarioApi.DTOs;
using InventarioApi.Exceptions;
using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Services;

public interface IProductoService
{
    Task<List<Producto>> ListarAsync();
    Task<Producto> ObtenerAsync(int id);
    Task<Producto> CrearAsync(ProductoGuardarDto dto);
    Task<Producto> ActualizarAsync(int id, ProductoGuardarDto dto);
    Task EliminarAsync(int id);
}

public class ProductoService : IProductoService
{
    private readonly AppDbContext _db;

    public ProductoService(AppDbContext db) => _db = db;

    public Task<List<Producto>> ListarAsync() =>
        _db.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();

    public async Task<Producto> ObtenerAsync(int id)
    {
        var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return producto ?? throw new NotFoundException($"El producto {id} no existe");
    }

    public async Task<Producto> CrearAsync(ProductoGuardarDto dto)
    {
        var codigo = dto.Codigo.Trim();
        if (await _db.Productos.AnyAsync(p => p.Codigo == codigo))
            throw new ConflictException($"Ya existe un producto con el código {codigo}");

        var producto = new Producto
        {
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Precio = dto.Precio,
            StockMinimo = dto.StockMinimo
            // StockActual queda en 0: solo cambia con entradas y salidas
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    public async Task<Producto> ActualizarAsync(int id, ProductoGuardarDto dto)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"El producto {id} no existe");

        var codigo = dto.Codigo.Trim();
        if (await _db.Productos.AnyAsync(p => p.Codigo == codigo && p.Id != id))
            throw new ConflictException($"Ya existe otro producto con el código {codigo}");

        producto.Codigo = codigo;
        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion?.Trim();
        producto.Precio = dto.Precio;
        producto.StockMinimo = dto.StockMinimo;

        await _db.SaveChangesAsync();
        return producto;
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"El producto {id} no existe");

        // Si tiene historial no se borra, para no perder trazabilidad
        if (await _db.Movimientos.AnyAsync(m => m.ProductoId == id))
            throw new ConflictException("No se puede eliminar un producto que tiene movimientos registrados");

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
    }
}
