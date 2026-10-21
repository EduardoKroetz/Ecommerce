namespace Ecommerce.Application.DTOs.Auth;

public class RegisterCustomerResponse
{
    public required string UserId { get; set; }
    public required int CustomerId { get; set; }
    public required string Token { get; set; }
}