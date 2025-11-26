using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rise.Persistence;
using Serilog;
using Serilog.Events;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Missing argument: dotnet run -- [migrate|reset]");
                return;
            }

            Log.Information("Starting database reset");

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    var cs = Environment.GetEnvironmentVariable("DB_CONNECTION")
                             ?? configuration.GetConnectionString("DatabaseConnection")
                             ?? throw new InvalidOperationException("No connection string found");

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseMySql(cs, ServerVersion.AutoDetect(cs))
                               .EnableDetailedErrors());

                    services
                        .AddIdentityCore<IdentityUser>(options => { })
                        .AddRoles<IdentityRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>();

                    services.AddScoped<DbSeeder>();
                })
                .UseSerilog()
                .Build();

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

            string action = args[0].ToLower();

            switch (action)
            {
                case "migrate":
                    Log.Information("Applying migrations...");
                    await db.Database.MigrateAsync();
                    Log.Information("Database migrations applied successfully.");

                    Log.Information("Running seeder.");
                    await seeder.SeedAsync();
                    Log.Information("Finished seeder.");
                    break;

                case "reset":
                    Log.Information("Deleting database...");
                    await db.Database.EnsureDeletedAsync();
                    Log.Information("Database deleted successfully.");

                    Log.Information("Applying migrations...");
                    await db.Database.MigrateAsync();
                    Log.Information("Database migrations applied successfully.");

                    Log.Information("Running seeder.");
                    await seeder.SeedAsync();
                    Log.Information("Finished seeder.");
                    break;

                default:
                    Console.WriteLine("Invalid argument. Use 'migrate', or 'reset'.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred during database reset");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
