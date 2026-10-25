using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Extensions;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class PurchaseService(
    IPurchaseRepository purchaseRepository,
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserProvider userProvider,
    IUnitOfWork uow,
    TimeProvider timeProvider)
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

        cart.EnsureNotEmpty();

        // Products are another aggregate: loaded by id, never through the cart item
        var products = (await productRepository.GetListByIdsAsync(cart.Items.Select(ci => ci.ProductId)))
            .ToDictionary(p => p.Id);

        var itemsWithInsufficientStock = cart.Items
            .Where(ci => ci.Quantity > products[ci.ProductId].StockBalance)
            .Select(ci => new { ci.ProductId, products[ci.ProductId].Name, products[ci.ProductId].StockBalance, ci.Quantity })
            .ToList();

        if (itemsWithInsufficientStock.Count > 0)
            throw new DomainException(EDomainExceptionCode.InsufficientStock, "Some items in your cart have insufficient stock.", new { items = itemsWithInsufficientStock });

        var createdAt = timeProvider.GetUtcDateTime();

        // One order per seller: products from the same seller share the same order
        var itemsBySeller = cart.Items
            .GroupBy(ci => products[ci.ProductId].SellerId)
            .Select(sellerItems => new
            {
                SellerId = sellerItems.Key,
                Items = sellerItems.Select(ci => new OrderItem(products[ci.ProductId], ci.Quantity)).ToList()
            })
            .ToList();

        await using var transaction = await uow.BeginTransactionAsync();

        foreach (var item in itemsBySeller.SelectMany(s => s.Items))
        {
            var stockDecrementResult = await productRepository.TryDecrementStockAsync(item.ProductId, item.Quantity);

            if (!stockDecrementResult)
            {
                await transaction.RollbackAsync();

                throw new DomainException(EDomainExceptionCode.InsufficientStock, $"Product '{item.ProductName}' no longer has enough stock.");
            }
        }

        var orders = itemsBySeller.Select(s => new Order(s.SellerId, s.Items, createdAt)).ToList();
        var purchase = new Purchase(customerId, request.PaymentMethod!.Value, orders, createdAt);

        // The purchase is saved first: orders reference it by id (ADR 0016)
        await purchaseRepository.AddAsync(purchase);

        foreach (var order in orders)
            order.AssignToPurchase(purchase.Id);

        await orderRepository.AddRangeAsync(orders);

        cart.Clear();
        await cartRepository.UpdateAsync(cart);

        await transaction.CommitAsync();

        return await GetByIdAsync(purchase.Id);
    }

    public async Task CancelAsync(int purchaseId)
    {
        var purchase = await purchaseRepository.GetByIdForCustomerAsync(purchaseId, userProvider.CustomerId)
            ?? throw new NotFoundException("Purchase not found.");

        purchase.Cancel();

        var orders = await orderRepository.GetListByPurchaseIdAsync(purchase.Id);

        await using var transaction = await uow.BeginTransactionAsync();

        foreach (var order in orders.Where(o => o.Status == EOrderStatus.AwaitingPayment))
        {
            order.Cancel();

            foreach (var item in order.Items)
                await productRepository.IncrementStockAsync(item.ProductId, item.Quantity);
        }

        purchase.RecalculateTotal(orders);

        await orderRepository.UpdateRangeAsync(orders);
        await purchaseRepository.UpdateAsync(purchase);

        await transaction.CommitAsync();
    }

    public async Task PayAsync(int purchaseId)
    {
        var purchase = await purchaseRepository.GetByIdForCustomerAsync(purchaseId, userProvider.CustomerId)
            ?? throw new NotFoundException("Purchase not found.");

        purchase.Pay();

        var orders = await orderRepository.GetListByPurchaseIdAsync(purchase.Id);

        foreach (var order in orders.Where(o => o.Status == EOrderStatus.AwaitingPayment))
            order.MarkAsPaid();

        await using var transaction = await uow.BeginTransactionAsync();

        await orderRepository.UpdateRangeAsync(orders);
        await purchaseRepository.UpdateAsync(purchase);

        await transaction.CommitAsync();
    }
}
