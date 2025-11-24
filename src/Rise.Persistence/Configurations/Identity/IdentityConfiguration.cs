using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rise.Persistence.Configurations.Identity;

/// <summary>
/// Configuration for the Identity tables.
/// </summary>
internal class IdentityConfiguration :
    IEntityTypeConfiguration<IdentityUser>,
    IEntityTypeConfiguration<IdentityRole>,
    IEntityTypeConfiguration<IdentityUserRole<string>>,
    IEntityTypeConfiguration<IdentityUserClaim<string>>,
    IEntityTypeConfiguration<IdentityUserLogin<string>>,
    IEntityTypeConfiguration<IdentityRoleClaim<string>>,
    IEntityTypeConfiguration<IdentityUserToken<string>>
{
    // Configures the IdentityUser tables.
    // NOTE:
    // If you want to use a separate schema for your Identity tables,
    // you can specify the schema as "auth" like this:
    //     builder.ToTable("Users", "auth");
    // However, be aware that SQLite does NOT support schemas, so this only works with SQL Server, MarioDB, PostgreSQL,...
    // The default below will work on any provider.
    
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
        => builder.ToTable("IdentityUsers");

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
        => builder.ToTable("IdentityRoles");

    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        => builder.ToTable("IdentityUserRoles");

    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
        => builder.ToTable("IdentityUserClaims");

    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
        => builder.ToTable("IdentityUserLogins");

    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
        => builder.ToTable("IdentityRoleClaims");

    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
        => builder.ToTable("IdentityUserTokens");
}