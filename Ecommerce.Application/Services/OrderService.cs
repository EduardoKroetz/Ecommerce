using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IPurchaseRepository purchaseRepository,
    IProductRepository productRepository,
    ICurrentUserProvider userProvider,
    IUnitOfWork uow)
{
    public async Task<PagedResult<OrderResponse>> SearchAsync(PaginationQuery query)
    {
        return await orderRepository.SearchForCustomerAsync(userProvider.CustomerId, query);
    }

    public async Task<PagedResult<OrderResponse>> SearchSellerOrdersAsync(PaginationQuery query)
    {
        var sellerId = userProvider.SellerId ?? throw new ForbiddenException("User is not a seller.");

        return await orderRepository.SearchForSellerAsync(sellerId, query);
    }

    public async Task<OrderResponse> GetByIdAsync(int orderId)
    {
        return await orderRepository.GetDetailByIdForCustomerOrSellerAsync(orderId, userProvider.CustomerId, userProvider.SellerId)
            ?? throw new NotFoundException("Order not found.");
    }

    public async Task CancelAsync(int orderId)
    {
        var order = await orderRepository.GetByIdForCustomerOrSellerAsync(orderId, userProvider.CustomerId, userProvider.SellerId)
            ?? throw new NotFoundException("Order not found.");

        order.Cancel();

        // Purchase and its orders are loaded by id, never through the order (domain-model.md)
        var purchase = await purchaseRepository.GetByIdAsync(order.PurchaseId)
            ?? throw new InvalidOperationException($"Order '{order.Id}' references a missing purchase.");

        var purchaseOrders = await orderRepository.GetListByPurchaseIdAsync(order.PurchaseId);

        await using var transaction = await uow.BeginTransactionAsync();

        // Ascending ProductId: concurrent transactions never lock the same products in opposite orders (ADR 0018)
        foreach (var item in order.Items.OrderBy(i => i.ProductId))
            await productRepository.IncrementStockAsync(item.ProductId, item.Quantity);

        purchase.RecalculateTotal(purchaseOrders);
        purchase.CancelIfAllOrdersCancelled(purchaseOrders);

        await uow.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task ShipAsync(int orderId)
    {
        var sellerId = userProvider.SellerId ?? throw new ForbiddenException("User is not a seller.");

        var order = await orderRepository.GetByIdForSellerAsync(orderId, sellerId)
            ?? throw new NotFoundException("Order not found.");

        order.Ship();

        await uow.SaveChangesAsync();
    }

    public async Task ConfirmDeliveryAsync(int orderId)
    {
        var order = await orderRepository.GetByIdForCustomerAsync(orderId, userProvider.CustomerId)
            ?? throw new NotFoundException("Order not found.");

        order.ConfirmDelivery();

        await uow.SaveChangesAsync();
    }
}
