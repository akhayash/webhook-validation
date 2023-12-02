using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ローカル実行時にConsole Loggerを使用
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
}
else
{
    // クラウド実行時にApplication Insightsを使用
    builder.Services.AddApplicationInsightsTelemetry();
}

WebApplication app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) => 
{
    logger.LogInformation("/ is called");
    return "Eventgrid subscription validation";});

app.MapPost("/api/incomingCall", (
    [FromBody] EventGridEvent[] eventGridEvents,
    ILogger<Program> logger) =>
{

    logger?.LogInformation($"Incoming Call event received. EventGridEvents Count: {eventGridEvents.Length}");

    foreach (EventGridEvent eventGridEvent in eventGridEvents)
    {
        logger?.LogInformation($"Incoming Call EventGridEvent: {JsonSerializer.Serialize(eventGridEvent)}");

        if (eventGridEvent.TryGetSystemEventData(out object eventData))
        {
            // Handle the subscription validation event.
            if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
            {
                SubscriptionValidationResponse responseData = new()
                {
                    ValidationResponse = subscriptionValidationEventData.ValidationCode
                };
                return Results.Ok(responseData);
            }
        }
    }
    return Results.BadRequest("No valid webhook request");
});

// app.Urls.Add("https://+:8443");


app.Run();

