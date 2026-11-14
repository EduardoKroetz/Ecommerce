using Ecommerce.Domain.Exceptions;

namespace Ecommerce.UnitTests.Entities;

public class OrderTests
{
    [Fact]
    public void Create_StartsAwaitingPayment()
    {
        var order = OrderTestData.Create();

        Assert.Equal(EOrderStatus.AwaitingPayment, order.Status);
    }

    [Fact]
    public void Create_WithoutItems_IsRejected()
    {
        var exception = Assert.Throws<DomainException>(() => OrderTestData.Create(items: []));
        Assert.Equal(EDomainExceptionCode.OrderWithoutItems, exception.Code);
    }

    [Fact]
    public void Create_TotalIsSumOfItemPriceTimesQuantity()
    {
        var items = new List<OrderItem>
        {
            new(productId: 1, productName: "Test Product 1", unitPrice: 10.0m, quantity: 2),
            new(productId: 2, productName: "Test Product 2", unitPrice: 20.0m, quantity: 1)
        };

        var order = OrderTestData.Create(items: items);

        Assert.Equal(40.0m, order.TotalAmount);
    }

    [Fact]
    public void Create_WithDuplicatedItems_IsRejected()
    {
        var items = new List<OrderItem>
        {
            new(productId: 1, productName: "Test Product", unitPrice: 10.0m, quantity: 2),
            new(productId: 1, productName: "Test Product", unitPrice: 10.0m, quantity: 3)
        };

        var exception = Assert.Throws<DomainException>(() => OrderTestData.Create(items: items));
        Assert.Equal(EDomainExceptionCode.OrderWithDuplicatedItems, exception.Code);
    }

    [Fact]
    public void Create_KeepsSellerIdAndCreatedAt()
    {
        var createdAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var order = OrderTestData.Create(sellerId: 7, createdAt: createdAt);

        Assert.Equal(7, order.SellerId);
        Assert.Equal(createdAt, order.CreatedAt);
    }

    [Fact]
    public void Create_IsNotAssignedToPurchase()
    {
        var order = OrderTestData.Create();

        Assert.Equal(0, order.PurchaseId);
    }

    [Fact]
    public void Create_WithSourceListChangedAfterwards_KeepsItemsAndTotal()
    {
        var items = new List<OrderItem>
        {
            new(productId: 1, productName: "Test Product 1", unitPrice: 10.0m, quantity: 2)
        };
        var order = OrderTestData.Create(items: items);

        items.Add(new(productId: 2, productName: "Test Product 2", unitPrice: 20.0m, quantity: 1));

        Assert.Single(order.Items);
        Assert.Equal(20.0m, order.TotalAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateOrderItem_WithNonPositiveQuantity_IsRejected(int quantity)
    {
        var exception = Assert.Throws<DomainException>(() => new OrderItem(productId: 1, productName: "Test Product", unitPrice: 10.0m, quantity));
        Assert.Equal(EDomainExceptionCode.InvalidOrderItemQuantity, exception.Code);
    }

    [Fact]
    public void CreateOrderItem_TotalPriceIsUnitPriceTimesQuantity()
    {
        var orderItem = new OrderItem(productId: 1, productName: "Test Product", unitPrice: 12.50m, quantity: 3);

        Assert.Equal(37.50m, orderItem.TotalPrice);
    }

    [Fact]
    public void AssignToPurchase_SetsPurchaseId()
    {
        var order = OrderTestData.Create();

        order.AssignToPurchase(5);

        Assert.Equal(5, order.PurchaseId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AssignToPurchase_WithNonPositiveId_Throws(int purchaseId)
    {
        var order = OrderTestData.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => order.AssignToPurchase(purchaseId));
        Assert.Equal(0, order.PurchaseId);
    }

    [Fact]
    public void AssignToPurchase_WhenAlreadyAssigned_Throws()
    {
        var order = OrderTestData.Create();
        order.AssignToPurchase(5);

        Assert.Throws<InvalidOperationException>(() => order.AssignToPurchase(6));
        Assert.Equal(5, order.PurchaseId);
    }

    [Fact]
    public void MarkAsPaid_WhenAwaitingPayment_MovesToAwaitingShipment()
    {
        var order = OrderTestData.Create();

        order.MarkAsPaid();

        Assert.Equal(EOrderStatus.AwaitingShipment, order.Status);
    }

    [Theory]
    [InlineData(EOrderStatus.AwaitingShipment)]
    [InlineData(EOrderStatus.Shipped)]
    [InlineData(EOrderStatus.Delivered)]
    [InlineData(EOrderStatus.Cancelled)]
    public void MarkAsPaid_WhenNotAwaitingPayment_IsRejected(EOrderStatus status)
    {
        var order = OrderTestData.CreateWithStatus(status);

        var exception = Assert.Throws<DomainException>(() => order.MarkAsPaid());
        Assert.Equal(EDomainExceptionCode.InvalidOrderStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = status }, exception.Details);
        Assert.Equal(status, order.Status);
    }

    [Fact]
    public void Cancel_WhenAwaitingPayment_MovesToCancelled()
    {
        var order = OrderTestData.Create();

        order.Cancel();

        Assert.Equal(EOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsRejected()
    {
        var order = OrderTestData.CreateWithStatus(EOrderStatus.Cancelled);

        var exception = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(EDomainExceptionCode.OrderAlreadyCancelled, exception.Code);
        Assert.Equal(EOrderStatus.Cancelled, order.Status);
    }

    [Theory]
    [InlineData(EOrderStatus.AwaitingShipment)]
    [InlineData(EOrderStatus.Shipped)]
    [InlineData(EOrderStatus.Delivered)]
    public void Cancel_WhenAlreadyPaid_IsRejected(EOrderStatus status)
    {
        var order = OrderTestData.CreateWithStatus(status);

        var exception = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(EDomainExceptionCode.InvalidOrderStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = status }, exception.Details);
        Assert.Equal(status, order.Status);
    }

    [Fact]
    public void Ship_WhenAwaitingShipment_MovesToShipped()
    {
        var order = OrderTestData.CreateWithStatus(EOrderStatus.AwaitingShipment);

        order.Ship();

        Assert.Equal(EOrderStatus.Shipped, order.Status);
    }

    [Theory]
    [InlineData(EOrderStatus.AwaitingPayment)]
    [InlineData(EOrderStatus.Shipped)]
    [InlineData(EOrderStatus.Delivered)]
    [InlineData(EOrderStatus.Cancelled)]
    public void Ship_WhenNotAwaitingShipment_IsRejected(EOrderStatus status)
    {
        var order = OrderTestData.CreateWithStatus(status);

        var exception = Assert.Throws<DomainException>(() => order.Ship());
        Assert.Equal(EDomainExceptionCode.InvalidOrderStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = status }, exception.Details);
        Assert.Equal(status, order.Status);
    }

    [Fact]
    public void ConfirmDelivery_WhenShipped_MovesToDelivered()
    {
        var order = OrderTestData.CreateWithStatus(EOrderStatus.Shipped);

        order.ConfirmDelivery();

        Assert.Equal(EOrderStatus.Delivered, order.Status);
    }

    [Theory]
    [InlineData(EOrderStatus.AwaitingPayment)]
    [InlineData(EOrderStatus.AwaitingShipment)]
    [InlineData(EOrderStatus.Delivered)]
    [InlineData(EOrderStatus.Cancelled)]
    public void ConfirmDelivery_WhenNotShipped_IsRejected(EOrderStatus status)
    {
        var order = OrderTestData.CreateWithStatus(status);

        var exception = Assert.Throws<DomainException>(() => order.ConfirmDelivery());
        Assert.Equal(EDomainExceptionCode.InvalidOrderStatus, exception.Code);
        Assert.Equivalent(new { currentStatus = status }, exception.Details);
        Assert.Equal(status, order.Status);
    }
}
