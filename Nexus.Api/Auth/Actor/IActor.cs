namespace Nexus.Api.Auth.Actor;

public interface IActor
{
    public string Email { get; set; }
    
    public Guid RestaurantId { get; set; }
}