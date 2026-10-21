using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    ICustomerRepository customerRepository,
    ISellerRepository sellerRepository,
    CustomerService customerService,
    SellerService sellerService,
    IUnitOfWork uow)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await identityService.AuthenticateAsync(request.Email, request.Password);

        // Every user has a customer (ADR 0012); a missing one is a broken invariant, not a client error
        var customer = await customerRepository.GetByUserIdAsync(user.Id)
            ?? throw new InvalidOperationException($"User '{user.Id}' has no customer.");

        var seller = await sellerRepository.GetByUserIdAsync(user.Id);

        var token = tokenService.GenerateAccessToken(user, customer.Id, seller?.Id);

        return new LoginResponse
        {
            Token = token
        };
    }

    public async Task<RegisterCustomerResponse> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        await using var transaction = await uow.BeginTransactionAsync();

        var (user, customer) = await CreateUserAsync(request.Name, request.Email, request.Password);

        await transaction.CommitAsync();

        var token = tokenService.GenerateAccessToken(user, customer.Id, null);

        return new RegisterCustomerResponse
        {
            UserId = user.Id,
            CustomerId = customer.Id,
            Token = token
        };
    }

    public async Task<RegisterSellerResponse> RegisterSellerAsync(RegisterSellerRequest request)
    {
        await using var transaction = await uow.BeginTransactionAsync();

        var (user, customer) = await CreateUserAsync(request.Name, request.Email, request.Password);

        var seller = await sellerService.CreateAsync(user.Id);

        await transaction.CommitAsync();

        var token = tokenService.GenerateAccessToken(user, customer.Id, seller.Id);

        return new RegisterSellerResponse
        {
            UserId = user.Id,
            CustomerId = customer.Id,
            SellerId = seller.Id,
            Token = token
        };
    }

    // Single creation point for users: every user gets its customer. Must run inside the caller's transaction.
    private async Task<(IUser User, Customer Customer)> CreateUserAsync(string name, string email, string password)
    {
        var user = await identityService.CreateUserAsync(name, email, password);

        var customer = await customerService.CreateAsync(user.Id);

        return (user, customer);
    }
}
