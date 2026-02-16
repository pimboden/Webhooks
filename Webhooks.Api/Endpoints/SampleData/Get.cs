using Webhooks.Api.Repositories;

namespace Webhooks.Api.Endpoints.SampleData;

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sampledata", async (
                InMemorySampleDataRepository sampleDataRepository,
                CancellationToken cancellationToken) => Results.Ok((object?)await sampleDataRepository.GetAllAsync(cancellationToken)))
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}