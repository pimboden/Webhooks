using Microsoft.EntityFrameworkCore;
using Webhooks.Infratructure.Data;

namespace Webhooks.Api.Endpoints.SampleData;

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sampledata", async (
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) => Results.Ok(await webhooksDbContext.SampleDataItems.ToListAsync(cancellationToken)))
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}