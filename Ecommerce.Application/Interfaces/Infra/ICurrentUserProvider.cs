namespace Ecommerce.Application.Interfaces.Infra;

public interface ICurrentUserProvider
{
    string UserId { get; }
    int CustomerId { get; }
    int? SellerId { get; }
}
