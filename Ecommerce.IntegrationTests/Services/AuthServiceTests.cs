using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Infra.Identity;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class AuthServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task RegisterCustomer_CreatesUserAndCustomer()
    {
        var customer = await TestData.CustomerAsync(email: "customer@test.com");

        var login = await Services.Get<AuthService>().LoginAsync(new LoginRequest { Email = "customer@test.com", Password = TestData.DefaultPassword });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
        Assert.Equal(customer.CustomerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
    }

    [Fact]
    public async Task RegisterCustomer_IssuesTokenForCreatedCustomer()
    {
        var customer = await TestData.CustomerAsync();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(customer.Token);

        Assert.Equal(customer.UserId, token.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(customer.CustomerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
        Assert.DoesNotContain(token.Claims, c => c.Type == AppClaimTypes.SellerId);
    }

    [Fact]
    public async Task RegisterCustomer_WithDuplicatedEmail_IsInvalid()
    {
        await TestData.CustomerAsync(email: "customer@test.com");

        await Assert.ThrowsAsync<ValidationException>(() => TestData.CustomerAsync(email: "customer@test.com"));

        var customerCount = await QueryAsync(db => db.Customers.CountAsync(c => db.Users.Any(u => u.Id == c.UserId && u.Email == "customer@test.com")));
        Assert.Equal(1, customerCount);
    }

    [Fact]
    public async Task RegisterCustomer_WithWeakPassword_IsInvalid()
    {
        await Assert.ThrowsAsync<ValidationException>(() => TestData.CustomerAsync(email: "customer@test.com", password: "ABC"));

        var userExists = await QueryAsync(db => db.Users.AnyAsync(u => u.Email == "customer@test.com"));
        Assert.False(userExists);
    }

    [Fact]
    public async Task RegisterSeller_CreatesUserCustomerAndSeller()
    {
        var seller = await TestData.SellerAsync(email: "seller@test.com");

        var login = await Services.Get<AuthService>().LoginAsync(new LoginRequest { Email = "seller@test.com", Password = TestData.DefaultPassword });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
        Assert.Equal(seller.CustomerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
        Assert.Equal(seller.SellerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.SellerId).Value);
    }

    [Fact]
    public async Task RegisterSeller_IssuesTokenWithSellerClaim()
    {
        var seller = await TestData.SellerAsync();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(seller.Token);

        Assert.Equal(seller.CustomerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
        Assert.Equal(seller.SellerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.SellerId).Value);
    }

    [Fact]
    public async Task RegisterSeller_WithDuplicatedEmail_IsInvalid()
    {
        await TestData.CustomerAsync(email: "user@test.com");

        await Assert.ThrowsAsync<ValidationException>(() => TestData.SellerAsync(email: "user@test.com"));

        var sellerExists = await QueryAsync(db => db.Sellers.AnyAsync(s => db.Users.Any(u => u.Id == s.UserId && u.Email == "user@test.com")));
        Assert.False(sellerExists);
    }

    [Fact]
    public async Task Login_WithWrongPassword_IsUnauthorized()
    {
        await TestData.CustomerAsync(email: "customer@test.com");
        var service = Services.Get<AuthService>();

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequest { Email = "customer@test.com", Password = "wrong-password" }));
    }

    [Fact]
    public async Task Login_WithUnknownEmail_IsUnauthorized()
    {
        var service = Services.Get<AuthService>();

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequest { Email = "unknown@test.com", Password = TestData.DefaultPassword }));
    }

    [Fact]
    public async Task Login_WithEmailInDifferentCase_Authenticates()
    {
        var customer = await TestData.CustomerAsync(email: "customer@test.com");

        var login = await Services.Get<AuthService>().LoginAsync(new LoginRequest { Email = "CUSTOMER@TEST.COM", Password = TestData.DefaultPassword });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
        Assert.Equal(customer.CustomerId.ToString(), token.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
    }

    [Fact]
    public async Task Login_IssuesTokenExpiringAfterConfiguredMinutes()
    {
        await TestData.CustomerAsync(email: "customer@test.com");

        var login = await Services.Get<AuthService>().LoginAsync(new LoginRequest { Email = "customer@test.com", Password = TestData.DefaultPassword });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
        Assert.Equal(Now.UtcDateTime.AddMinutes(60), token.ValidTo);
    }
}
