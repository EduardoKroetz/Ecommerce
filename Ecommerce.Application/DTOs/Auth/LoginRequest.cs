using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Policies;

namespace Ecommerce.Application.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(PasswordPolicy.MinLength, ErrorMessage = "Password must be at least {1} characters long.")]
    public required string Password { get; set; }
}
