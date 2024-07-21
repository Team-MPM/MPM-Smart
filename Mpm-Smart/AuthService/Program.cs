using AuthService.Services;
using Services.Core;
using Services.Db;
using Services.Grpc;
using Services.Identity;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddServiceDefaults()
    .AddGrpc()
    .AddDb()
    .AddAuth()
    .AddIdentity()
    .AddIdentityServer();

var app = builder.Build();

app.MapDefaultMiddleware();

app.MapDefaultEndpoints();
app.MapGrpcService<TenantsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();