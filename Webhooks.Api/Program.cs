using System.Reflection;
using Npgsql;
using Webhooks.Api.Extensions;
using Webhooks.Api.OpenTelemetry;
using Webhooks.Api.Services;
using Webhooks.Infratructure;
using Webhooks.Contracts;
using Wolverine;
using Wolverine.RabbitMQ;

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
builder.Services.AddPersistence(builder.Configuration);

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))
        .AutoProvision();

    opts.PublishMessage<WebhookDispatched>().ToRabbitQueue("webhook-dispatched");
});

builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing
        .AddSource(DiagnosticConfig.Source.Name)
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
    await  app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();




var routGroupBuilder = app.MapGroup("api");

// Map endpoints to the versioned route group
app.MapEndpoints(routGroupBuilder);


app.Run();

