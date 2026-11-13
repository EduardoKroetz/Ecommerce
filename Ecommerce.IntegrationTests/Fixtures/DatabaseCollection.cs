namespace Ecommerce.IntegrationTests.Fixtures;

// Every class in this collection shares the fixture and runs sequentially: the database is reset before each test
[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
