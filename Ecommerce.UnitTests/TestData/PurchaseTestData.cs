namespace Ecommerce.UnitTests.TestData;

public static class PurchaseTestData
{
    public static Purchase Create(
        int customerId = 1,
        EPaymentMethod paymentMethod = EPaymentMethod.PIX,
        List<Order>? orders = null,
        DateTime createdAt = default)
    {
        orders ??= [OrderTestData.Create(sellerId: 1), OrderTestData.Create(sellerId: 2)];

        return new Purchase(customerId, paymentMethod, orders, createdAt);
    }

    // Mirrors the checkout: the purchase gets its id and the orders are assigned to it afterwards
    public static Purchase CreateWithAssignedOrders(List<Order> orders, int id = 10)
    {
        var purchase = Create(orders: orders).WithId(id);

        foreach (var order in orders)
            order.AssignToPurchase(id);

        return purchase;
    }

    // Reaches the status through the public transitions
    public static Purchase CreateWithStatus(EPurchaseStatus status)
    {
        var purchase = Create();

        switch (status)
        {
            case EPurchaseStatus.Pending:
                break;
            case EPurchaseStatus.Paid:
                purchase.Pay();
                break;
            case EPurchaseStatus.Cancelled:
                purchase.Cancel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return purchase;
    }
}
