using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;

namespace Rise.Persistence.Configurations.Users;

internal class UserConfiguration : EntityConfiguration<ApplicationUser>
{
    public override void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
        builder.HasIndex(x => x.AccountId).IsUnique();

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Biography).IsRequired().HasMaxLength(500);
        builder.Property(x => x.BirthDay).IsRequired();
        builder.Property(x => x.UserType).IsRequired();

        // db just needs _connections
        builder.Ignore(u => u.Connections);
        builder.Ignore(u => u.Friends);
        builder.Ignore(u => u.FriendRequests);
        builder.Ignore(u => u.BlockedUsers);

        builder.OwnsMany<UserConnection>("_connections", connections =>
        {
            connections.WithOwner()
                        .HasForeignKey("UserId");

            // shadow key
            connections.Property<int>("Id");
            connections.HasKey("Id");

            connections.Property(c => c.ConnectionType)
                        .HasConversion<string>()
                        .IsRequired();

            connections.Property(c => c.CreatedAt)
                .IsRequired();

            connections.Property<int>("UserConnectionId")
                        .IsRequired();

            connections.HasOne(c => c.Connection)
                        .WithMany()
                        .HasForeignKey("UserConnectionId")
                        .OnDelete(DeleteBehavior.Cascade);

            connections.ToTable("UserConnections");
        });
    }
}
