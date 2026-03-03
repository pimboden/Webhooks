using System.Reflection;
using System.Threading.Channels;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Webhooks.Api.Data;
using Webhooks.Api.Extensions;
using Webhooks.Api.OpenTelemetry;
using Webhooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

//Added by aspire
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

//Dependency Injections.

//register services
builder.Services.AddScoped<WebhookDispatcher>();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks"));
});

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();
    busConfig.AddConsumer<WebhookDispatchedConsumer>();
    busConfig.AddConsumer<WebhookTriggeredConsumer>();
    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing
        .AddSource(DiagnosticConfig.Source.Name)
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
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
    await  app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();




var routGroupBuilder = app.MapGroup("api");

// Map endpoints to the versioned route group
app.MapEndpoints(routGroupBuilder);


app.Run();

