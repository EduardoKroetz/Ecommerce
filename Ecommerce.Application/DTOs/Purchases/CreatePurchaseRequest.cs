using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Purchases;

public class CreatePurchaseRequest
{
    [Required(ErrorMessage = "Payment method is required.")]
    [EnumDataType(typeof(EPaymentMethod), ErrorMessage = "Invalid payment method.")]
    public EPaymentMethod? PaymentMethod { get; set; }
}
