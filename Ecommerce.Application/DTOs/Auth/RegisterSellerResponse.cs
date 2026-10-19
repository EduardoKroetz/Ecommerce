namespace Ecommerce.Application.DTOs.Auth;

public class RegisterSellerResponse
{
    public required string UserId { get; set; }
    public required int SellerId { get; set; }
    public required string Token { get; set; }
}
