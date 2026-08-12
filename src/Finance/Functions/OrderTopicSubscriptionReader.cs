using CommonLib.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Finance.Functions;

public class OrderTopicSubscriptionReader
{
    private readonly ILogger<OrderTopicSubscriptionReader> _logger;

    public OrderTopicSubscriptionReader(ILogger<OrderTopicSubscriptionReader> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OrderTopicSubscriptionReader))]
    public void Run(
        [ServiceBusTrigger(
            "%ServiceBusTopicName%",
            "%ServiceBusSubscriptionName%",
            Connection = "ServiceBusConnection")]
        string messageBody)
    {
        Order? order = null;

        try
        {
            order = JsonSerializer.Deserialize<Order>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize the message payload to Order. Payload: {Payload}", messageBody);
        }

        if (order is null)
        {
            _logger.LogWarning("Received message but no Order could be parsed. Payload: {Payload}", messageBody);
            return;
        }

        _logger.LogInformation(
            "Order received from topic subscription. Id={OrderId}, Customer={Customer}, Total={Total}",
            order.Id,
            order.CustomerName,
            order.TotalAmount);
    }
}
