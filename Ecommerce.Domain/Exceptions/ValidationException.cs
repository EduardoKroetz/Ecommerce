namespace Ecommerce.Domain.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyCollection<ValidationError>? Errors { get; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = [.. errors];
    }
}

public class ValidationError
{
    public string Code { get; }
    public string Message { get; }

    public ValidationError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}