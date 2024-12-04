using MudBlazor.Services;
using Server.Endpoints;
using Server.Services;
using Server.UI.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.AddAzureBlobClient("blobs");

builder.AddRedisOutputCache("cache");

builder.Services.AddSingleton<PluginIndexService>();
builder.Services.AddSingleton<BlobInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BlobInitializer>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.UseOutputCache();

app.MapDefaultEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPluginEndpoints();

app.Run();