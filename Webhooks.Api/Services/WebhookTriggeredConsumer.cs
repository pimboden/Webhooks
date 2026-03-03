using System.Text.Json;
using MassTransit;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Services;

internal sealed class WebhookTriggeredConsumer(HttpClient httpClient, WebhooksDbContext webhooksDbContext) : IConsumer<WebhookTriggered>
{
    public async Task Consume(ConsumeContext<WebhookTriggered> context)
    {
        var cancellationToken = CancellationToken.None;
        var payload = new WebhookPayload
        {
            Id = Guid.NewGuid(),
            EventType = context.Message.EventType,
            SubscriptionId = context.Message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = context.Message.Data
        };
        var jsonPayload = JsonSerializer.Serialize(payload);
        try
        {
            var response = await httpClient.PostAsJsonAsync(context.Message.WebhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = context.Message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = (int)response.StatusCode,
                Success = response.IsSuccessStatusCode,
                Timestamp = DateTime.UtcNow
            };
            await webhooksDbContext.WebhookDeliveryAttempts.AddAsync(attempt, cancellationToken);
            await webhooksDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = context.Message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = null,
                Success = false,
                Timestamp = DateTime.UtcNow
            };
            await webhooksDbContext.WebhookDeliveryAttempts.AddAsync(attempt,  cancellationToken);
            await webhooksDbContext.SaveChangesAsync( cancellationToken);
        }
    }
}