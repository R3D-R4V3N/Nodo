using System;
using System.IO;
using EntityFrameworkCore.Triggered;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Rise.Persistence.Triggers;

namespace Rise.Persistence;

/// <summary>
/// Provides <see cref="ApplicationDbContext"/> instances for design-time tooling such as migrations.
/// </summary>
internal class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DefaultConnectionString =
        """Server=localhost;Port=3306;Database=rise;User=root;Password=rise;TreatTinyAsBoolean=true;AllowPublicKeyRetrieval=True;SslMode=None;DefaultCommandTimeout=180;""";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION") ??
            configuration.GetConnectionString("DatabaseConnection") ??
            DefaultConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var configuredVersion = Environment.GetEnvironmentVariable("DB_SERVER_VERSION")
                               ?? configuration["Database:ServerVersion"];

        var serverVersion = MySqlServerVersionResolver.Resolve(connectionString, configuredVersion);

        optionsBuilder
            .UseMySql(connectionString, serverVersion)
            .UseTriggers(static options => options.AddTrigger<EntityBeforeSaveTrigger>());

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var basePath = Directory.GetCurrentDirectory();
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

        var serverProjectPath = Path.Combine(basePath, "..", "Rise.Server");
        if (Directory.Exists(serverProjectPath))
        {
            configurationBuilder
                .AddJsonFile(Path.Combine(serverProjectPath, "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(serverProjectPath, $"appsettings.{environmentName}.json"), optional: true);
        }

        return configurationBuilder
            .AddEnvironmentVariables()
            .Build();
    }
}
