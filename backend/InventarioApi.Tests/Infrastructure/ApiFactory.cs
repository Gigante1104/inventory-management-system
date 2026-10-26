using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventarioApi.Tests.Infrastructure;

// Arranca la API real en memoria, pero apuntando a la base inventario_test
public class ApiFactory : WebApplicationFactory<Program>
{
    public ApiFactory()
    {
        // Crea tablas y procedimientos en inventario_test (una vez al crear la fábrica)
        TestDatabase.Preparar();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Entorno propio de pruebas: la API no carga sus User Secrets de desarrollo
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection", TestDatabase.ConnectionString);

        // Clave JWT aleatoria que solo vive en memoria: no hay ningún secreto que guardar
        builder.UseSetting("Jwt:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Segunda red de seguridad: comprobamos la conexión que realmente quedó dentro de la API
        var conexion = host.Services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
        TestDatabase.ExigirBaseDePruebas(conexion);

        return host;
    }
}