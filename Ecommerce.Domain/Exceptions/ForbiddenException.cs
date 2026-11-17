namespace Ecommerce.Domain.Exceptions;

// Authenticated, but without the role the operation needs (403). Not authenticated is UnauthorizedException (401)
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
