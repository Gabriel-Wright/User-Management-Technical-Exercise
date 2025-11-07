using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagement.UI;
using UserManagement.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Since static files - just use configuration value directly
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Missing 'ApiBaseUrl' in configuration.");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});
Console.WriteLine($"Set API Base URL: {apiBaseUrl}");
// API services
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IUserAuditApiService, UserAuditApiService>();

await builder.Build().RunAsync();