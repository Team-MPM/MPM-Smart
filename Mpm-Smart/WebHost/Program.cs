var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Web_Server>("Web-Server", "Watch");

builder.Build().Run();