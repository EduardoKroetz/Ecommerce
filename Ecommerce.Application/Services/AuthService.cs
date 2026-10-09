using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces.Infra;

namespace Ecommerce.Application.Services;

public class AuthService(IIdentityService identityService, ITokenService tokenService)
{
    public async Task<LoginUserResponse> LoginAsync(LoginUserRequest request)
    {
        var user = await identityService.AuthenticateAsync(request.Email, request.Password);

        var token = tokenService.GenerateAccessToken(user);

        return new LoginUserResponse
        {
            Token = token
        };
    }

    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request)
    {
        var user = await identityService.CreateUserAsync(request.Email, request.Password);

        var token = tokenService.GenerateAccessToken(user);

        return new RegisterUserResponse
        {
            UserId = user.Id,
            Token = token
        };
    }
}
