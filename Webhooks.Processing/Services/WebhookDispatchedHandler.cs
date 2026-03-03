using Microsoft.EntityFrameworkCore;
using Webhooks.Contracts;
using Webhooks.Infratructure.Data;

namespace Webhooks.Processing.Services;

public sealed class WebhookDispatchedHandler(WebhooksDbContext db)
{
    public async IAsyncEnumerable<WebhookTriggered> Handle(WebhookDispatched message)
    {
        var subscriptions = await db.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == message.EventType)
            .ToListAsync();

        foreach (var s in subscriptions)
            yield return new WebhookTriggered(s.Id, s.EventType, s.WebhookUrl, message.Data);
    }
}
