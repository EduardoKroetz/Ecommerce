namespace Ecommerce.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; set; }
    public object? Details { get; set; }

    public DomainException(string code, string message, object? details = null) : base(message)
    {
        Code = code;
        Details = details;
    }
}

