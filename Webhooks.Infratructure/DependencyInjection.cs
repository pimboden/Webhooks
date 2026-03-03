using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Webhooks.Infratructure.Abstractions;
using Webhooks.Infratructure.Data;

namespace Webhooks.Infratructure;

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
