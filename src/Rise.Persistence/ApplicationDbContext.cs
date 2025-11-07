using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Messages;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence.Configurations.Users;

namespace Rise.Persistence;

/// <summary>
/// Entrance to the database, inherits from IdentityDbContext and is basically a Unit Of Work and Repository pattern combined.
/// A <see cref="DbSet"/> is a repository for a specific type of entity.
/// The <see cref="ApplicationDbContext"/> is the Unit Of Work pattern
/// Will look very similar when switching database providers.
/// See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1
/// See https://enterprisecraftsmanship.com/posts/should-you-abstract-database/
/// </summary>
/// <param name="opts"></param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : IdentityDbContext<IdentityUser>(opts)
{
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Supervisor> Supervisors => Set<Supervisor>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<UserRegistration> UserRegistrations => Set<UserRegistration>();
    public DbSet<UserConnection> UserConnections => Set<UserConnection>();
    public DbSet<UserHobby> Hobbies => Set<UserHobby>();
    public DbSet<UserSentiment> Sentiments => Set<UserSentiment>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All columns in the database have a maxlength of 200.
        configurationBuilder.Properties<string>().HaveMaxLength(200);
        // All decimals columns should have 2 digits after the comma
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applying all types of IEntityTypeConfiguration in the Persistence project.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
