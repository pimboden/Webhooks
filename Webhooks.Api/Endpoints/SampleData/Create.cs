using Webhooks.Api.Data;
using Webhooks.Api.Models.Requests;
using Webhooks.Api.Services;

namespace Webhooks.Api.Endpoints.SampleData;

public class Create : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sampledata", async (
                SampleDataCreateRequest request,
                WebhooksDbContext webhooksDbContext,
                WebhookDispatcher webhookDispatcher,
                CancellationToken cancellationToken) =>
            {

                //This is a POC for now no Clean architecture, we will refactor this later, the main goal is to have a working endpoint that we can test with the webhook system, and then we will refactor it to follow Clean Architecture principles.
                var sampleData = new Models.SampleData(Guid.NewGuid(), request.Name,request.Description);
                await webhooksDbContext.SampleDataItems.AddAsync(sampleData, cancellationToken);
                await webhookDispatcher.DispatchAsync("sampledata.created", sampleData, cancellationToken);
                return Results.Ok(sampleData);
            })
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}