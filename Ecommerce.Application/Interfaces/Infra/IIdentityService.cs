using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Infra;

public interface IIdentityService
{
    Task<IUser> CreateUserAsync(string email, string password);
    Task<IUser> AuthenticateAsync(string email, string password);
}
