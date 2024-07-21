using SmartHomeHost;
using SystemBase;

var builder = SmartHomeBuilder.Create<VirtualSystem.VirtualSystem>(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();