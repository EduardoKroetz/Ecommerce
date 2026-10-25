using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];

    private Order() { } // EF

    public Order(int sellerId, IEnumerable<OrderItem> items, DateTime createdAt)
    {
        _items.AddRange(items);

        if (_items.Count == 0)
            throw new DomainException(EDomainExceptionCode.OrderWithoutItems, "Order must have at least one item.");

        SellerId = sellerId;
        Status = EOrderStatus.AwaitingPayment;
        CreatedAt = createdAt;
        TotalAmount = _items.Sum(oi => oi.TotalPrice);
    }

    public int PurchaseId { get; private set; }
    public int SellerId { get; private set; }
    public EOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal TotalAmount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Cross-aggregate navigation: Infra queries only, never commands or entity methods (domain-model.md)
    internal Purchase Purchase { get; private set; } = null!;

    // The purchase id only exists after the purchase is saved, so the link happens once, after creation
    public void AssignToPurchase(int purchaseId)
    {
        if (PurchaseId != 0)
            throw new InvalidOperationException($"Order '{Id}' is already assigned to purchase '{PurchaseId}'.");

        PurchaseId = purchaseId;
    }

    public void MarkAsPaid()
    {
        if (Status != EOrderStatus.AwaitingPayment)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only orders awaiting payment can be paid.");

        Status = EOrderStatus.AwaitingShipment;
    }

    public void Cancel()
    {
        if (Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderAlreadyCancelled, "Order is already cancelled.");

        if (Status != EOrderStatus.AwaitingPayment)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only orders awaiting payment can be cancelled.");

        Status = EOrderStatus.Cancelled;
    }

    public void Ship()
    {
        if (Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderCancelled, "Order is cancelled.");

        if (Status != EOrderStatus.AwaitingShipment)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only orders awaiting shipment can be shipped.");

        Status = EOrderStatus.Shipped;
    }

    public void ConfirmDelivery()
    {
        if (Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderCancelled, "Order is cancelled.");

        if (Status != EOrderStatus.Shipped)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only shipped orders can be confirmed as delivered.");

        Status = EOrderStatus.Delivered;
    }
}
