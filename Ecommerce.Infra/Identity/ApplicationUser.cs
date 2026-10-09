using Ecommerce.Application.Interfaces.Infra;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infra.Identity;

public class ApplicationUser : IdentityUser, IUser
{
}
