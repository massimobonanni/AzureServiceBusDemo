var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/servicebus", (IConfiguration configuration) => Results.Ok(new
{
    fullyQualifiedNamespace = configuration["ServiceBus:FullyQualifiedNamespace"],
    queueName = configuration["ServiceBus:QueueName"]
}));

app.Run();
