using Microsoft.IdentityModel.JsonWebTokens;

namespace Nexus.Api.Auth.Actor;

public class Actor : IActor
{
    public Actor(IHttpContextAccessor httpContextAccessor)
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims.ToList();

        Email = claims?.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
        
        if (Guid.TryParse(claims?.FirstOrDefault(d => d.Type == nameof(RestaurantId))?.Value, out var restaurantId))
            RestaurantId = restaurantId;
    }

    public string Email { get; set; }
    
    public Guid RestaurantId { get; set; }
}