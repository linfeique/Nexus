using System.Text.Json;
using Ardalis.Result;

namespace Nexus.Api.Features.Restaurants.Handlers;

public record CreateOrderCommand(
    Guid RestaurantId,
    Guid OrderId,
    decimal DesirableDeliveryPrice,
    string Notes);

public class CreateOrderHandler(
    IGrainFactory grainFactory)
{
    public Task<Result> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        return Task.FromResult(Result.Success());
    }
}