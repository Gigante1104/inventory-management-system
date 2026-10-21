using InventarioApi.DTOs;
using InventarioApi.Models;
using InventarioApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Controllers;

[ApiController]
[Authorize] // todos los endpoints requieren token JWT
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productos;

    public ProductosController(IProductoService productos) => _productos = productos;

    [HttpGet]
    public async Task<ActionResult<List<Producto>>> Listar() => Ok(await _productos.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> Obtener(int id) => Ok(await _productos.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Producto>> Crear(ProductoGuardarDto dto)
    {
        var creado = await _productos.CrearAsync(dto);
        // 201 Created + cabecera Location apuntando al nuevo recurso
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> Actualizar(int id, ProductoGuardarDto dto) =>
        Ok(await _productos.ActualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productos.EliminarAsync(id);
        return NoContent();
    }
}
