namespace Ecommerce.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; set; }
    public object? Details { get; set; }

    public DomainException(EDomainExceptionCode code, string message, object? details = null) : base(message)
    {
        Code = code.ToString();
        Details = details;
    }
}

public enum EDomainExceptionCode
{
    CartNotFound,
    CartEmpty,
    InsufficientStock,
    OrderNotFound,
    OrderAlreadyCancelled,
    OrderCancelled,
    InvalidOrderStatus,
    PurchaseAlreadyCancelled,
    PurchaseCancelled,
    PurchaseAlreadyPaid
}
