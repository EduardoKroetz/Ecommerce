using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class OrderService(IOrderRepository orderRepository, ICurrentUserProvider userProvider)
{
    public async Task<PagedResult<OrderResponse>> SearchAsync(PaginationQuery query)
    {
        return await orderRepository.SearchByUserIdAsync(userProvider.UserId, query);
    }

    public async Task<PagedResult<OrderResponse>> SearchSellerOrdersAsync(PaginationQuery query)
    {
        var sellerId = userProvider.SellerId ?? throw new UnauthorizedException("User is not a seller.");

        return await orderRepository.SearchBySellerIdAsync(sellerId, query);
    }

    public async Task<OrderResponse> GetByIdAsync(int orderId)
    {
        return await orderRepository.GetDetailByIdAndUserIdOrSellerIdAsync(orderId, userProvider.UserId, userProvider.SellerId)
            ?? throw new NotFoundException("Order not found.");
    }

    public async Task CancelAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAndUserIdOrSellerIdAsync(orderId, userProvider.UserId, userProvider.SellerId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderAlreadyCancelled, "Order is already cancelled.");

        if (order.Status != EOrderStatus.AwaitingPayment)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only orders awaiting payment can be cancelled.");

        order.Status = EOrderStatus.Cancelled;

        order.Items.ForEach(oi => oi.Product.StockBalance += oi.Quantity);

        var purchase = order.Purchase;

        purchase.TotalAmount = purchase.Orders.Where(o => o.Status != EOrderStatus.Cancelled).Sum(o => o.TotalAmount);

        if (purchase.Orders.All(o => o.Status == EOrderStatus.Cancelled))
            purchase.Status = EPurchaseStatus.Cancelled;

        await orderRepository.UpdateAsync(order);
    }

    public async Task ShipAsync(int orderId)
    {
        var sellerId = userProvider.SellerId ?? throw new UnauthorizedException("User is not a seller.");

        var order = await orderRepository.GetByIdAndSellerIdAsync(orderId, sellerId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderCancelled, "Order is cancelled.");

        if (order.Status != EOrderStatus.AwaitingShipment)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only orders awaiting shipment can be shipped.");

        order.Status = EOrderStatus.Shipped;

        await orderRepository.UpdateAsync(order);
    }

    public async Task ConfirmDeliveryAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAndUserIdAsync(orderId, userProvider.UserId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderCancelled, "Order is cancelled.");

        if (order.Status != EOrderStatus.Shipped)
            throw new DomainException(EDomainExceptionCode.InvalidOrderStatus, "Only shipped orders can be confirmed as delivered.");

        order.Status = EOrderStatus.Delivered;

        await orderRepository.UpdateAsync(order);
    }
}
