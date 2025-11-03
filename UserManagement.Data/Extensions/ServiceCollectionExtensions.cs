using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UserManagement.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            //inMemory DB for dev
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("UserManagement.Data.DataContext"));
        }
        else
        {
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_UMS_CONNECTION")
                       ?? configuration.GetConnectionString("DefaultUMSConnection");


            // MySQL for prod
            try
            {
                services.AddDbContext<DataContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not connect to DB.", ex);
            }
        }
        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<DataContext>());

        return services;
    }
}