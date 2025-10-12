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
        builder.Property(x => x.UserType).IsRequired();
        builder.Property(x => x.OrganizationId).IsRequired();

        builder
            .HasMany<ApplicationUser>("Friends")
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserFriends",
                j => j
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("FriendId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("AccountId")
                    .OnDelete(DeleteBehavior.ClientCascade));

        builder
            .HasMany<ApplicationUser>("FriendRequests")
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserFriendRequests",
                j => j
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("FriendRequestId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("AccountId")
                    .OnDelete(DeleteBehavior.ClientCascade));
    }
}
