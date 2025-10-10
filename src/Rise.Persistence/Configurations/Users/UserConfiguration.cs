using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Projects;
using Rise.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Users
{
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

            // connections
            //builder.HasMany<ApplicationUser>("friends").WithMany();
            //builder.HasMany<ApplicationUser>("friendRequests").WithMany();
            //builder.HasMany<ApplicationUser>("blockedUsers").WithMany();

            //builder.Navigation(e => e.Friends)
            //    .UsePropertyAccessMode(PropertyAccessMode.Field);

            //builder.Navigation(e => e.FriendRequests)
            //    .UsePropertyAccessMode(PropertyAccessMode.Field);

            //builder.Navigation(e => e.BlockedUsers)
            //    .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
