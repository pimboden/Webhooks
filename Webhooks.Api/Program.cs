using System.Reflection;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddHttpClient<WebhookDispatcher>();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks"));
});
builder.Services.AddHostedService<WebhookProcessor>();
builder.Services.AddSingleton(_ => Channel.CreateBounded<WebhookDispatch>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));
builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DiagnosticConfig.Source.Name));
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

