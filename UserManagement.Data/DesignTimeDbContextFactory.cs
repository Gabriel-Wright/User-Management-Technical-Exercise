using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Data
{
    /// <summary>
    /// Used by EF Core tools for design-time migrations.
    /// This allows migrations to run WITHOUT needing a live database connection.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var webProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "UserManagement.Web");

            var builder = new ConfigurationBuilder()
                .SetBasePath(webProjectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            // Get connection string
            var connectionString =
                Environment.GetEnvironmentVariable("MIGRATION_UMS_CONNECTION")
                ?? config.GetConnectionString("MigrationUMSConnection");

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            //Use a fixed mySQL version - loaded from appsettings.json
            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(config["MySQL:ServerVersion"])
            );

            return new DataContext(optionsBuilder.Options);
        }
    }
}