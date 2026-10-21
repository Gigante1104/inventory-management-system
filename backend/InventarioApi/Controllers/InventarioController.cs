using InventarioApi.DTOs;
using InventarioApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventario;

    public InventarioController(IInventarioService inventario) => _inventario = inventario;

    // GET api/inventario -> estado actual (usa fn_inventario_actual)
    [HttpGet]
    public async Task<ActionResult<List<InventarioItemDto>>> ObtenerInventario() =>
        Ok(await _inventario.ObtenerInventarioAsync());

    // POST api/inventario/entrada
    [HttpPost("entrada")]
    public async Task<ActionResult<MovimientoResultadoDto>> Entrada(MovimientoCrearDto dto) =>
        Ok(await _inventario.RegistrarEntradaAsync(dto));

    // POST api/inventario/salida
    [HttpPost("salida")]
    public async Task<ActionResult<MovimientoResultadoDto>> Salida(MovimientoCrearDto dto) =>
        Ok(await _inventario.RegistrarSalidaAsync(dto));

    // GET api/inventario/movimientos?productoId=1&tipo=ENTRADA
    [HttpGet("movimientos")]
    public async Task<ActionResult<List<MovimientoRespuestaDto>>> Movimientos(
        [FromQuery] int? productoId, [FromQuery] string? tipo) =>
        Ok(await _inventario.ListarMovimientosAsync(productoId, tipo));
}
