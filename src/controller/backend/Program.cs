using System.Diagnostics;
using Backend.Api;
using Backend.Services;
using Data.System;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Database

builder.Services.AddDbContextPool<SystemDbContext>(options =>
{
    options.UseSqlite("Data Source=system.db", dbContextOptions =>
    {
        dbContextOptions.MigrationsAssembly(typeof(SystemDbContext).Assembly.FullName);
    });

    //options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

// Logging and Telemetry

var telemetryDataService = new TelemetryDataService();
builder.Services.AddSingleton(telemetryDataService);

builder.Services.AddLogging(options =>
{
    options.ClearProviders();

    options.AddSerilog(new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger());
    
    options.AddSerilog(new LoggerConfiguration()
        .WriteTo.File("logs/backend.log")
        .MinimumLevel.Information()
        .CreateLogger());
    
    options.AddSerilog(new LoggerConfiguration()
        .WriteTo.File("logs/error.log")
        .MinimumLevel.Error()
        .CreateLogger());
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(options =>
    {
        options.AddService("Mpm-Smart-Backend");
    })
    .WithLogging(options =>
    {
        options.AddInMemoryExporter(telemetryDataService.LogRecords);
    })
    .WithMetrics(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddRuntimeInstrumentation();
            //.AddMeter(/* Register custom meters here later*/);
        options.AddInMemoryExporter(telemetryDataService.Metrics, readerOptions =>
        {
            readerOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
            {
                ExportIntervalMilliseconds = 1000
            };
        });
    })
    .WithTracing(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddInMemoryExporter(telemetryDataService.Traces);
    });

// Plugins

builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<PluginLoader>();

// ----------------------------------------------------------------

var app = builder.Build();

// ----------------------------------------------------------------

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

app.MapTelemetryEndpoints();

await app.RunAsync("http://*:54321");
