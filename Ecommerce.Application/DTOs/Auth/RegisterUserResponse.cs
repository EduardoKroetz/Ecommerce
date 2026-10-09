namespace Ecommerce.Application.DTOs.Auth;

public class RegisterUserResponse
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
}