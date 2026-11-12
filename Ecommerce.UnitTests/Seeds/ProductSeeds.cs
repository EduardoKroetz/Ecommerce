namespace Ecommerce.UnitTests.Seeds;

public class ProductSeeds
{
    public static Product Create(
        int sellerId = 1,
        string name = "Test Product",
        string description = "Test description",
        decimal price = 10.0m,
        int stockBalance = 100,
        DateTime createdAt = default)
    {
        return new Product(sellerId, name, description, price, stockBalance, createdAt);
    }
}
