namespace InventarioApi.Tests.Infrastructure;

// Todas las clases de prueba marcadas con [Collection("Api")] comparten UNA sola fábrica
[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory> { }