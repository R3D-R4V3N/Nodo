using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Persistence.Triggers;
using Testcontainers.MySql;

namespace Rise.Services.Tests.Helpers;
public class EFFixture : IAsyncLifetime
{
    private readonly MySqlContainer Container = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithCleanUp(true)
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithDatabase("testdb")
            .Build();
    public ApplicationDbContext CreateApplicationDbContext()
    {
        var cs = Container.GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(cs, ServerVersion.AutoDetect(cs))
            .UseTriggers(options => options.AddTrigger<EntityBeforeSaveTrigger>())
            .Options;

        return new ApplicationDbContext(options);
    }
    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var dbContext = CreateApplicationDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}