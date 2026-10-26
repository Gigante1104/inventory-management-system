using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InventarioApi.Tests.Infrastructure;

// Lee la conexión de la base de pruebas y la prepara con los mismos scripts SQL del proyecto
public static class TestDatabase
{
    private static readonly Lazy<string> _conexion = new(Leer);

    public static string ConnectionString => _conexion.Value;

    private static string Leer()
    {
        // Busca primero en User Secrets y luego en variables de entorno (TestDb__ConnectionString)
        var config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestDatabase).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var conexion = config["TestDb:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Falta TestDb:ConnectionString. Configúrala con 'dotnet user-secrets set' en el proyecto de pruebas.");

            ExigirBaseDePruebas(conexion);
        return conexion;
    }

    // Protección: nunca correr las pruebas contra una base que no sea de pruebas
    public static void ExigirBaseDePruebas(string? conexion)
    {
        var nombreBase = conexion is null ? null : new NpgsqlConnectionStringBuilder(conexion).Database;
        if (nombreBase is null || !nombreBase.EndsWith("_test", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"La base '{nombreBase}' no termina en '_test'. Las pruebas se niegan a correr para proteger tus datos.");
    }

    // Ejecuta 01_tablas.sql y 02_funciones_procedimientos.sql (se pueden repetir sin problema)
    public static void Preparar()
    {
        using var conexion = new NpgsqlConnection(ConnectionString);
        conexion.Open();

        foreach (var archivo in new[] { "01_tablas.sql", "02_funciones_procedimientos.sql" })
        {
            using var comando = new NpgsqlCommand(File.ReadAllText(RutaScript(archivo)), conexion);
            comando.ExecuteNonQuery();
        }
    }

    // Sube carpetas desde donde corren las pruebas hasta encontrar /database
    private static string RutaScript(string archivo)
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null)
        {
            var ruta = Path.Combine(carpeta.FullName, "database", archivo);
            if (File.Exists(ruta)) return ruta;
            carpeta = carpeta.Parent;
        }
        throw new FileNotFoundException($"No se encontró database/{archivo} subiendo desde {AppContext.BaseDirectory}");
    }
}