using Microsoft.EntityFrameworkCore;
using Webhooks.Infrastructure.Models;

namespace Webhooks.Infrastructure.Data;

public sealed class WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : DbContext(options)
{
    public DbSet<SampleData> SampleDataItems { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SampleData>(builder =>
        {
            builder.ToTable("sample_data_items");
            builder.HasKey(sd => sd.Id);
        });
        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");
            builder.HasKey(sd => sd.Id);
        });
        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");
            builder.HasKey(sd => sd.Id);
            builder.HasOne<WebhookSubscription>().WithMany().HasForeignKey(d => d.WebhookSubscriptionId);
        });
    }
}