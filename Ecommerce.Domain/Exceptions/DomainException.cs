namespace Ecommerce.Domain.Exceptions;

public class DomainException : Exception
{
    public EDomainExceptionCode Code { get; set; }
    public object? Details { get; set; }

    public DomainException(EDomainExceptionCode code, string message, object? details = null) : base(message)
    {
        Code = code;
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
    InvalidOrderStatus,
    PurchaseAlreadyCancelled,
    PurchaseAlreadyPaid,
    InvalidPurchaseStatus,
    CartItemQuantityExceeded,
    CartItemNotFound,
    OrderWithoutItems,
    PurchaseWithoutOrders,
    PurchaseDuplicateSellerOrders,
    ProductNameInvalid,
    ProductDescriptionTooLong,
    ProductPriceInvalid,
    ProductStockInvalid,
    ProductCategoryNotAssociated,
    InvalidOrderItemQuantity,
    OrderWithDuplicatedItems
}
