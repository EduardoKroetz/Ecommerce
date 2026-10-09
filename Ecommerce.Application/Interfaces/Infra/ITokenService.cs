using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Infra;

public interface ITokenService
{
    string GenerateAccessToken(IUser user);
}