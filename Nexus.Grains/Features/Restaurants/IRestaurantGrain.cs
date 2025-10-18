using Nexus.Grains.Features.Restaurants.State;
using Orleans;

namespace Nexus.Grains.Features.Restaurants;

[Alias("Nexus.Grains.Features.Restaurants.IRestaurantGrain")]
public interface IRestaurantGrain : IGrainWithGuidKey
{
    [Alias("Initialise")]
    Task Initialise(string name, string address, string cnpj);

    [Alias("AddOrder")]
    Task AddOrder(Guid orderId, decimal desirableDeliveryPrice, string notes);

    [Alias("GetState")]
    Task<RestaurantState> GetState();
}