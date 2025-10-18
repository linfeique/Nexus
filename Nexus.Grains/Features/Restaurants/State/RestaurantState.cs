namespace Nexus.Grains.Features.Restaurants.State;

[GenerateSerializer]
[Alias("Nexus.Grains.Features.Restaurants.State.RestaurantState")]
public class RestaurantState
{
    public const string StateName = "RestaurantState";
    
    [Id(0)]
    public Guid Id { get; set; }
    
    [Id(1)]
    public string Name { get; set; } = string.Empty;
    
    [Id(2)]
    public string Address { get; set; } = string.Empty;
    
    [Id(3)]
    public string Cnpj { get; set; } = string.Empty;
    
    [Id(4)]
    public List<Guid> Orders { get; set; } = [];
}