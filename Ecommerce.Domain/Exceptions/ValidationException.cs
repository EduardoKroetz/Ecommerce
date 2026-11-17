namespace Ecommerce.Domain.Exceptions;

// Invalid input caught by the application (e.g. Identity rules), grouped by field: the same shape as the
// automatic model validation of the API, so clients read a single 400 format
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
