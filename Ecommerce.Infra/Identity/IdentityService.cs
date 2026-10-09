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
            throw new InvalidOperationException("Invalid email or password");

        return user;
    }

    public async Task<IUser> CreateUserAsync(string email, string password)
    {
        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
        };

        var result = await userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationError(e.Code, e.Description));

            throw new ValidationException(errors);
        }

        return newUser;
    }

}
