using Azure.Messaging.ServiceBus;
using CommonLib.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using UILib.Utilities;

// Create a builder for the host application
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true);

// Build the host application
var app = builder.Build();

// Get the configuration service
var config = app.Services.GetRequiredService<IConfiguration>();

// Retrieve Service Bus settings (store them in user-secrets or environment variables)
var serviceBusConnection = config["ServiceBus:ConnectionString"];
var topicName = config["ServiceBus:TopicName"];

ConsoleUtility.WriteApplicationBanner("Order Generator", ConsoleColor.Cyan);

await using var client = new ServiceBusClient(serviceBusConnection);
ServiceBusSender sender = client.CreateSender(topicName);

while (true)
{
    // Ask user for the number of orders to generate
    var numberOfOrders = ReadNumberOfOrders();
    if (numberOfOrders is null)
    {
        break;
    }

    // Sample orders to send
    var orders = OrderGenerator.Generate(numberOfOrders.Value, seed: DateTime.Now.Microsecond);

    ConsoleUtility.WriteLine($"Sending {orders.Count()} orders to '{topicName}'...", ConsoleColor.Cyan);

    foreach (var order in orders)
    {
        ConsoleUtility.WriteLine($"\t Sending {order}", ConsoleColor.Green);
        var message = new ServiceBusMessage(
            BinaryData.FromObjectAsJson(order, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }))
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = "NewOrder"
        };
        message.ApplicationProperties["CustomerName"] = order.CustomerName;
        message.ApplicationProperties["OrderTotal"] = order.TotalAmount;
        message.ApplicationProperties["Sender"] = "OrderGeneratorApp";

        await sender.SendMessageAsync(message);

        await Task.Delay(1000);
    }
    ConsoleUtility.WriteLine($"Finished sending {orders.Count()} orders to '{topicName}'.", ConsoleColor.Cyan);
    ConsoleUtility.WriteLine();
}

/// <summary>
/// Prompts the user for a positive number of orders or to quit the application.
/// </summary>
/// <returns>The number of orders to generate, or <see langword="null"/> when the user chooses to quit.</returns>
int? ReadNumberOfOrders()
{
    while (true)
    {
        ConsoleUtility.Write("Enter the number of orders to generate (quit to exit): ", ConsoleColor.White);
        var input = Console.ReadLine();

        if (input?.Trim().ToLower() == "quit")
        {
            return null;
        }

        if (int.TryParse(input, out var numberOfOrders) && numberOfOrders > 0)
        {
            return numberOfOrders;
        }

        ConsoleUtility.WriteLine("Please enter a valid positive number.", ConsoleColor.Red);
        ConsoleUtility.WriteLine();
    }
}




