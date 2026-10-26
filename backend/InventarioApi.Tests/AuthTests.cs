using System.Net;
using System.Net.Http.Json;
using InventarioApi.DTOs;
using InventarioApi.Tests.Infrastructure;

namespace InventarioApi.Tests;

[Collection("Api")]
public class AuthTests
{
    private readonly ApiFactory _factory;

    public AuthTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401()
    {
        // Arrange: un usuario existente y un intento con otra contraseña
        var cliente = _factory.CreateClient();
        var usuario = ApiHelpers.NombreUnico("usr");
        await cliente.PostAsJsonAsync("/api/auth/register",
            new RegistroDto { NombreUsuario = usuario, Password = ApiHelpers.PasswordPrueba });

        // Act
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login",
            new LoginDto { NombreUsuario = usuario, Password = "otra-clave-incorrecta" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Productos_SinToken_Devuelve401()
    {
        // Arrange: cliente sin Authorization
        var cliente = _factory.CreateClient();

        // Act
        var respuesta = await cliente.GetAsync("/api/productos");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task RegistroYLogin_DevuelvenTokenJwt()
    {
        // Arrange
        var cliente = _factory.CreateClient();
        var usuario = ApiHelpers.NombreUnico("usr");

        // Act
        var registro = await cliente.PostAsJsonAsync("/api/auth/register",
            new RegistroDto { NombreUsuario = usuario, Password = ApiHelpers.PasswordPrueba });
        var login = await cliente.PostAsJsonAsync("/api/auth/login",
            new LoginDto { NombreUsuario = usuario, Password = ApiHelpers.PasswordPrueba });

        // Assert: un JWT tiene 3 partes separadas por puntos
        var tokenRegistro = (await registro.Content.ReadFromJsonAsync<AuthRespuestaDto>())!.Token;
        var tokenLogin = (await login.Content.ReadFromJsonAsync<AuthRespuestaDto>())!.Token;
        Assert.Equal(3, tokenRegistro.Split('.').Length);
        Assert.Equal(3, tokenLogin.Split('.').Length);
    }
}