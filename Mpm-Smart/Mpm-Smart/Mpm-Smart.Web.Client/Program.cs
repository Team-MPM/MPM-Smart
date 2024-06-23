using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mpm_Smart.Shared.Services;
using Mpm_Smart.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Mpm_Smart.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
