using System.Threading.Channels;

namespace Webhooks.Api.Services;

internal sealed class WebhookProcessor(IServiceScopeFactory scopeFactory, 
    Channel<WebhookDispatch> webhooksChannel) :BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var dispatch in webhooksChannel.Reader.ReadAllAsync(cancellationToken))
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
            await dispatcher.ProcessAsync(dispatch.EventType, dispatch.Data, cancellationToken);
        }
    }
}