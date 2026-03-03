using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var pgPassword = builder.AddParameter("postgres-password", secret: true);
var database = builder.AddPostgres("postgres",password:pgPassword)
    .WithDataVolume()
    .WithHostPort(49959)
    .WithPgAdmin()
    .AddDatabase("webhooks");

var queue = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume()
    .WithManagementPlugin();

builder.AddProject<Projects.Webhooks_Api>("webhooks-api")
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue);

builder.AddProject<Projects.Webhooks_Processing>("webhooks-processing")
    .WithReference(database)
    .WithReference(queue)
    .WaitFor(database)
    .WaitFor(queue); ;

builder.Build().Run();
