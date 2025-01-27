using System.IO.Abstractions;
using Backend.Endpoints;
using Backend.Services.Database;
using Backend.Services.Identity;
using Backend.Services.Permissions;
using Backend.Services.PluginDataQuery;
using Backend.Services.Plugins;
using Backend.Utils;
using Data.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PluginBase;
using PluginBase.Services;
using PluginBase.Services.Devices;
using PluginBase.Services.Networking;
using PluginBase.Services.Permissions;
using PluginBase.Services.Telemetry;
using Serilog;

// ----------------------- Load Key -------------------------------

await KeyManager.CreateKeyIfNotExists("key.rsa");
var rsa = await KeyManager.LoadKey("key.rsa");
var key = new RsaSecurityKey(rsa);

// ----------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// ------------------------ General -------------------------------

builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

// ------------------------ Database ------------------------------

builder.Services.AddDbContextPool<SystemDbContext>(options =>
{
    options.UseSqlite("Data Source=system.db",
        dbContextOptions => { dbContextOptions.MigrationsAssembly(typeof(SystemDbContext).Assembly.FullName); });

    //options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

// ------------------------ Identity ------------------------------

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = key
        };

        options.MapInboundClaims = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("token", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("Permission", policy =>
    {
        policy.Requirements.Add(new PermissionRequirement(""));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionVerifier>();
builder.Services.AddSingleton<AvailablePermissionProvider>();
builder.Services.AddSingleton<PermissionHandler>();

builder.Services.AddIdentity<SystemUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Lockout.AllowedForNewUsers = false;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    })
    .AddEntityFrameworkStores<SystemDbContext>();

// ----------------- Logging and Telemetry ------------------------

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
    .ConfigureResource(options => { options.AddService("Mpm-Smart-Backend"); })
    .WithLogging(options => { options.AddInMemoryExporter(telemetryDataCollector.LogRecords); })
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

// ------------------------- Plugins ------------------------------

builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<IPluginLoader, PluginLoader>();

builder.Services.AddSingleton<NetworkScanner>();
builder.Services.AddSingleton<DeviceTypeRegistry>();
builder.Services.AddSingleton<DeviceRegistry>();
builder.Services.AddSingleton<DeviceManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DeviceManager>());
builder.Services.AddSingleton<DataRequester>();


// ------------------------ Cors ----------------------------------

builder.Services.AddCors();

// ----------------------------------------------------------------

var app = builder.Build();
ServiceProviderHelper.Configure(app.Services);

// ----------------------------------------------------------------

await app.LoadPluginsAsync();

// ------------------------ Permissions ------------------------------

var permissionProvider = app.Services.GetRequiredService<AvailablePermissionProvider>();
permissionProvider.AddRange("System", UserClaims.ExportPermissions());

// ------------------------ Middleware ----------------------------

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ------------------------ Cors ----------------------------------

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

// ----------------------------------------------------------------

await app.StartPluginSystemAsync();

// ------------------------ Endpoints -----------------------------

app.MapOpenApi();

app.MapIdentityEndpoints(key);
app.MapUserProfileEndpoints();
app.MapUserManagementEndpoint();
app.MapSettingsEndpoints();
app.MapPermissionEndpoints();
app.MapRoleManagementEndpoint();
app.MapPluginEndpoints();
app.MapDataRequesterEndpoints();
app.MapDeviceEndpoints();

app.MapGet("/", () => "Hello World!");
app.MapGet("/info", () => new
{
    Version = "1.0.0", System = "Mpm Smart Backend", Id = key.Rsa.ExportRSAPublicKey()
});
app.MapGet("/kys", (IHostApplicationLifetime env) => env.StopApplication());

await app.RunAsync("http://*:54321");