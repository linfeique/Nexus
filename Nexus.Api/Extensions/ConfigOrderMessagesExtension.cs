using DotPulsar;
using Nexus.Api.Features.Restaurants.Handlers;
using Wolverine;
using Wolverine.Pulsar;

namespace Nexus.Api.Extensions;

public static class ConfigOrderMessagesExtension
{
    private const string OrdersTopic = "persistent://public/default/orders";
    
    public static void AddPublishOrderMessages(this WolverineOptions options)
    {
        options.PublishMessage<CreateOrderCommand>()
            .ToPulsarTopic(OrdersTopic)
            .SendInline();
    }
    
    public static void AddSubscribeOrderMessages(this WolverineOptions options)
    {
        options.ListenToPulsarTopic(OrdersTopic)
            .SubscriptionName("orders-sub")
            .SubscriptionType(SubscriptionType.Exclusive)
            .Sequential();
    }
}