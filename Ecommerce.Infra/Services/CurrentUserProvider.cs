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

    // Every authenticated user has a customer (ADR 0012): a missing claim is a broken invariant (500), never a 403
    public int CustomerId =>
        int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(AppClaimTypes.CustomerId), out var customerId)
            ? customerId
            : throw new InvalidOperationException($"The '{AppClaimTypes.CustomerId}' claim is missing from the authenticated user.");

    public int? SellerId =>
        int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(AppClaimTypes.SellerId), out var sellerId)
            ? sellerId
            : null;
}
