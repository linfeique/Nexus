using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Auth.Actor;
using Nexus.Api.Features.Restaurants.Handlers;
using Nexus.Grains.Features.Restaurants;
using Wolverine;

namespace Nexus.Api.Features.Restaurants;

[ApiController]
[Route("[controller]")]
[Authorize]
public class RestaurantsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        IActor actor,
        IGrainFactory grainFactory)
    {
        var restaurantGrain = grainFactory.GetGrain<IRestaurantGrain>(actor.RestaurantId);
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
        [FromServices] IMessageBus messageBus,
        [FromServices] ILogger<RestaurantsController> logger,
        [FromServices] IActor actor)
    {
        // var restaurantGrain = clusterClient.GetGrain<IRestaurantGrain>(id);
        // await restaurantGrain.AddOrder(request.OrderId, request.DesirableDeliveryPrice, request.Notes);
        logger.LogInformation("Restaurant Actor ID: {RestaurantId}", actor.RestaurantId);
        await messageBus.PublishAsync(new CreateOrderCommand());
        return Ok();
    }
}

public record InitialiseRestaurantRequest(string Name, string Address, string Cnpj);
public record AddOrderRequest(Guid OrderId, decimal DesirableDeliveryPrice, string Notes);