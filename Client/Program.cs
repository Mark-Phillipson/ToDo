using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using Blazored.Toast;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredLocalStorage();
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["wwwroot"] ?? builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<BlazorApp.Client.Services.OfflineStateService>();

var host = builder.Build();
var offlineService = host.Services.GetRequiredService<BlazorApp.Client.Services.OfflineStateService>();
await offlineService.InitializeAsync();
await host.RunAsync();
