using Azure.Messaging.ServiceBus;
using CommonLib.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

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

// Ask user for the number of orders to generate
int numberOfOrders;
while (true)
{
    Console.Write("Enter the number of orders to generate: ");
    var input = Console.ReadLine();
    
    if (int.TryParse(input, out numberOfOrders) && numberOfOrders > 0)
    {
        break;
    }
    
    Console.WriteLine("Please enter a valid positive number.");
}

// Sample orders to send
var orders = OrderGenerator.Generate(numberOfOrders, seed: DateTime.Now.Microsecond);

await using var client = new ServiceBusClient(serviceBusConnection);
ServiceBusSender sender = client.CreateSender(topicName);

Console.WriteLine($"Sending {orders.Count()} orders to '{topicName}'...");

foreach (var order in orders)
{
    Console.WriteLine($"\t Sending {order}");
    var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(order, new JsonSerializerOptions
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

Console.WriteLine("Done.");




