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
            Email = request.Email,
            UserName = request.Name
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Failed to create user." });

        var token = tokenService.GenerateToken(user.Id, request.Email);

        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Unauthorized(new { Message = "Invalid email or password." });

        var token = tokenService.GenerateToken(user.Id, request.Email);

        return Ok(new { Token = token });
    }
}
