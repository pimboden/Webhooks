using Microsoft.AspNetCore.Routing;

namespace Webhooks.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
