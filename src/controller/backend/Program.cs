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

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
