using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users.Hobbys;

namespace Rise.Persistence.Configurations.Users.Hobbies;

internal sealed class UserHobbyConfiguration : EntityConfiguration<UserHobby>
{
    public override void Configure(EntityTypeBuilder<UserHobby> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.Hobby)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(s => s.Hobby)
            .IsUnique();

        builder.ToTable("Hobbies");
    }
}
