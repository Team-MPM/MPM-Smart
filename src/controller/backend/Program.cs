using System.IO.Abstractions;
using Backend.Services.Database;
using Backend.Services.Plugins;
using Data.System;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Shared.Services.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// General

builder.Services.AddSingleton<IFileSystem, FileSystem>();

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

var telemetryDataCollector = new TelemetryDataCollector();
builder.Services.AddSingleton<ITelemetryDataCollector>(telemetryDataCollector);

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
        options.AddInMemoryExporter(telemetryDataCollector.LogRecords);
    })
    .WithMetrics(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddRuntimeInstrumentation();
            //.AddMeter(/* Register custom meters here later*/);
        options.AddInMemoryExporter(telemetryDataCollector.Metrics, readerOptions =>
        {
            readerOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
            {
                ExportIntervalMilliseconds = 5000
            };
        });
    })
    .WithTracing(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddInMemoryExporter(telemetryDataCollector.Traces);
    });

// Plugins

builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<IPluginLoader, PluginLoader>();

// ----------------------------------------------------------------

var app = builder.Build();

// ----------------------------------------------------------------

await app.LoadPluginsAsync();

app.UseRouting();

await app.StartPluginSystemAsync();

app.MapGet("/", () => "Hello World!");

app.MapGet("/kys", (IHostApplicationLifetime env) => env.StopApplication());

await app.RunAsync("http://*:543");