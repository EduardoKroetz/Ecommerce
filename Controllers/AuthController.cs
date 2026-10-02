using Ecommerce.DTOs.Auth;
using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
public class AuthController(TokenService tokenService, UserManager<User> userManager) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Problem(title: "Failed to create user.", statusCode: StatusCodes.Status400BadRequest);

        var token = tokenService.GenerateToken(user.Id, request.Email);

        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);

        var token = tokenService.GenerateToken(user.Id, request.Email);

        return Ok(new { Token = token });
    }
}
