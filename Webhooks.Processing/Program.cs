using Npgsql;
using Webhooks.Infrastructure;
using Webhooks.Processing.OpenTelemetry;
using Webhooks.Processing.Services;
using Webhooks.Contracts;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHttpClient<WebhookTriggeredHandler>();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))
        .AutoProvision()
        .DeclareExchange("Webhooks.Api.Services:WebhookTriggered")
        .BindExchange("Webhooks.Api.Services:WebhookTriggered").ToQueue("webhook-triggered");

    // Receive WebhookDispatched from Api
    opts.ListenToRabbitQueue("webhook-dispatched");

    // Route cascaded WebhookTriggered through RabbitMQ for parallel fanout
    opts.PublishMessage<WebhookTriggered>().ToRabbitExchange("Webhooks.Api.Services:WebhookTriggered");
    opts.ListenToRabbitQueue("webhook-triggered")
        .ListenerCount(3); // tell RabbitMQ to push up to at once
});

builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing
        .AddSource(DiagnosticConfig.Source.Name)
        .AddSource("Wolverine")
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
