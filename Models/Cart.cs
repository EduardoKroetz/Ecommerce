namespace Ecommerce.Models;

public class Cart
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required DateTime CreatedAt { get; set; }
}
