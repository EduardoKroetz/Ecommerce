namespace Ecommerce.Domain.Enums;

public enum EOrderStatus
{
    AwaitingPayment,
    AwaitingShipment,
    Shipped,
    Delivered,
    Cancelled
}
