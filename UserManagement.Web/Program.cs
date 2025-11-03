using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using UserManagement.Data;
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
        .AddDataAccess(builder.Configuration, builder.Environment)
        .AddDomainServices()
        .AddMarkdown()
        .AddControllersWithViews();

    var app = builder.Build();

    //DB check prod
    if (!builder.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            if (!dbContext.Database.CanConnect())
            {
                Log.Fatal("Cannot connect to production database. Application will stop.");
                throw new InvalidOperationException("Cannot connect to the production database.");
            }

            Log.Information("Successfully connected to the production database.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database connection test failed. Application will stop.");
            throw; // Stop the app
        }
    }

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
