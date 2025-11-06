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
using UserManagement.Services.Events;
using UserMangement.Services.Events;

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
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowBlazorClient",
                        policy => policy
                            .WithOrigins(FindAllowedCorsOrigin(builder.Environment, builder.Configuration))
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                    );
                });
                //Add services to the container. - removing Views from this layer. Separate UI layer introduced.
                builder.Services
                    .AddDataAccess(builder.Configuration, builder.Environment)
                    .AddDomainServices()
                        .AddControllers();
                builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

                //Domain services
                builder.Services.AddScoped<UserService>();
                builder.Services.AddScoped<AuditService>();
                //Adding Swagger so we can check Web APIs
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });
                var app = builder.Build();
                app.UseCors("AllowBlazorClient");
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
                ConfigureEventBusSubscriptions(app);

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

        private static string FindAllowedCorsOrigin(IHostEnvironment env, IConfiguration configuration)
        {
            //Check environment-specific env var
            string? envVar = env.IsDevelopment()
                ? Environment.GetEnvironmentVariable("UI_URL_DEV")
                : Environment.GetEnvironmentVariable("UI_URL_PROD");

            //Use env var if set, otherwise fallback to configuration
            if (!string.IsNullOrWhiteSpace(envVar))
            {
                Log.Information("CORs origin allowed on {Origin}", envVar);
                return envVar;
            }

            string? configValue = configuration.GetValue<string>("AllowedUIOrigin");
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                Log.Information("CORs origin allowed on {Origin}", configValue);
                return configValue;
            }
            Log.Fatal("No CORS origin configured or defined");
            throw new InvalidOperationException("No CORS origin configured for the current environment.");
        }
        private static void ConfigureEventBusSubscriptions(WebApplication app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();

            //Have to call this for each subscription
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

            SubscribeScoped<UserCreatedEvent, AuditService>((service, evt) => service.Handle(evt));
            SubscribeScoped<UserUpdatedEvent, AuditService>((service, evt) => service.Handle(evt));
            SubscribeScoped<UserDeletedEvent, AuditService>((service, evt) => service.Handle(evt));

        }
    }



}