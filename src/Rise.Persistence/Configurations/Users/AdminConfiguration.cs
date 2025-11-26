using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;
using Rise.Persistence.Configurations.Users.Hobbies;
using Rise.Persistence.Configurations.Users.Sentiments;


namespace Rise.Persistence.Configurations.Users;

internal class AdminConfiguration : EntityConfiguration<Admin>
{
    public override void Configure(EntityTypeBuilder<Admin> builder)
    {
        base.Configure(builder);
        builder.ToTable("Admins");
    }
}
