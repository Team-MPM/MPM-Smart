using SmartHomeHost;
using SystemBase;

var builder = SmartHome.CreateBuilder<VirtualSystem.VirtualSystem>(args);



var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var system = app.Services.GetRequiredService<ISystem>();
if (system.State != SystemState.Running)
{
    logger.LogCritical("System failed to start");
    return;
}

app.MapGet("/", () => "Hello World!");

app.Run();