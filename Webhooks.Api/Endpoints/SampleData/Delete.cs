using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Services;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Api.Endpoints.SampleData;

public class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("sampledata/{id:guid}", async (
                Guid id,
                WebhooksDbContext webhooksDbContext,
                WebhookDispatcher webhookDispatcher,
                CancellationToken cancellationToken) =>
            {
                var sampleData = await webhooksDbContext.SampleDataItems
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (sampleData is null)
                {
                    return Results.NotFound(new { Message = $"Sample data with ID {id} not found." });
                }

                webhooksDbContext.SampleDataItems.Remove(sampleData);
                await webhooksDbContext.SaveChangesAsync(cancellationToken);
                await webhookDispatcher.DispatchAsync("sampledata.deleted", sampleData, cancellationToken);
                

                return Results.NoContent();
            })
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}