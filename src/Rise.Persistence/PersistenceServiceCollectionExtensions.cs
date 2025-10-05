using EntityFrameworkCore.Triggered.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Rise.Persistence.Triggers;

namespace Rise.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var connectionString = configuration.GetConnectionString("DatabaseConnection")
                              ?? throw new InvalidOperationException("Connection string 'DatabaseConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            options.EnableDetailedErrors();

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }

            options.UseTriggers(triggerOptions => triggerOptions.AddTrigger<EntityBeforeSaveTrigger>());
        });

        return services;
    }
}
