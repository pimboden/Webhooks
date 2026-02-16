using System.Reflection;
using Webhooks.Api.Extensions;
using Webhooks.Api.Repositories;
using Webhooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

//InMemory repositories.. for POC no need of interfaces
//Dependency Injections.
builder.Services.AddSingleton<InMemorySampleDataRepository>();
builder.Services.AddSingleton<InMemoryWebhookSubscriptionRepository>();

//register services
builder.Services.AddHttpClient<WebhookDispatcher>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/v1/openapi.json", "v1");
    });
}

app.UseHttpsRedirection();




var routGroupBuilder = app.MapGroup("api");

// Map endpoints to the versioned route group
app.MapEndpoints(routGroupBuilder);


app.Run();

