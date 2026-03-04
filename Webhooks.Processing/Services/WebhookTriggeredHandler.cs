using System.Text.Json;
using Webhooks.Contracts;
using Webhooks.Infrastructure.Data;
using Webhooks.Infrastructure.Models;

namespace Webhooks.Processing.Services;

public sealed class WebhookTriggeredHandler(HttpClient httpClient, WebhooksDbContext db)
{
    public async Task Handle(WebhookTriggered message, CancellationToken cancellationToken)
    {
        var payload = new WebhookPayload
        {
            Id = Guid.NewGuid(),
            EventType = message.EventType,
            SubscriptionId = message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = message.Data
        };
        var jsonPayload = JsonSerializer.Serialize(payload);
        try
        {
            var response = await httpClient.PostAsJsonAsync(message.WebhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = (int)response.StatusCode,
                Success = response.IsSuccessStatusCode,
                Timestamp = DateTime.UtcNow
            };
            await db.WebhookDeliveryAttempts.AddAsync(attempt, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = null,
                Success = false,
                Timestamp = DateTime.UtcNow
            };
            await db.WebhookDeliveryAttempts.AddAsync(attempt, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
