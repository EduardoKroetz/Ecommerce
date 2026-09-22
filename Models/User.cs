using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Models;

public class User : IdentityUser
{
    public required string Name { get; set; }
}
