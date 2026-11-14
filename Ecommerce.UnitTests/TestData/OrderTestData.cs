namespace Ecommerce.UnitTests.TestData;

public static class OrderTestData
{
    public static Order Create(int sellerId = 1, List<OrderItem>? items = null, DateTime createdAt = default)
    {
        items ??= new()
        {
            new(productId: 1, productName: "Test Product 1", unitPrice: 10.0m, quantity: 2),
            new(productId: 2, productName: "Test Product 2", unitPrice: 20.0m, quantity: 1)
        };

        return new Order(sellerId, items, createdAt);
    }

    // Reaches the status through the public transitions, never by reflection
    public static Order CreateWithStatus(EOrderStatus status)
    {
        var order = Create();

        switch (status)
        {
            case EOrderStatus.AwaitingPayment:
                break;
            case EOrderStatus.AwaitingShipment:
                order.MarkAsPaid();
                break;
            case EOrderStatus.Shipped:
                order.MarkAsPaid();
                order.Ship();
                break;
            case EOrderStatus.Delivered:
                order.MarkAsPaid();
                order.Ship();
                order.ConfirmDelivery();
                break;
            case EOrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return order;
    }
}
