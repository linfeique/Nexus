using Microsoft.AspNetCore.Identity;

namespace Nexus.Api.Auth;

public class User : IdentityUser
{
    public Guid RestaurantId { get; set; }
}