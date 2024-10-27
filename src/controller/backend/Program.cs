using Backend.Services;
using Data.System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<SystemDbContext>(options =>
{
    options.UseSqlite("Data Source=system.db", options =>
    {
        options.MigrationsAssembly(typeof(SystemDbContext).Assembly.FullName);
    });

    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<PluginLoader>();

var app = builder.Build();

var pluginLoader = app.Services.GetRequiredService<PluginLoader>();
await pluginLoader.StartAsync(CancellationToken.None);
await pluginLoader.WaitForPluginsToLoadAsync();

app.UseRouting();

var pluginManager = app.Services.GetRequiredService<PluginManager>();
var pluginEndpoints = pluginManager.Plugins.SelectMany(p => p.Endpoints);
foreach (var registerPluginEndpoint in pluginEndpoints)
    registerPluginEndpoint(app);

app.MapGet("/", () => "Hello World!");

app.MapGet("/kys", (IHostApplicationLifetime env) => env.StopApplication());

await app.RunAsync("http://*:54321");
