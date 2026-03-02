using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Services;

internal sealed class WebhookDispatcher(
    Channel<WebhookDispatch> webhooksChannel,
    HttpClient httpClient,
    WebhooksDbContext webhooksDbContext)
{
    public async Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken) where T : notnull
    {
        await webhooksChannel.Writer.WriteAsync(new WebhookDispatch(eventType, data), cancellationToken);
    }

    public async Task ProcessAsync<T>(string eventType, T data, CancellationToken cancellationToken)
    {
        var subscriptions = await webhooksDbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == eventType)
            .ToListAsync(cancellationToken);
        foreach (var subscription in subscriptions)
        {
            var payload = new WebhookPayload<T>
            {
                Id = Guid.NewGuid(), 
                EventType = subscription.EventType, 
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = data
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            try
            {
                var response = await httpClient.PostAsJsonAsync(subscription.WebhookUrl, payload, cancellationToken);
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
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
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ResponseStatusCode = null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                };
                await webhooksDbContext.WebhookDeliveryAttempts.AddAsync(attempt, cancellationToken);
                await webhooksDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}