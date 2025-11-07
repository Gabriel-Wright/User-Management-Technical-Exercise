using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using UserManagement.Data;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Events;
using UserMangement.Services.Events;

namespace UserManagement.Web
{
    /// <summary>
    /// Entry point for the User Management Web API.
    /// Configures logging, services, middleware, and environment-specific behaviour.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureBootstrapLogger();

            try
            {
                Log.Information("Starting web host...");

                var builder = WebApplication.CreateBuilder(args);

                ConfigureHost(builder);
                ConfigureServices(builder);

                var app = builder.Build();

                ConfigureMiddleware(app);
                ConfigureEventBusSubscriptions(app);
                ConfigureSwagger(app, builder.Environment);
                ValidateDatabaseConnection(app, builder.Environment);

                Log.Information("Application startup complete. Running...");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start correctly.");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // ---------------------------------------------------------------
        //  Logging
        // ---------------------------------------------------------------

        private static void ConfigureBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }

        private static void ConfigureHost(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext());
        }

        // ---------------------------------------------------------------
        //  Service setup
        // ---------------------------------------------------------------

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient", policy =>
                {
                    policy
                        .WithOrigins(FindAllowedCorsOrigin(builder.Environment, builder.Configuration))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            //Data layer
            builder.Services
                .AddDataAccess(builder.Configuration, builder.Environment)
                .AddDomainServices();

            //Event bus - in memory
            builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

            builder.Services
                .AddControllers()
                .AddApplicationPart(typeof(Program).Assembly);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
        }

        // ---------------------------------------------------------------
        //  Middleware Pipeline - catching errors from controller
        // ---------------------------------------------------------------

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowBlazorClient");

            app.UseMiddleware<ExceptionMiddlewareCatcher>();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseHsts();

            app.UseAuthorization();
            app.MapControllers();
        }

        // ---------------------------------------------------------------
        //  Swagger - Dev
        // ---------------------------------------------------------------

        private static void ConfigureSwagger(WebApplication app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Log.Information("Running in Development environment. Swagger enabled.");
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                Log.Information("Running in Production environment.");
            }
        }

        // ---------------------------------------------------------------
        //  Database Validation - Prod
        // ---------------------------------------------------------------

        private static void ValidateDatabaseConnection(WebApplication app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
                return;

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
                Log.Fatal(ex, "Database connection validation failed. Application will stop.");
                throw;
            }
        }

        // ---------------------------------------------------------------
        //  CORS Helpers
        // ---------------------------------------------------------------

        private static string FindAllowedCorsOrigin(IHostEnvironment env, IConfiguration configuration)
        {
            // Try environment variables first (for containerized deployment)
            string? envVar = env.IsDevelopment()
                ? Environment.GetEnvironmentVariable("UI_UMS_URL_DEV")
                : Environment.GetEnvironmentVariable("UI_UMS_URL_PROD");

            if (!string.IsNullOrWhiteSpace(envVar))
            {
                Log.Information("CORS origin allowed: {Origin}", envVar);
                return envVar;
            }

            string? configValue = configuration.GetValue<string>("AllowedUIOrigin");
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                Log.Information("CORS origin allowed: {Origin}", configValue);
                return configValue;
            }

            Log.Fatal("No CORS origin configured or defined.");
            throw new InvalidOperationException("No CORS origin configured for the current environment.");
        }

        // ---------------------------------------------------------------
        //  Event Bus Configuration
        // ---------------------------------------------------------------

        private static void ConfigureEventBusSubscriptions(WebApplication app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();

            //Scoped subscription helper
            void SubscribeScoped<TEvent, TService>(Func<TService, TEvent, Task> handler)
                where TEvent : IUserDomainEvent
                where TService : notnull
            {
                eventBus.Subscribe<TEvent>(async evt =>
                {
                    using var scope = app.Services.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    await handler(service, evt);
                });
            }

            //Audit Event Subscriptions
            SubscribeScoped<UserCreatedEvent, IAuditService>((service, evt) => service.Handle(evt));
            SubscribeScoped<UserUpdatedEvent, IAuditService>((service, evt) => service.Handle(evt));
            SubscribeScoped<UserDeletedEvent, IAuditService>((service, evt) => service.Handle(evt));
        }
    }
}
