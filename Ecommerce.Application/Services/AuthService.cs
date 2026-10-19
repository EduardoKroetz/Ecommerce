using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;

namespace Ecommerce.Application.Services;

public class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    ISellerRepository sellerRepository,
    SellerService sellerService,
    IUnitOfWork uow)
{
    public async Task<LoginUserResponse> LoginAsync(LoginUserRequest request)
    {
        var user = await identityService.AuthenticateAsync(request.Email, request.Password);

        var seller = await sellerRepository.GetByUserIdAsync(user.Id);

        var token = tokenService.GenerateAccessToken(user, seller?.Id);

        return new LoginUserResponse
        {
            Token = token
        };
    }

    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request)
    {
        var user = await identityService.CreateUserAsync(request.Name, request.Email, request.Password);

        var token = tokenService.GenerateAccessToken(user, null);

        return new RegisterUserResponse
        {
            UserId = user.Id,
            Token = token
        };
    }

    public async Task<RegisterSellerResponse> RegisterSellerAsync(RegisterSellerRequest request)
    {
        await using var transaction = await uow.BeginTransactionAsync();

        var user = await identityService.CreateUserAsync(request.Name, request.Email, request.Password);

        var seller = await sellerService.CreateAsync(user.Id);

        await transaction.CommitAsync();

        var token = tokenService.GenerateAccessToken(user, seller.Id);

        return new RegisterSellerResponse
        {
            UserId = user.Id,
            SellerId = seller.Id,
            Token = token
        };
    }
}
