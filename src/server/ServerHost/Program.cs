using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddAzureRedis("cache")
    .RunAsContainer();

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var blob = storage.AddBlobs("blobs");

builder.AddProject<Server>("server")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(blob);

await builder.Build().RunAsync();