using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Webhooks.Api.Endpoints;

namespace Webhooks.Api.Extensions;

public static class EndpointExtensions
{
    extension(IServiceCollection services)
    {

        public IServiceCollection AddEndpoints(Assembly assembly)
        {
            var serviceDescriptors = assembly
                .DefinedTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                               type.IsAssignableTo(typeof(IEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
                .ToArray();

            services.TryAddEnumerable(serviceDescriptors);

            return services;
        }
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder != null ? routeGroupBuilder : app;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }
        return app;
    }

}