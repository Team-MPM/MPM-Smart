using System.Net;
using ApiSchema.Identity;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Pages.General;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddSingleton<UserAccessor>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddSingleton<ApiAccessor>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();
builder.Services.AddSingleton<ControllerConnectionManager>();
builder.Services.AddScoped<RT>();
builder.Services.AddScoped<TokenHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var connectionManager = scope.ServiceProvider.GetRequiredService<ControllerConnectionManager>();
    var storage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
    var auth = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
    scope.ServiceProvider.GetRequiredService<UserAccessor>();
    connectionManager.Init();
    var current = await storage.GetItemAsync<HomePage.ControllerInfo>("current-controller");
    if (current is not null)
    {
        await connectionManager.ConnectToControllerAsync(new ControllerConnectionDetails(
            Address: current.Address,
            Port: current.Port,
            UseHttps: current.UseHttps
        ), new ControllerStoredCredentials(storage), auth);
    }
}

await app.RunAsync();
