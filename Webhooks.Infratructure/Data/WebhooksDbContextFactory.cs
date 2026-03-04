using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Webhooks.Infrastructure.Data;

public sealed class WebhooksDbContextFactory : IDesignTimeDbContextFactory<WebhooksDbContext>
{

    public WebhooksDbContext CreateDbContext(string[] args)
    {

        var optionsBuilder = new DbContextOptionsBuilder<WebhooksDbContext>();
        optionsBuilder.UseNpgsql("lo");

        return new WebhooksDbContext(optionsBuilder.Options);
    }
}
