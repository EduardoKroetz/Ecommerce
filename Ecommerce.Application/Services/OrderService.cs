using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserProvider userProvider,
    IUnitOfWork uof)
{
    public async Task<PagedResult<GetOrder>> SearchAsync(PaginationQuery query)
    {
        return await orderRepository.SearchByUserIdAsync(userProvider.UserId, query);
    }

    public async Task<GetOrder?> GetByIdAsync(int orderId)
    {
        return await orderRepository.GetDetailByIdAndUserIdAsync(orderId, userProvider.UserId) ?? throw new NotFoundException("Order not found.");
    }

    public async Task<GetOrder> CreateAsync()
    {
        var userId = userProvider.UserId;

        var cart = await cartRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("Cart not found.");

        if (cart.Items.Count == 0)
            throw new DomainException(EDomainExceptionCode.CartEmpty, "Cart cannot be empty.");

        var itemsWithInsufficientStock = cart.Items
            .Where(ci => ci.Quantity > ci.Product.StockBalance)
            .Select(ci => new { ci.ProductId, ci.Product.Name, ci.Product.StockBalance, ci.Quantity })
            .ToList();

        if (itemsWithInsufficientStock.Count > 0)
            throw new DomainException(EDomainExceptionCode.InsufficientStock, "Some items in your cart have insufficient stock.", new { items = itemsWithInsufficientStock });

        var orderItems = cart.Items.Select(ci => new OrderItem
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            UnitPrice = ci.Product.Price,
            TotalPrice = ci.Product.Price * ci.Quantity,
        }).ToList();

        var order = new Order
        {
            UserId = userId,
            Status = EOrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = orderItems.Sum(oi => oi.TotalPrice),
            Items = orderItems
        };

        await using var transaction = await uof.BeginTransactionAsync();

        foreach (var item in orderItems)
        {
            var stockDecrementResult = await productRepository.TryDecrementStockAsync(item.ProductId, item.Quantity);

            if (!stockDecrementResult)
            {
                await transaction.RollbackAsync();

                throw new DomainException(EDomainExceptionCode.InsufficientStock, $"Product '{item.ProductName}' no longer has enough stock.");
            }
        }

        await orderRepository.AddAsync(order);

        cart.Items.Clear();
        await cartRepository.UpdateAsync(cart);

        await transaction.CommitAsync();

        return GetOrder.Map(order);
    }

    public async Task CancelAsync(int orderId)
    {
        var userId = userProvider.UserId;

        var order = await orderRepository.GetByIdAndUserIdAsync(orderId, userId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status == EOrderStatus.Cancelled)
            throw new DomainException(EDomainExceptionCode.OrderAlreadyCancelled, "Order is already cancelled.");

        order.Status = EOrderStatus.Cancelled;

        order.Items.ForEach(oi => oi.Product.StockBalance += oi.Quantity);

        await orderRepository.UpdateAsync(order);
    }
}
