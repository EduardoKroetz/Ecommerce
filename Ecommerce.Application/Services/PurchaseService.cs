using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class PurchaseService(
    IPurchaseRepository purchaseRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserProvider userProvider,
    IUnitOfWork uow)
{
    public async Task<PagedResult<PurchaseResponse>> SearchAsync(PaginationQuery query)
    {
        return await purchaseRepository.SearchForCustomerAsync(userProvider.CustomerId, query);
    }

    public async Task<PurchaseResponse> GetByIdAsync(int purchaseId)
    {
        return await purchaseRepository.GetDetailByIdForCustomerAsync(purchaseId, userProvider.CustomerId)
            ?? throw new NotFoundException("Purchase not found.");
    }

    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request)
    {
        var customerId = userProvider.CustomerId;

        var cart = await cartRepository.GetForCustomerAsync(customerId)
            ?? throw new NotFoundException("Cart not found.");

        if (cart.Items.Count == 0)
            throw new DomainException(EDomainExceptionCode.CartEmpty, "Cart cannot be empty.");

        var itemsWithInsufficientStock = cart.Items
            .Where(ci => ci.Quantity > ci.Product.StockBalance)
            .Select(ci => new { ci.ProductId, ci.Product.Name, ci.Product.StockBalance, ci.Quantity })
            .ToList();

        if (itemsWithInsufficientStock.Count > 0)
            throw new DomainException(EDomainExceptionCode.InsufficientStock, "Some items in your cart have insufficient stock.", new { items = itemsWithInsufficientStock });

        var createdAt = DateTime.UtcNow;

        // One order per seller: products from the same seller share the same order
        var orders = cart.Items
            .GroupBy(ci => ci.Product.SellerId)
            .Select(sellerItems =>
            {
                var orderItems = sellerItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price,
                    TotalPrice = ci.Product.Price * ci.Quantity,
                }).ToList();

                return new Order
                {
                    SellerId = sellerItems.Key,
                    Status = EOrderStatus.AwaitingPayment,
                    CreatedAt = createdAt,
                    TotalAmount = orderItems.Sum(oi => oi.TotalPrice),
                    Items = orderItems
                };
            })
            .ToList();

        var totalAmount = orders.Sum(o => o.TotalAmount);

        var purchase = new Purchase
        {
            CustomerId = customerId,
            OriginalTotalAmount = totalAmount,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod!.Value,
            Status = EPurchaseStatus.Pending,
            CreatedAt = createdAt,
            Orders = orders
        };

        await using var transaction = await uow.BeginTransactionAsync();

        foreach (var item in orders.SelectMany(o => o.Items))
        {
            var stockDecrementResult = await productRepository.TryDecrementStockAsync(item.ProductId, item.Quantity);

            if (!stockDecrementResult)
            {
                await transaction.RollbackAsync();

                throw new DomainException(EDomainExceptionCode.InsufficientStock, $"Product '{item.ProductName}' no longer has enough stock.");
            }
        }

        await purchaseRepository.AddAsync(purchase);

        cart.Items.Clear();
        await cartRepository.UpdateAsync(cart);

        await transaction.CommitAsync();

        return PurchaseResponse.Map(purchase);
    }

    public async Task CancelAsync(int purchaseId)
    {
        var purchase = await purchaseRepository.GetByIdForCustomerAsync(purchaseId, userProvider.CustomerId)
            ?? throw new NotFoundException("Purchase not found.");

        if (purchase.Status == EPurchaseStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyCancelled, "Purchase is already cancelled.");

        if (purchase.Status == EPurchaseStatus.Paid)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyPaid, "Purchase cannot be cancelled after payment.");

        foreach (var order in purchase.Orders.Where(o => o.Status == EOrderStatus.AwaitingPayment))
        {
            order.Status = EOrderStatus.Cancelled;
            order.Items.ForEach(oi => oi.Product.StockBalance += oi.Quantity);
        }

        purchase.Status = EPurchaseStatus.Cancelled;
        purchase.TotalAmount = purchase.Orders.Where(o => o.Status != EOrderStatus.Cancelled).Sum(o => o.TotalAmount);

        await purchaseRepository.UpdateAsync(purchase);
    }

    public async Task PayAsync(int purchaseId)
    {
        var purchase = await purchaseRepository.GetByIdForCustomerAsync(purchaseId, userProvider.CustomerId)
            ?? throw new NotFoundException("Purchase not found.");

        if (purchase.Status == EPurchaseStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.PurchaseCancelled, "Purchase is cancelled.");

        if (purchase.Status == EPurchaseStatus.Paid)
            throw new DomainException(EDomainExceptionCode.PurchaseAlreadyPaid, "Purchase is already paid.");

        purchase.Status = EPurchaseStatus.Paid;

        foreach (var order in purchase.Orders.Where(o => o.Status == EOrderStatus.AwaitingPayment))
            order.Status = EOrderStatus.AwaitingShipment;

        await purchaseRepository.UpdateAsync(purchase);
    }
}
