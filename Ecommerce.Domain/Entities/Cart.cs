namespace Ecommerce.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required DateTime CreatedAt { get; set; }

    public List<CartItem> Items { get; set; } = [];
}
