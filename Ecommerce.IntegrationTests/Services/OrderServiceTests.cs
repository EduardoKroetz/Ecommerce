using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class OrderServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Search_ReturnsOnlyCurrentCustomerOrders()
    {
        var otherCustomer = await TestData.CustomerAsync();
        var purchase = await TestData.PurchaseAsync();
        await TestData.PurchaseAsync(otherCustomer);

        var result = await Services.Get<OrderService>().SearchAsync(new PaginationQuery());

        Assert.Equal(purchase.Orders.Single().Id, Assert.Single(result.Data).Id);
    }

    [Fact]
    public async Task Search_OrdersByNewestFirst()
    {
        var oldest = await TestData.PurchaseAsync();
        Time.Advance(TimeSpan.FromMinutes(1));
        var newest = await TestData.PurchaseAsync();

        var result = await Services.Get<OrderService>().SearchAsync(new PaginationQuery());

        Assert.Equal([newest.Orders.Single().Id, oldest.Orders.Single().Id], result.Data.Select(o => o.Id));
    }

    [Fact]
    public async Task SearchSellerOrders_ReturnsOnlySellerOrders()
    {
        var otherSeller = await TestData.SellerAsync();
        var purchase = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(), await TestData.ProductAsync(otherSeller)]);

        var result = await Services.Get<OrderService>(Seller).SearchSellerOrdersAsync(new PaginationQuery());

        var order = Assert.Single(result.Data);
        Assert.Equal(purchase.Orders.Single(o => o.SellerId == Seller.SellerId).Id, order.Id);
    }

    [Fact]
    public async Task SearchSellerOrders_WithoutSeller_IsForbidden()
    {
        var service = Services.Get<OrderService>(Customer);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.SearchSellerOrdersAsync(new PaginationQuery()));
    }

    [Fact]
    public async Task GetById_AsCustomer_ReturnsOrder()
    {
        var purchase = await TestData.PurchaseAsync();
        var orderId = purchase.Orders.Single().Id;

        var order = await Services.Get<OrderService>(Customer).GetByIdAsync(orderId);

        Assert.Equal(purchase.Id, order.PurchaseId);
    }

    [Fact]
    public async Task GetById_AsSeller_ReturnsOrder()
    {
        var purchase = await TestData.PurchaseAsync();
        var orderId = purchase.Orders.Single().Id;

        var order = await Services.Get<OrderService>(Seller).GetByIdAsync(orderId);

        Assert.Equal(Seller.SellerId, order.SellerId);
    }

    [Fact]
    public async Task GetById_AsUnrelatedUser_IsNotFound()
    {
        var purchase = await TestData.PurchaseAsync();
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<OrderService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(purchase.Orders.Single().Id));
    }

    [Fact]
    public async Task Cancel_AsCustomer_PersistsCancellation()
    {
        var purchase = await TestData.PurchaseAsync();
        var orderId = purchase.Orders.Single().Id;

        await Services.Get<OrderService>(Customer).CancelAsync(orderId);

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Cancel_AsSeller_PersistsCancellation()
    {
        var purchase = await TestData.PurchaseAsync();
        var orderId = purchase.Orders.Single().Id;

        await Services.Get<OrderService>(Seller).CancelAsync(orderId);

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Cancel_RestoresStock()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var purchase = await TestData.PurchaseAsync(products: [product]);

        await Services.Get<OrderService>().CancelAsync(purchase.Orders.Single().Id);

        var restored = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(10, restored.StockBalance);
    }

    [Fact]
    public async Task Cancel_WithOtherActiveOrders_RecalculatesPurchaseTotal()
    {
        var otherSeller = await TestData.SellerAsync();
        var purchase = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(price: 40.0m), await TestData.ProductAsync(otherSeller, price: 30.0m)]);

        await Services.Get<OrderService>().CancelAsync(purchase.Orders.Single(o => o.SellerId == Seller.SellerId).Id);

        var updated = await Services.Get<PurchaseService>().GetByIdAsync(purchase.Id);
        Assert.Equal(30.0m, updated.TotalAmount);
        Assert.Equal(70.0m, updated.OriginalTotalAmount);
        Assert.Equal(EPurchaseStatus.Pending, updated.Status);
    }

    [Fact]
    public async Task Cancel_WhenLastActiveOrder_CancelsPurchase()
    {
        var purchase = await TestData.PurchaseAsync();

        await Services.Get<OrderService>().CancelAsync(purchase.Orders.Single().Id);

        var updated = await Services.Get<PurchaseService>().GetByIdAsync(purchase.Id);
        Assert.Equal(EPurchaseStatus.Cancelled, updated.Status);
        Assert.Equal(0.0m, updated.TotalAmount);
    }

    [Fact]
    public async Task Cancel_AsUnrelatedUser_IsNotFound()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var purchase = await TestData.PurchaseAsync(products: [product]);
        var otherCustomer = await TestData.CustomerAsync();
        var service = Services.Get<OrderService>(otherCustomer);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CancelAsync(purchase.Orders.Single().Id));

        var unchanged = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, unchanged.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenPaid_IsRejected()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var purchase = await TestData.PaidPurchaseAsync(products: [product]);
        var orderId = purchase.Orders.Single().Id;
        var service = Services.Get<OrderService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CancelAsync(orderId));
        Assert.Equal(EDomainExceptionCode.InvalidOrderStatus, exception.Code);

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.AwaitingShipment, order.Status);
        var unchanged = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, unchanged.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenCancelledConcurrently_IsConcurrencyConflict()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var purchase = await TestData.PurchaseAsync(products: [product]);
        var orderId = purchase.Orders.Single().Id;
        BeforeNextWrite(() => Services.Get<OrderService>(Seller).CancelAsync(orderId));
        var service = Services.Get<OrderService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CancelAsync(orderId));

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Cancelled, order.Status);
        var restored = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(10, restored.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenPaidConcurrently_IsConcurrencyConflict()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var purchase = await TestData.PurchaseAsync(products: [product]);
        var orderId = purchase.Orders.Single().Id;
        BeforeNextWrite(() => Services.Get<PurchaseService>().PayAsync(purchase.Id));
        var service = Services.Get<OrderService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CancelAsync(orderId));

        var paid = await Services.Get<PurchaseService>().GetByIdAsync(purchase.Id);
        Assert.Equal(EPurchaseStatus.Paid, paid.Status);
        Assert.Equal(EOrderStatus.AwaitingShipment, paid.Orders.Single().Status);
        var unchanged = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, unchanged.StockBalance);
    }

    // Both cancellations recalculate the same purchase: the losing one conflicts, and its retry sees the sibling cancelled
    [Fact]
    public async Task Cancel_WhenSiblingOrderCancelledConcurrently_CancelsPurchaseOnRetry()
    {
        var otherSeller = await TestData.SellerAsync();
        var purchase = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(), await TestData.ProductAsync(otherSeller)]);
        var orderId = purchase.Orders.Single(o => o.SellerId == Seller.SellerId).Id;
        var siblingOrderId = purchase.Orders.Single(o => o.Id != orderId).Id;
        BeforeNextWrite(() => Services.Get<OrderService>().CancelAsync(siblingOrderId));
        var service = Services.Get<OrderService>();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CancelAsync(orderId));

        await Services.Get<OrderService>().CancelAsync(orderId);

        var cancelled = await Services.Get<PurchaseService>().GetByIdAsync(purchase.Id);
        Assert.Equal(EPurchaseStatus.Cancelled, cancelled.Status);
        Assert.Equal(0.0m, cancelled.TotalAmount);
    }

    [Fact]
    public async Task Ship_PersistsShipment()
    {
        var purchase = await TestData.PaidPurchaseAsync();
        var orderId = purchase.Orders.Single().Id;

        await Services.Get<OrderService>(Seller).ShipAsync(orderId);

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Shipped, order.Status);
    }

    [Fact]
    public async Task Ship_WithOrderFromAnotherSeller_IsNotFound()
    {
        var purchase = await TestData.PaidPurchaseAsync();
        var orderId = purchase.Orders.Single().Id;
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<OrderService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ShipAsync(orderId));

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.AwaitingShipment, order.Status);
    }

    [Fact]
    public async Task Ship_WithoutSeller_IsForbidden()
    {
        var purchase = await TestData.PaidPurchaseAsync();
        var service = Services.Get<OrderService>(Customer);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.ShipAsync(purchase.Orders.Single().Id));
    }

    [Fact]
    public async Task ConfirmDelivery_PersistsDelivery()
    {
        var purchase = await TestData.PaidPurchaseAsync();
        var orderId = purchase.Orders.Single().Id;
        await Services.Get<OrderService>(Seller).ShipAsync(orderId);

        await Services.Get<OrderService>().ConfirmDeliveryAsync(orderId);

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Delivered, order.Status);
    }

    [Fact]
    public async Task ConfirmDelivery_AsUnrelatedCustomer_IsNotFound()
    {
        var purchase = await TestData.PaidPurchaseAsync();
        var orderId = purchase.Orders.Single().Id;
        await Services.Get<OrderService>(Seller).ShipAsync(orderId);
        var otherCustomer = await TestData.CustomerAsync();
        var service = Services.Get<OrderService>(otherCustomer);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ConfirmDeliveryAsync(orderId));

        var order = await Services.Get<OrderService>().GetByIdAsync(orderId);
        Assert.Equal(EOrderStatus.Shipped, order.Status);
    }
}
