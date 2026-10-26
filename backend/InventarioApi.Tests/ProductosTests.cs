using System.Net;
using System.Net.Http.Json;
using InventarioApi.DTOs;
using InventarioApi.Tests.Infrastructure;

namespace InventarioApi.Tests;

[Collection("Api")]
public class ProductosTests
{
    private readonly ApiFactory _factory;

    public ProductosTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CrearProducto_ConDatosInvalidos_Devuelve400()
    {
        // Arrange: precio negativo y nombre vacío
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var invalido = new ProductoGuardarDto
        {
            Codigo = ApiHelpers.NombreUnico("prd"),
            Nombre = "",
            Precio = -5,
            StockMinimo = 0
        };

        // Act
        var respuesta = await cliente.PostAsJsonAsync("/api/productos", invalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}