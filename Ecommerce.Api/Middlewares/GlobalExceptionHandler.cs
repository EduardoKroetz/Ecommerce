using Ecommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Middlewares;

// Turns every exception into a ProblemDetails, so all error responses go through the same writer and JSON options
public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
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
            // Same shape as the automatic model validation ([ApiController]): errors grouped by field
            ValidationException validationException => new ValidationProblemDetails(new Dictionary<string, string[]>(validationException.Errors))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = validationException.Message
            },
            NotFoundException => CreateProblemDetails(StatusCodes.Status404NotFound, exception.Message),
            ConflictException => CreateProblemDetails(StatusCodes.Status409Conflict, exception.Message),
            DbUpdateConcurrencyException => CreateProblemDetails(StatusCodes.Status409Conflict, "The resource was changed by another request. Reload it and try again."),
            UnauthorizedException => CreateProblemDetails(StatusCodes.Status401Unauthorized, exception.Message),
            ForbiddenException => CreateProblemDetails(StatusCodes.Status403Forbidden, exception.Message),
            // Unmapped: a bug or an infrastructure failure. Generic title, no exception details for the client.
            // Not left to the default handler: its MVC writer writes nothing when [ApiController] is set on the assembly
            _ => CreateProblemDetails(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        // A handled exception is no longer logged by the middleware (.NET 10): the expected ones (4xx) stay out of
        // the log, the unexpected ones are logged here
        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

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
