var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("hotelbooking");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

builder.AddProject<Projects.HotelBooking_Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.HotelBooking_Worker>("worker")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.Build().Run();
