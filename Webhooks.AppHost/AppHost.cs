using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var pgPassword = builder.AddParameter("postgres-password", secret: true);
var database = builder.AddPostgres("postgres",password:pgPassword)
    .WithDataVolume()
    .WithHostPort(49959)
    .WithPgAdmin()
    .AddDatabase("WebhooksDb");

var queue = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume()
    .WithManagementPlugin();

var api = builder.AddProject<Projects.Webhooks_Api>("webhooks-api")
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue);

builder.AddJavaScriptApp("webhooks-ui", "../webhooks-ui", "dev")
    .WithPnpm(install: true, installArgs: ["--frozen-lockfile"])
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithReference(api)
    .WaitFor(api);

builder.AddProject<Projects.Webhooks_Processing>("webhooks-processing")
    .WithReplicas(3)
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue); ;

builder.Build().Run();
