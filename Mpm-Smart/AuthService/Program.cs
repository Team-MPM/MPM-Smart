using System.Security.Claims;
using AuthService.Services;
using DataModel.Auth;
using Duende.IdentityServer;
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
    .AddIdentity();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapGrpcService<TenantsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();