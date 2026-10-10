namespace Ecommerce.Application.Interfaces.Infra;

public interface ICurrentUserProvider
{
    string UserId { get; }
}
