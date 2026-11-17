using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infra.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{

    public async Task<IUser> AuthenticateAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !await userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedException("Invalid email or password.");

        return user;
    }

    public async Task<IUser> CreateUserAsync(string name, string email, string password)
    {
        var newUser = new ApplicationUser
        {
            Name = name,
            UserName = email,
            Email = email,
        };

        // Exception to the unit of work: UserManager runs its own SaveChanges (ADR 0020)
        var result = await userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => GetRequestField(e.Code))
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            throw new ValidationException(errors);
        }

        return newUser;
    }

    // Identity reports only a code, so the request field is derived from it: password rules start with "Password",
    // and the user name is the email. Anything else is not tied to a field (empty key, as in the model validation)
    private static string GetRequestField(string code) => code switch
    {
        _ when code.StartsWith("Password") => "Password",
        _ when code.Contains("Email") || code.Contains("UserName") => "Email",
        _ => string.Empty
    };
}
