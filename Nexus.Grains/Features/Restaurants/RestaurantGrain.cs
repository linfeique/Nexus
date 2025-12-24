using Nexus.Grains.Features.Restaurants.State;

namespace Nexus.Grains.Features.Restaurants;

public class RestaurantGrain : Grain, IRestaurantGrain
{
    private readonly IPersistentState<RestaurantState> _restaurantState;
    
    public RestaurantGrain(
        [PersistentState(RestaurantState.StateName, SiloConstants.StorageName)] 
        IPersistentState<RestaurantState> restaurantState)
    {
        _restaurantState = restaurantState;
    }

    public async Task Initialise(string name, string address, string cnpj)
    {
        _restaurantState.State.Id = this.GetPrimaryKey();
        _restaurantState.State.Name = name;
        _restaurantState.State.Address = address;
        _restaurantState.State.Cnpj = cnpj;
        
        await _restaurantState.WriteStateAsync();
    }

    public async Task AddOrder(Guid orderId, decimal desirableDeliveryPrice, string notes)
    {
        _restaurantState.State.Orders.Add(orderId);
        await _restaurantState.WriteStateAsync();
    }

    public Task<RestaurantState> GetState()
    {
        return Task.FromResult(_restaurantState.State);
    }
}