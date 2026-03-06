using System.Reflection;
using Npgsql;
using Webhooks.Api.Extensions;
using Webhooks.EventDispatcher.Wolverine;
using Webhooks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//Added by aspire
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3001", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddPersistence(builder.Configuration);

// Registers IWebhookDispatcher + configures Wolverine + RabbitMQ in one call
builder.Host.AddEventDispatcher(opts =>
{
    opts.RabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;
    // opts.ExchangeName defaults to "webhook.dispatched.exchange"
    // opts.QueueName   defaults to "webhook-dispatched"  ← must match Webhooks.Processing
});

builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing
        .AddSource(EventDispatcherDiagnostics.ActivitySourceName)
        .AddSource("Wolverine")
        .AddNpgsql();
});

var app = builder.Build();

//Added by aspire
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/v1/openapi.json", "v1");
    });
    await app.ApplyMigrationsAsync();
}

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var routGroupBuilder = app.MapGroup("api");

// Map endpoints to the versioned route group
app.MapEndpoints(routGroupBuilder);

app.Run();
