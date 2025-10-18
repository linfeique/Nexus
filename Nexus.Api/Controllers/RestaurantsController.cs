using Microsoft.AspNetCore.Mvc;
using Nexus.Grains.Features.Restaurants;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RestaurantsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        IClusterClient clusterClient)
    {
        var restaurantGrain = clusterClient.GetGrain<IRestaurantGrain>(id);
        return Ok(await restaurantGrain.GetState());
    }

    [HttpPost]
    public async Task<IActionResult> Initialise(
        [FromBody] InitialiseRestaurantRequest request,
        IClusterClient clusterClient)
    {
        var id = Guid.CreateVersion7();
        
        var restaurantGrain = clusterClient.GetGrain<IRestaurantGrain>(id);
        await restaurantGrain.Initialise(request.Name, request.Address, request.Cnpj);
        return Ok(id);
    }

    [HttpPost("{id:guid}/orders")]
    public async Task<IActionResult> AddOrder(
        Guid id,
        [FromBody] AddOrderRequest request,
        IClusterClient clusterClient)
    {
        var restaurantGrain = clusterClient.GetGrain<IRestaurantGrain>(id);
        await restaurantGrain.AddOrder(request.OrderId, request.DesirableDeliveryPrice, request.Notes);
        return Ok();
    }
}

public record InitialiseRestaurantRequest(string Name, string Address, string Cnpj);
public record AddOrderRequest(Guid OrderId, decimal DesirableDeliveryPrice, string Notes);