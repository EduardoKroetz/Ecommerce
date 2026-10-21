using Microsoft.AspNetCore.Http;

namespace Ecommerce.Infra.Services;

public class CancellationTokenProvider(IHttpContextAccessor httpContextAccessor) : ICancellationTokenProvider
{
    // Cancelled when the client aborts the request; outside an HTTP request (jobs, tests) there is nothing to cancel
    public CancellationToken Token => httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
}
