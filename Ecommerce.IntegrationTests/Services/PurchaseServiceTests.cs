using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class PurchaseServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Search_ReturnsOnlyCurrentCustomerPurchases()
    {
        var otherCustomer = await TestData.CustomerAsync();
        var purchase = await TestData.PurchaseAsync();
        await TestData.PurchaseAsync(otherCustomer);

        var result = await Services.Get<PurchaseService>().SearchAsync(new PaginationQuery());

        Assert.Equal(purchase.Id, Assert.Single(result.Data).Id);
    }

    [Fact]
    public async Task Search_OrdersByNewestFirst()
    {
        var oldest = await TestData.PurchaseAsync();
        Time.Advance(TimeSpan.FromMinutes(1));
        var newest = await TestData.PurchaseAsync();

        var result = await Services.Get<PurchaseService>().SearchAsync(new PaginationQuery());

        Assert.Equal([newest.Id, oldest.Id], result.Data.Select(p => p.Id));
    }

    [Fact]
    public async Task GetById_WithPurchaseFromAnotherCustomer_IsNotFound()
    {
        var purchase = await TestData.PurchaseAsync();
        var otherCustomer = await TestData.CustomerAsync();
        var service = Services.Get<PurchaseService>(otherCustomer);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(purchase.Id));
    }

    [Fact]
    public async Task Create_PersistsPurchaseWithTotalsAndPaymentMethod()
    {
        var products = new[] { await TestData.ProductAsync(price: 40.0m), await TestData.ProductAsync(price: 30.0m) };

        var created = await TestData.PurchaseAsync(products: products, paymentMethod: EPaymentMethod.Boleto);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
        Assert.Equal(EPaymentMethod.Boleto, purchase.PaymentMethod);
        Assert.Equal(70.0m, purchase.TotalAmount);
        Assert.Equal(70.0m, purchase.OriginalTotalAmount);
        Assert.Equal(Now.UtcDateTime, purchase.CreatedAt);
    }

    [Fact]
    public async Task Create_SplitsOrdersBySeller()
    {
        var otherSeller = await TestData.SellerAsync();
        var products = new[]
        {
            await TestData.ProductAsync(),
            await TestData.ProductAsync(),
            await TestData.ProductAsync(otherSeller)
        };

        var created = await TestData.PurchaseAsync(products: products);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(2, purchase.Orders.Count);
        Assert.Equal(2, purchase.Orders.Single(o => o.SellerId == Seller.SellerId).Items.Count);
        Assert.Single(purchase.Orders.Single(o => o.SellerId == otherSeller.SellerId).Items);
        Assert.All(purchase.Orders, o => Assert.Equal(EOrderStatus.AwaitingPayment, o.Status));
    }

    [Fact]
    public async Task Create_DecrementsStock()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        await TestData.CartItemAsync(product: product, quantity: 3);

        await Services.Get<PurchaseService>().CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX });

        var updated = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(7, updated.StockBalance);
    }

    [Fact]
    public async Task Create_ClearsCart()
    {
        await TestData.PurchaseAsync();

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task Create_FreezesItemPricesAgainstLaterProductChanges()
    {
        var product = await TestData.ProductAsync(price: 10.0m);
        var created = await TestData.PurchaseAsync(products: [product]);
        await Services.Get<ProductService>(Seller).UpdateAsync(product.Id, new UpdateProductRequest { Name = product.Name, Price = 50.0m });

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);

        Assert.Equal(10.0m, Assert.Single(purchase.Orders.Single().Items).UnitPrice);
        Assert.Equal(10.0m, purchase.TotalAmount);
    }

    [Fact]
    public async Task Create_WithoutCart_IsNotFound()
    {
        var service = Services.Get<PurchaseService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));
    }

    [Fact]
    public async Task Create_WithInsufficientStock_IsRejected()
    {
        var product = await TestData.ProductAsync(stockBalance: 1);
        await TestData.CartItemAsync(product: product, quantity: 2);
        var service = Services.Get<PurchaseService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));
        Assert.Equal(EDomainExceptionCode.InsufficientStock, exception.Code);

        var stock = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(1, stock.StockBalance);
        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
        var purchases = await Services.Get<PurchaseService>().SearchAsync(new PaginationQuery());
        Assert.Equal(0, purchases.TotalCount);
    }

    [Fact]
    public async Task Create_WhenLastItemSoldConcurrently_IsRejected()
    {
        var otherCustomer = await TestData.CustomerAsync();
        var product = await TestData.ProductAsync(stockBalance: 1);
        await TestData.CartItemAsync(product: product);
        await TestData.CartItemAsync(otherCustomer, product);
        BeforeNextWrite(() => Services.Get<PurchaseService>(otherCustomer).CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));
        var service = Services.Get<PurchaseService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));
        Assert.Equal(EDomainExceptionCode.InsufficientStock, exception.Code);

        var purchases = await Services.Get<PurchaseService>().SearchAsync(new PaginationQuery());
        Assert.Equal(0, purchases.TotalCount);
        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Single(cart.Items);
        var stock = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(0, stock.StockBalance);
    }

    // The concurrent checkout clears the cart first: deleting its items again affects no rows
    [Fact]
    public async Task Create_WhenSameCartCheckedOutConcurrently_IsConcurrencyConflict()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        await TestData.CartItemAsync(product: product);
        BeforeNextWrite(() => Services.Get<PurchaseService>().CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));
        var service = Services.Get<PurchaseService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CreateAsync(new CreatePurchaseRequest { PaymentMethod = EPaymentMethod.PIX }));

        var purchases = await Services.Get<PurchaseService>().SearchAsync(new PaginationQuery());
        Assert.Equal(1, purchases.TotalCount);
        var stock = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, stock.StockBalance);
    }

    [Fact]
    public async Task Pay_PersistsPaymentAndMarksOrdersAsPaid()
    {
        var otherSeller = await TestData.SellerAsync();
        var created = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(), await TestData.ProductAsync(otherSeller)]);

        await Services.Get<PurchaseService>().PayAsync(created.Id);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
        Assert.All(purchase.Orders, o => Assert.Equal(EOrderStatus.AwaitingShipment, o.Status));
    }

    [Fact]
    public async Task Pay_WithCancelledOrder_KeepsItCancelled()
    {
        var otherSeller = await TestData.SellerAsync();
        var created = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(), await TestData.ProductAsync(otherSeller)]);
        var cancelledOrderId = created.Orders.Single(o => o.SellerId == Seller.SellerId).Id;
        await Services.Get<OrderService>().CancelAsync(cancelledOrderId);

        await Services.Get<PurchaseService>().PayAsync(created.Id);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EOrderStatus.Cancelled, purchase.Orders.Single(o => o.Id == cancelledOrderId).Status);
        Assert.Equal(EOrderStatus.AwaitingShipment, purchase.Orders.Single(o => o.Id != cancelledOrderId).Status);
    }

    [Fact]
    public async Task Pay_WithPurchaseFromAnotherCustomer_IsNotFound()
    {
        var created = await TestData.PurchaseAsync();
        var otherCustomer = await TestData.CustomerAsync();
        var service = Services.Get<PurchaseService>(otherCustomer);

        await Assert.ThrowsAsync<NotFoundException>(() => service.PayAsync(created.Id));

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }

    [Fact]
    public async Task Pay_WhenOrderCancelledConcurrently_IsConcurrencyConflict()
    {
        var otherSeller = await TestData.SellerAsync();
        var created = await TestData.PurchaseAsync(products: [await TestData.ProductAsync(price: 40.0m), await TestData.ProductAsync(otherSeller, price: 30.0m)]);
        var cancelledOrderId = created.Orders.Single(o => o.SellerId == Seller.SellerId).Id;
        BeforeNextWrite(() => Services.Get<OrderService>().CancelAsync(cancelledOrderId));
        var service = Services.Get<PurchaseService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.PayAsync(created.Id));

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
        Assert.Equal(30.0m, purchase.TotalAmount);
        Assert.Equal(EOrderStatus.Cancelled, purchase.Orders.Single(o => o.Id == cancelledOrderId).Status);
        Assert.Equal(EOrderStatus.AwaitingPayment, purchase.Orders.Single(o => o.Id != cancelledOrderId).Status);
    }

    [Fact]
    public async Task Cancel_CancelsOrdersAndRestoresStock()
    {
        var otherSeller = await TestData.SellerAsync();
        var products = new[] { await TestData.ProductAsync(stockBalance: 10), await TestData.ProductAsync(otherSeller, stockBalance: 10) };
        var created = await TestData.PurchaseAsync(products: products);

        await Services.Get<PurchaseService>().CancelAsync(created.Id);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
        Assert.Equal(0.0m, purchase.TotalAmount);
        Assert.All(purchase.Orders, o => Assert.Equal(EOrderStatus.Cancelled, o.Status));
        foreach (var product in products)
            Assert.Equal(10, (await Services.Get<ProductService>().GetByIdAsync(product.Id)).StockBalance);
    }

    [Fact]
    public async Task Cancel_WithCancelledOrder_RestoresItsStockOnlyOnce()
    {
        var otherSeller = await TestData.SellerAsync();
        var product = await TestData.ProductAsync(stockBalance: 10);
        var created = await TestData.PurchaseAsync(products: [product, await TestData.ProductAsync(otherSeller)]);
        await Services.Get<OrderService>().CancelAsync(created.Orders.Single(o => o.SellerId == Seller.SellerId).Id);

        await Services.Get<PurchaseService>().CancelAsync(created.Id);

        var updated = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(10, updated.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenPaid_IsRejected()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var created = await TestData.PaidPurchaseAsync(products: [product]);
        var service = Services.Get<PurchaseService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CancelAsync(created.Id));
        Assert.Equal(EDomainExceptionCode.InvalidPurchaseStatus, exception.Code);

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
        Assert.Equal(EOrderStatus.AwaitingShipment, purchase.Orders.Single().Status);
        var unchanged = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, unchanged.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenPaidConcurrently_IsConcurrencyConflict()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var created = await TestData.PurchaseAsync(products: [product]);
        BeforeNextWrite(() => Services.Get<PurchaseService>().PayAsync(created.Id));
        var service = Services.Get<PurchaseService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CancelAsync(created.Id));

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Paid, purchase.Status);
        Assert.Equal(EOrderStatus.AwaitingShipment, purchase.Orders.Single().Status);
        var unchanged = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(9, unchanged.StockBalance);
    }

    [Fact]
    public async Task Cancel_WhenCancelledConcurrently_IsConcurrencyConflict()
    {
        var product = await TestData.ProductAsync(stockBalance: 10);
        var created = await TestData.PurchaseAsync(products: [product]);
        BeforeNextWrite(() => Services.Get<PurchaseService>().CancelAsync(created.Id));
        var service = Services.Get<PurchaseService>();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.CancelAsync(created.Id));

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Cancelled, purchase.Status);
        var restored = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Equal(10, restored.StockBalance);
    }

    [Fact]
    public async Task Cancel_WithPurchaseFromAnotherCustomer_IsNotFound()
    {
        var created = await TestData.PurchaseAsync();
        var otherCustomer = await TestData.CustomerAsync();
        var service = Services.Get<PurchaseService>(otherCustomer);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CancelAsync(created.Id));

        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(created.Id);
        Assert.Equal(EPurchaseStatus.Pending, purchase.Status);
    }
}
