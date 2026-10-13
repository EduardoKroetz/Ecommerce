using Ecommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Middlewares;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            DomainException domainException => CreateProblemDetails(StatusCodes.Status422UnprocessableEntity, domainException.Message, new Dictionary<string, object?>
            {
                ["code"] = domainException.Code,
                ["details"] = domainException.Details
            }),
            ValidationException validationException => CreateProblemDetails(StatusCodes.Status400BadRequest, validationException.Message, new Dictionary<string, object?>
            {
                ["errors"] = validationException.Errors?.Select(e => new { code = e.Code, message = e.Message })
            }),
            NotFoundException => CreateProblemDetails(StatusCodes.Status404NotFound, exception.Message),
            ConflictException => CreateProblemDetails(StatusCodes.Status409Conflict, exception.Message),
            UnauthorizedException => CreateProblemDetails(StatusCodes.Status401Unauthorized, exception.Message),
            _ => null
        };

        // Unmapped exceptions fall through to the default handler (500 ProblemDetails)
        if (problemDetails is null)
            return false;

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails CreateProblemDetails(int statusCode, string title, IDictionary<string, object?>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
                problemDetails.Extensions[key] = value;
        }

        return problemDetails;
    }
}
