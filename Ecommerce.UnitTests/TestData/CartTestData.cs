namespace Ecommerce.UnitTests.TestData;

public static class CartTestData
{
    // Starts empty: items go in through AddItem, the behavior under test
    public static Cart Create(int customerId = 1, DateTime createdAt = default)
    {
        return new Cart(customerId, createdAt);
    }
}
