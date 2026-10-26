using System.Net.Http.Headers;
using System.Net.Http.Json;
using InventarioApi.DTOs;

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
}