using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Webhooks.Infrastructure.Abstractions;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WebhooksDb")!;
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddDbContext<WebhooksDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }
}
