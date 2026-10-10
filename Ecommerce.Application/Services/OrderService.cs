using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, ICurrentUserProvider userProvider)
{
    public async Task<GetOrder> CreateAsync()
    {
        var userId = userProvider.UserId;

        var cart = await cartRepository.GetByUserIdAsync(userId) ?? throw new NotFoundException("Cart not found.");

        if (cart.Items.Count == 0)
        {
            throw new DomainException("Cart cannot be empty.");
        }

        var itemsWithInsufficientStock = cart.Items
            .Where(ci => ci.Quantity > ci.Product.StockBalance)
            .Select(ci => new { ci.ProductId, ci.Product.Name, ci.Product.StockBalance, ci.Quantity })
            .ToList();

        if (itemsWithInsufficientStock.Count > 0)
        {
            return Problem(
                title: "Insufficient stock",
                detail: "Some items in your cart have insufficient stock.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["items"] = itemsWithInsufficientStock
                });
        }

        var orderItems = cartItems.Select(ci => new OrderItem
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

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        foreach (var item in orderItems)
        {
            // Update the stock balance of the product with atomic operation to avoid concurrency issues
            var updatedRows = await dbContext.Products
                .Where(p => p.Id == item.ProductId && p.StockBalance >= item.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockBalance, p => p.StockBalance - item.Quantity));

            if (updatedRows == 0)
            {
                await transaction.RollbackAsync();

                return Problem(
                    title: "Insufficient stock",
                    detail: $"Product '{item.ProductName}' no longer has enough stock.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        await dbContext.Orders.AddAsync(order);
        dbContext.CartItems.RemoveRange(cartItems);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
