using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Webhooks.Api.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Webhooks.Api.Services;

internal sealed class WebhookDispatchedConsumer(WebhooksDbContext webhooksDbContext) : IConsumer<WebhookDispatched>
{
    public async Task Consume(ConsumeContext<WebhookDispatched> context)
    {
        var message = context.Message;
        var subscriptions = await webhooksDbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == message.EventType)
            .ToListAsync();
        //foreach (var subscription in subscriptions)
        //{
        //    await context.Publish(
        //        new WebhookTriggered(
        //            subscription.Id,
        //            subscription.EventType,
        //            subscription.WebhookUrl,
        //            message.Data)
        //    );
        //}
        await context.PublishBatch(subscriptions.Select(s=>new WebhookTriggered(s.Id, s.EventType, s.WebhookUrl,message.Data)));
    }
}