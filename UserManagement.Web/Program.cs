using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using UserManagement.Data;

namespace UserManagement.Web
{

    public class Program
    {
        public static void Main(string[] args)
        {
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

                //Add services to the container. - removing Views from this layer. Separate UI layer introduced.
                builder.Services
                    .AddDataAccess(builder.Configuration, builder.Environment)
                    .AddDomainServices()
                        .AddControllers();

                //Adding Swagger so we can check Web APIs
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });
                var app = builder.Build();

                app.UseMiddleware<ExceptionMiddlewareCatcher>();
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

                app.UseHsts();
                app.UseHttpsRedirection();
                app.UseAuthorization();

                app.MapControllers();

                if (builder.Environment.IsDevelopment())
                {
                    Log.Information("Running in Development environment");
                    Log.Information("Swagger view enabled");
                    app.UseSwagger();
                    app.UseSwaggerUI();
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

        }
    }
}