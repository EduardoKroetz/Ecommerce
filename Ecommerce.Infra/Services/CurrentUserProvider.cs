using Ecommerce.Application.Interfaces.Infra;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Infra.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public string GetCurrentUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;

        return httpContext?.User?.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User not found");
    }
}
