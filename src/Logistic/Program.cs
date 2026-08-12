using Azure.Messaging.ServiceBus;
using CommonLib.Utilities;
using CommonLib.Entities;
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

// --- Service Bus Topic Receiver Setup ---
var serviceBusConnectionString = config["ServiceBus:ConnectionString"];
var topicName = config["ServiceBus:TopicName"];
var subscriptionName = config["ServiceBus:SubscriptionName"];

// Validate that subscription name is configured
if (string.IsNullOrWhiteSpace(subscriptionName))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Subscription name is not configured in appsettings.");
    Console.WriteLine("Please enter the subscription name:");
    Console.ResetColor();
    subscriptionName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(subscriptionName))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Subscription name cannot be empty. Exiting application.");
        Console.ResetColor();
        return;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Using subscription name: {subscriptionName}");
    Console.ResetColor();
}

await using var client = new ServiceBusClient(serviceBusConnectionString);

var processor = client.CreateProcessor(topicName, subscriptionName,
        new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

// Attach handlers
processor.ProcessMessageAsync += ProcessMessageHandler;
processor.ProcessErrorAsync += ProcessErrorHandler;

Console.WriteLine("Starting Service Bus topic subscription processor...");
await processor.StartProcessingAsync();

Console.WriteLine("Listening for messages. Press <Enter> to exit.");
Console.ReadLine();

Console.WriteLine("Stopping processor...");
await processor.StopProcessingAsync();
await processor.DisposeAsync();

// ---------------- Handlers ----------------
static async Task ProcessMessageHandler(ProcessMessageEventArgs args)
{
    try
    {
        var json = args.Message.Body.ToString();
        var order = JsonSerializer.Deserialize<Order>(json,
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

        if (order is not null)
        {
            Console.WriteLine($"[Message Received] Order : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{order}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Message Received] Payload could not be deserialized to Order");
            Console.ResetColor();
        }

        await args.CompleteMessageAsync(args.Message);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Message handling failed: {ex.Message}");
        Console.ResetColor();
        await args.AbandonMessageAsync(args.Message);
    }
}

static Task ProcessErrorHandler(ProcessErrorEventArgs args)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[Processor Error] EntityPath={args.EntityPath} Source={args.ErrorSource} Exception={args.Exception.Message}");
    Console.ResetColor();
    return Task.CompletedTask;
}
