using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Webhooks.Api.Services;
using Webhooks.Infratructure.Data;
using Webhooks.Processing.OpenTelemetry;
using Webhooks.Infratructure;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddPersistence(builder.Configuration);
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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
