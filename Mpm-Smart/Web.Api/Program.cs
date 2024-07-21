

  using Grpc.Net.Client;
using Services;
using Services.Grpc;
using Services.Grpc.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

Console.WriteLine(builder.Configuration["services::http:0"]);

builder.Services.AddGrpcClient<TenantClient>(options =>
{
    options.Host = new Uri($"{builder.Configuration["services:AuthService:http:0"]}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();