using System.Text.Json;
using Ardalis.Result;

namespace Nexus.Api.Features.Restaurants.Handlers;

public record CreateOrderCommand();

public class CreateOrderHandler(ILogger<CreateOrderHandler> logger)
{
    public Task<Result> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        logger.LogInformation("Received CreateOrderCommand: {Payload}", JsonSerializer.Serialize(request));
        return Task.FromResult(Result.Success());
    }
}