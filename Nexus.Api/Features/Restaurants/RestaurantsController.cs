using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Auth;
using Nexus.Api.Auth.Actor;
using Nexus.Api.Features.Restaurants.Handlers;
using Nexus.Api.Features.Restaurants.Requests;
using Nexus.Grains.Features.Restaurants;
using Wolverine;

namespace Nexus.Api.Features.Restaurants;

[ApiController]
[Route("[controller]")]
public class RestaurantsController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Get(
        IActor actor,
        IGrainFactory grainFactory)
    {
        var restaurantGrain = grainFactory.GetGrain<IRestaurantGrain>(actor.RestaurantId);
        return Ok(await restaurantGrain.GetState());
    }

    [HttpPost("{id:guid}/orders")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> AddOrder(
        Guid id,
        [FromBody] AddOrderRequest request,
        [FromServices] IMessageBus messageBus,
        [FromServices] IActor actor)
    {
        await messageBus.PublishAsync(new CreateOrderCommand(
            actor.RestaurantId, request.OrderId, request.DesirableDeliveryPrice, request.Notes));
        
        return Ok();
    }
}
