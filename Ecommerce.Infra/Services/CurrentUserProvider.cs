using System.Security.Claims;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Infra.Identity;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Infra.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public string UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedException("User not found");

    public int? SellerId =>
        int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(AppClaimTypes.SellerId), out var sellerId)
            ? sellerId
            : null;
}
