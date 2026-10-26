using System.Net;
using InventarioApi.Tests.Infrastructure;
using Npgsql;

namespace InventarioApi.Tests;

[Collection("Api")]
public class BloqueoTests
{
    private readonly ApiFactory _factory;

    public BloqueoTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Salida_ConLaFilaBloqueada_EsperaYLuegoVeElStockActualizado()
    {
        // Arrange: producto con 1 unidad
        var cliente = await ApiHelpers.CrearClienteAutenticadoAsync(_factory);
        var producto = await ApiHelpers.CrearProductoAsync(cliente);
        await ApiHelpers.RegistrarEntradaAsync(cliente, producto.Id, 1);

        // Una transacción externa deja el stock en 0, pero todavía NO confirma (la fila queda bloqueada)
        await using var conexion = new NpgsqlConnection(TestDatabase.ConnectionString);
        await conexion.OpenAsync();
        await using var transaccion = await conexion.BeginTransactionAsync();
        await using (var cmd = new NpgsqlCommand(
            "UPDATE productos SET stock_actual = 0 WHERE id = @id", conexion, transaccion))
        {
            cmd.Parameters.AddWithValue("id", producto.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        // Act: la salida llega mientras la fila está bloqueada y debe quedarse esperando
        var salida = ApiHelpers.RegistrarSalidaAsync(cliente, producto.Id, 1);
        await Task.Delay(500);
        var seBloqueo = !salida.IsCompleted;

        await transaccion.CommitAsync(); // ahora sí confirmamos: el stock real es 0
        var respuesta = await salida;

        // Assert: esperó, y al continuar vio el stock actualizado (no el valor viejo)
        Assert.True(seBloqueo, "La salida debía esperar mientras la fila estaba bloqueada");
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("Stock insuficiente", await respuesta.Content.ReadAsStringAsync());
    }
}