using Ecommerce.Domain.Exceptions;

namespace Ecommerce.UnitTests.Entities;

public class PurchaseTests
{
    [Fact]
    public void Create_StartsPending()
    {
        var purchase = PurchaseTestData.Create();

        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }

    [Fact]
    public void Create_WithoutOrders_IsRejected()
    {
        var exception = Assert.Throws<DomainException>(() => PurchaseTestData.Create(orders: []));
        Assert.Equal(EDomainExceptionCode.PurchaseWithoutOrders, exception.Code);
    }

    [Fact]
    public void Create_WithDuplicatedSellerOrders_IsRejected()
    {
        var orders = new List<Order> { OrderTestData.Create(sellerId: 1), OrderTestData.Create(sellerId: 1) };

        var exception = Assert.Throws<DomainException>(() => PurchaseTestData.Create(orders: orders));
        Assert.Equal(EDomainExceptionCode.PurchaseDuplicateSellerOrders, exception.Code);
    }

    [Theory]
    [InlineData(EOrderStatus.AwaitingShipment)]
    [InlineData(EOrderStatus.Shipped)]
    [InlineData(EOrderStatus.Delivered)]
    [InlineData(EOrderStatus.Cancelled)]
    public void Create_WithOrderNotAwaitingPayment_IsRejected(EOrderStatus status)
    {
        var orders = new List<Order> { OrderTestData.CreateWithStatus(status) };

        var exception = Assert.Throws<DomainException>(() => PurchaseTestData.Create(orders: orders));
        Assert.Equal(EDomainExceptionCode.PurchaseWithOrderNotAwaitingPayment, exception.Code);
    }

    [Fact]
    public void Create_TotalIsSumOfOrderTotals()
    {
        var purchase = PurchaseTestData.Create(orders: [OrderOf(sellerId: 1, total: 40.0m), OrderOf(sellerId: 2, total: 30.0m)]);

        Assert.Equal(70.0m, purchase.TotalAmount);
    }

    [Fact]
    public void Create_OriginalTotalEqualsTotal()
    {
        var purchase = PurchaseTestData.Create();

        Assert.Equal(purchase.TotalAmount, purchase.OriginalTotalAmount);
    }

    [Fact]
    public void Create_KeepsCustomerIdPaymentMethodAndCreatedAt()
    {
        var createdAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var purchase = PurchaseTestData.Create(customerId: 7, paymentMethod: EPaymentMethod.Boleto, createdAt: createdAt);

        Assert.Equal(7, purchase.CustomerId);
        Assert.Equal(EPaymentMethod.Boleto, purchase.PaymentMethod);
        Assert.Equal(createdAt, purchase.CreatedAt);
    }

    [Fact]
    public void Pay_WhenPending_MovesToPaid()
    {
        var purchase = PurchaseTestData.Create();

        purchase.Pay();

        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
    }

    [Fact]
    public void Pay_WhenAlreadyPaid_IsRejected()
    {
        var purchase = PurchaseTestData.CreateWithStatus(EPurchaseStatus.Paid);

        var exception = Assert.Throws<DomainException>(() => purchase.Pay());
        Assert.Equal(EDomainExceptionCode.PurchaseAlreadyPaid, exception.Code);
        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
    }

    [Fact]
    public void Pay_WhenCancelled_IsRejected()
    {
        var purchase = PurchaseTestData.CreateWithStatus(EPurchaseStatus.Cancelled);

        var exception = Assert.Throws<DomainException>(() => purchase.Pay());
        Assert.Equal(EDomainExceptionCode.InvalidPurchaseStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = EPurchaseStatus.Cancelled }, exception.Details);
        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
    }

    [Fact]
    public void Cancel_WhenPending_MovesToCancelled()
    {
        var purchase = PurchaseTestData.Create();

        purchase.Cancel();

        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsRejected()
    {
        var purchase = PurchaseTestData.CreateWithStatus(EPurchaseStatus.Cancelled);

        var exception = Assert.Throws<DomainException>(() => purchase.Cancel());
        Assert.Equal(EDomainExceptionCode.PurchaseAlreadyCancelled, exception.Code);
        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
    }

    [Fact]
    public void Cancel_WhenPaid_IsRejected()
    {
        var purchase = PurchaseTestData.CreateWithStatus(EPurchaseStatus.Paid);

        var exception = Assert.Throws<DomainException>(() => purchase.Cancel());
        Assert.Equal(EDomainExceptionCode.InvalidPurchaseStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = EPurchaseStatus.Paid }, exception.Details);
        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
    }

    [Fact]
    public void RecalculateTotal_WithCancelledOrder_ExcludesItFromTotal()
    {
        var orders = new List<Order> { OrderOf(sellerId: 1, total: 40.0m), OrderOf(sellerId: 2, total: 30.0m) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders[0].Cancel();

        purchase.RecalculateTotal(orders);

        Assert.Equal(30.0m, purchase.TotalAmount);
    }

    [Fact]
    public void RecalculateTotal_WithAllOrdersCancelled_TotalIsZero()
    {
        var orders = new List<Order> { OrderOf(sellerId: 1, total: 40.0m), OrderOf(sellerId: 2, total: 30.0m) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders.ForEach(o => o.Cancel());

        purchase.RecalculateTotal(orders);

        Assert.Equal(0.0m, purchase.TotalAmount);
    }

    [Fact]
    public void RecalculateTotal_KeepsOriginalTotal()
    {
        var orders = new List<Order> { OrderOf(sellerId: 1, total: 40.0m), OrderOf(sellerId: 2, total: 30.0m) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders[0].Cancel();

        purchase.RecalculateTotal(orders);

        Assert.Equal(70.0m, purchase.OriginalTotalAmount);
    }

    [Fact]
    public void RecalculateTotal_WithOrderFromAnotherPurchase_Throws()
    {
        var orders = new List<Order> { OrderOf(sellerId: 1, total: 40.0m), OrderOf(sellerId: 2, total: 30.0m) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders, id: 10);
        var foreignOrder = OrderOf(sellerId: 3, total: 50.0m);
        foreignOrder.AssignToPurchase(99);

        Assert.Throws<InvalidOperationException>(() => purchase.RecalculateTotal([.. orders, foreignOrder]));
        Assert.Equal(70.0m, purchase.TotalAmount);
    }

    [Fact]
    public void CancelIfAllOrdersCancelled_WithAllOrdersCancelled_MovesToCancelled()
    {
        var orders = new List<Order> { OrderTestData.Create(sellerId: 1), OrderTestData.Create(sellerId: 2) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders.ForEach(o => o.Cancel());

        purchase.CancelIfAllOrdersCancelled(orders);

        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
    }

    [Fact]
    public void CancelIfAllOrdersCancelled_WithActiveOrder_KeepsPending()
    {
        var orders = new List<Order> { OrderTestData.Create(sellerId: 1), OrderTestData.Create(sellerId: 2) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders[0].Cancel();

        purchase.CancelIfAllOrdersCancelled(orders);

        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }

    [Fact]
    public void CancelIfAllOrdersCancelled_WithoutOrders_KeepsPending()
    {
        var purchase = PurchaseTestData.CreateWithAssignedOrders([OrderTestData.Create()]);

        purchase.CancelIfAllOrdersCancelled([]);

        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }

    [Fact]
    public void CancelIfAllOrdersCancelled_WithOrderFromAnotherPurchase_Throws()
    {
        var orders = new List<Order> { OrderTestData.Create(sellerId: 1) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders, id: 10);
        var foreignOrder = OrderTestData.Create(sellerId: 2);
        foreignOrder.AssignToPurchase(99);
        orders[0].Cancel();
        foreignOrder.Cancel();

        Assert.Throws<InvalidOperationException>(() => purchase.CancelIfAllOrdersCancelled([.. orders, foreignOrder]));
        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }

    [Fact]
    public void CancelIfAllOrdersCancelled_WhenAlreadyCancelled_IsRejected()
    {
        var orders = new List<Order> { OrderTestData.Create(sellerId: 1) };
        var purchase = PurchaseTestData.CreateWithAssignedOrders(orders);
        orders[0].Cancel();
        purchase.Cancel();

        var exception = Assert.Throws<DomainException>(() => purchase.CancelIfAllOrdersCancelled(orders));
        Assert.Equal(EDomainExceptionCode.PurchaseAlreadyCancelled, exception.Code);
        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
    }

    private static Order OrderOf(int sellerId, decimal total) =>
        OrderTestData.Create(sellerId: sellerId, items: [new(productId: sellerId, productName: $"Test Product {sellerId}", unitPrice: total, quantity: 1)]);
}
