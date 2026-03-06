using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var pgPassword = builder.AddParameter("postgres-password", secret: true);
var database = builder.AddPostgres("postgres",password:pgPassword)
    .WithDataVolume()
    .WithHostPort(49959)
    .WithPgAdmin()
    .AddDatabase("WebhooksDb");

// Use a parameter for the password so external apps (e.g. WebForms, legacy APIs)
// can use a stable, known connection string: amqp://guest:{password}@localhost:5672
// Set via AppHost user secrets: dotnet user-secrets set "Parameters:rabbitmq-password" "yourpassword"
var rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);

var queue = builder.AddRabbitMQ("rabbitmq", password: rabbitPassword)
    .WithDataVolume()
    .WithManagementPlugin()
    .WithEndpoint("tcp", e => e.Port = 5672);  // Pin AMQP port to fixed host port so non-Aspire apps can reach it

var api = builder.AddProject<Projects.Webhooks_Api>("webhooks-api")
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue);

builder.AddJavaScriptApp("webhooks-ui", "../webhooks-ui", "dev")
    .WithPnpm(install: true, installArgs: ["--frozen-lockfile"])
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithEnvironment("CI", "true")
    .WithReference(api)
    .WaitFor(api);

builder.AddProject<Projects.Webhooks_Processing>("webhooks-processing")
    .WithReplicas(3)
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue); ;

builder.Build().Run();
