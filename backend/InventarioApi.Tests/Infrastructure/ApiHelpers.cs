using System.Net.Http.Headers;
using System.Net.Http.Json;
using InventarioApi.DTOs;
using InventarioApi.Models;

namespace InventarioApi.Tests.Infrastructure;

// Utilidades para que cada prueba pueda crear sus propios datos sin repetir código
public static class ApiHelpers
{
    public const string PasswordPrueba = "Clave-Prueba-123";

    // Nombre único: así las pruebas no chocan aunque compartan la misma base
    public static string NombreUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}"[..20];

    // Registra un usuario nuevo y devuelve un cliente HTTP ya autenticado con su token
    public static async Task<HttpClient> CrearClienteAutenticadoAsync(ApiFactory factory)
    {
        var cliente = factory.CreateClient();
        var registro = new RegistroDto { NombreUsuario = NombreUnico("usr"), Password = PasswordPrueba };

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/register", registro);
        respuesta.EnsureSuccessStatusCode();

        var auth = await respuesta.Content.ReadFromJsonAsync<AuthRespuestaDto>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return cliente;
    }
        // Crea un producto nuevo (con stock 0) y lo devuelve
    public static async Task<Producto> CrearProductoAsync(HttpClient cliente)
    {
        var dto = new ProductoGuardarDto
        {
            Codigo = NombreUnico("prd"),
            Nombre = "Producto de prueba",
            Precio = 1000,
            StockMinimo = 0
        };

        var respuesta = await cliente.PostAsJsonAsync("/api/productos", dto);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<Producto>())!;
    }

    public static Task<HttpResponseMessage> RegistrarEntradaAsync(HttpClient cliente, int productoId, int cantidad) =>
        cliente.PostAsJsonAsync("/api/inventario/entrada",
            new MovimientoCrearDto { ProductoId = productoId, Cantidad = cantidad });

    public static Task<HttpResponseMessage> RegistrarSalidaAsync(HttpClient cliente, int productoId, int cantidad) =>
        cliente.PostAsJsonAsync("/api/inventario/salida",
            new MovimientoCrearDto { ProductoId = productoId, Cantidad = cantidad });

    // Lee el stock actual del producto directamente desde la API
    public static async Task<int> ObtenerStockAsync(HttpClient cliente, int productoId)
    {
        var producto = await cliente.GetFromJsonAsync<Producto>($"/api/productos/{productoId}");
        return producto!.StockActual;
    }
}