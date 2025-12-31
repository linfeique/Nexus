namespace Nexus.Api.Features.Restaurants.Requests;

public record AddOrderRequest(
    Guid OrderId,
    decimal DesirableDeliveryPrice,
    string Notes);