using System.Net;
using InventarioApi.Tests.Infrastructure;

namespace InventarioApi.Tests;

[Collection("Api")]
public class ConcurrenciaTests
{
    private readonly ApiFactory _factory;

    public ConcurrenciaTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task DosSalidasSimultaneas_PorLaUltimaUnidad_SoloUnaTieneExito()
    {
        // Arrange: producto con UNA sola unidad
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var producto = await ApiHelpers.CrearProductoAsync(cliente);
        await ApiHelpers.RegistrarEntradaAsync(cliente, producto.Id, 1);

        // Act: dos salidas lanzadas al mismo tiempo
        var respuestas = await Task.WhenAll(
            ApiHelpers.RegistrarSalidaAsync(cliente, producto.Id, 1),
            ApiHelpers.RegistrarSalidaAsync(cliente, producto.Id, 1));

        // Assert: una gana, la otra recibe el error del negocio y el stock queda en 0
        Assert.Equal(1, respuestas.Count(r => r.StatusCode == HttpStatusCode.OK));
        var fallida = respuestas.Single(r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Contains("Stock insuficiente", await fallida.Content.ReadAsStringAsync());
        Assert.Equal(0, await ApiHelpers.ObtenerStockAsync(cliente, producto.Id));
    }
}