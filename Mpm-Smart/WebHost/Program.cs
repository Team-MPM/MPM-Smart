var builder = DistributedApplication.CreateBuilder(args);

var launchProfile = builder.ExecutionContext.IsPublishMode ? "https" : "Watch";

var api = builder.AddProject<Projects.Web_Api>("Api", "Watch");

builder.AddProject<Projects.Web_Server>("Web-Server", "Watch")
    .WithReference(api);

builder.Build().Run();