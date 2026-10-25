using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

// Holds only its own state: rules spanning purchase and orders are orchestrated by the Application (ADR 0014)
public class Purchase : BaseEntity
{
    // Cross-aggregate navigation: Infra queries only, never commands or entity methods (domain-model.md)
    private readonly List<Order> _orders = [];

    private Purchase() { } // EF

    // Orders are only read to compute the totals; they are linked afterwards through Order.AssignToPurchase
    public Purchase(int customerId, EPaymentMethod paymentMethod, IEnumerable<Order> orders, DateTime createdAt)
    {
        var purchaseOrders = orders.ToList();

        if (purchaseOrders.Count == 0)
            throw new DomainException(EDomainExceptionCode.PurchaseWithoutOrders, "Purchase must have at least one order.");

        if (purchaseOrders.GroupBy(o => o.SellerId).Any(sellerOrders => sellerOrders.Count() > 1))
            throw new DomainException(EDomainExceptionCode.PurchaseDuplicateSellerOrders, "Purchase must have only one order per seller.");

        CustomerId = customerId;
        PaymentMethod = paymentMethod;
        OriginalTotalAmount = SumActiveOrders(purchaseOrders);
        TotalAmount = OriginalTotalAmount;
        Status = EPurchaseStatus.Pending;
        CreatedAt = createdAt;
    }

    public int CustomerId { get; private set; }
    public decimal OriginalTotalAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public EPaymentMethod PaymentMethod { get; private set; }
    public EPurchaseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public void Pay()
    {
        if (Status == EPurchaseStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.PurchaseCancelled, "Purchase is cancelled.");

        if (Status == EPurchaseStatus.Paid)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyPaid, "Purchase is already paid.");

        Status = EPurchaseStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == EPurchaseStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyCancelled, "Purchase is already cancelled.");

        if (Status == EPurchaseStatus.Paid)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyPaid, "Purchase cannot be cancelled after payment.");

        Status = EPurchaseStatus.Cancelled;
    }

    // Sum of the non-cancelled orders. OriginalTotalAmount stays frozen.
    public void RecalculateTotal(IEnumerable<Order> orders)
    {
        var purchaseOrders = EnsureOwnOrders(orders);

        TotalAmount = SumActiveOrders(purchaseOrders);
    }

    public void CancelIfAllOrdersCancelled(IEnumerable<Order> orders)
    {
        var purchaseOrders = EnsureOwnOrders(orders);

        if (purchaseOrders.Count > 0 && purchaseOrders.All(o => o.Status == EOrderStatus.Cancelled))
            Cancel();
    }

    private static decimal SumActiveOrders(IEnumerable<Order> orders) =>
        orders.Where(o => o.Status != EOrderStatus.Cancelled).Sum(o => o.TotalAmount);

    private List<Order> EnsureOwnOrders(IEnumerable<Order> orders)
    {
        var purchaseOrders = orders.ToList();

        if (purchaseOrders.Any(o => o.PurchaseId != Id))
            throw new InvalidOperationException($"Orders passed to purchase '{Id}' must belong to it.");

        return purchaseOrders;
    }
}
