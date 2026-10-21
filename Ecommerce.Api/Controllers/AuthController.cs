using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        var response = await authService.RegisterCustomerAsync(request);

        return Ok(response);
    }

    [HttpPost("register/seller")]
    public async Task<IActionResult> RegisterSeller([FromBody] RegisterSellerRequest request)
    {
        var response = await authService.RegisterSellerAsync(request);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);

        return Ok(response);
    }
}
