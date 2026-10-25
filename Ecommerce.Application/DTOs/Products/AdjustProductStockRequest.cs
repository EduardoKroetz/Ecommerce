namespace Ecommerce.Application.DTOs.Products;

public class AdjustProductStockRequest
{
    // Signed: positive adds stock, negative removes it
    public int Quantity { get; set; }
}
