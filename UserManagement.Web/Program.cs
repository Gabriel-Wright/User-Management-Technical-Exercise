using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Westwind.AspNetCore.Markdown;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services
        .AddDataAccess()
        .AddDomainServices()
        .AddMarkdown()
        .AddControllersWithViews();

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseMarkdown();

    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapDefaultControllerRoute();


    if (builder.Environment.IsDevelopment())
    {
        Log.Information("Running in Development environment");
    }
    else
    {
        Log.Information("App started"); // production-safe log
    }

    app.Run();

}
catch
{
    Log.Fatal("Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
