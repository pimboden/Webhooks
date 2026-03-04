using System.Reflection;
using ImTools;
using Npgsql;
using Webhooks.Api.Extensions;
using Webhooks.Api.OpenTelemetry;
using Webhooks.Api.Services;
using Webhooks.Infrastructure;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3001", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//Dependency Injections.

//register services
builder.Services.AddScoped<WebhookDispatcher>();
builder.Services.AddPersistence(builder.Configuration);

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))
        .AutoProvision()
        .DeclareExchange("Webhooks.Api.Services:WebhookDispatched")
        .BindExchange("Webhooks.Api.Services:WebhookDispatched").ToQueue("webhook-dispatched");

    opts.PublishMessage<WebhookDispatched>().ToRabbitExchange("Webhooks.Api.Services:WebhookDispatched");
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

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}




var routGroupBuilder = app.MapGroup("api");

// Map endpoints to the versioned route group
app.MapEndpoints(routGroupBuilder);


app.Run();

