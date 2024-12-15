using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddAzureSqlServer("sql")
    .RunAsContainer();

var db = sql.AddDatabase("db");

var redis = builder.AddAzureRedis("cache")
    .RunAsContainer();

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var blob = storage.AddBlobs("blobs");

builder.AddProject<Server>("server")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(blob)
    .WithReference(db)
    .WaitFor(db);

await builder.Build().RunAsync();