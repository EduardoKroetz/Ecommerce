using System.Security.Claims;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Infra.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public string UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedException("User not found");
}
