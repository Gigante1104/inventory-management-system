using System.Net;
using InventarioApi.Tests.Infrastructure;

namespace InventarioApi.Tests;

[Collection("Api")]
public class InventarioTests
{
    private readonly ApiFactory _factory;

    public InventarioTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Entrada_AumentaElStock()
    {
        // Arrange: producto nuevo con stock 0
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var producto = await ApiHelpers.CrearProductoAsync(cliente);

        // Act
        var respuesta = await ApiHelpers.RegistrarEntradaAsync(cliente, producto.Id, 10);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(10, await ApiHelpers.ObtenerStockAsync(cliente, producto.Id));
    }

    [Fact]
    public async Task Salida_ConMasUnidadesQueElStock_Devuelve400()
    {
        // Arrange: producto con 5 unidades
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var producto = await ApiHelpers.CrearProductoAsync(cliente);
        await ApiHelpers.RegistrarEntradaAsync(cliente, producto.Id, 5);

        // Act: pedimos 6
        var respuesta = await ApiHelpers.RegistrarSalidaAsync(cliente, producto.Id, 6);

        // Assert: el error viene del procedimiento almacenado y el stock no cambió
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("Stock insuficiente", await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(5, await ApiHelpers.ObtenerStockAsync(cliente, producto.Id));
    }

    [Fact]
    public async Task Salida_PorElStockExacto_DejaElStockEnCero()
    {
        // Arrange: producto con 7 unidades
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var producto = await ApiHelpers.CrearProductoAsync(cliente);
        await ApiHelpers.RegistrarEntradaAsync(cliente, producto.Id, 7);

        // Act: sacamos las 7
        var respuesta = await ApiHelpers.RegistrarSalidaAsync(cliente, producto.Id, 7);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(0, await ApiHelpers.ObtenerStockAsync(cliente, producto.Id));
    }
}