using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagement.UI;
using UserManagement.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IUserApiService, UserApiService>(sp =>
    new UserApiService(sp.GetRequiredService<HttpClient>()));
builder.Services.AddScoped<IUserAuditApiService, UserAuditApiService>(sp =>
    new UserAuditApiService(sp.GetRequiredService<HttpClient>()));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7084/api/") });

builder.Services.AddScoped<UserApiService>();
await builder.Build().RunAsync();