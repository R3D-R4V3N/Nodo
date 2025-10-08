using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Supervisors;
using Rise.Domain.Users;
using System;
using System.Linq;

namespace Rise.Persistence.Configurations.Users
{
    internal class ApplicationUserConfiguration : EntityConfiguration<ApplicationUser>
    {
        public override void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Biography)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);
            builder.Property(x => x.Birthday)
                .IsRequired();
            builder.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Hobbys)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasColumnName("Hobbys");

            builder.Property(x => x.Likes)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasColumnName("Likes");

            builder.Property(x => x.Dislikes)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasColumnName("Dislikes");

            builder
                .HasMany(x => x.Friends)
                .WithMany();

            builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
            builder.HasIndex(x => x.AccountId).IsUnique();

            builder.Ignore(x => x.Chats);

            builder.Property(x => x.Supervisor)
                .IsRequired();

            builder.HasMany(x => x.IntrestedEvents)
                .WithMany();

            builder.Property(x => x.Settings)
                .IsRequired();
        }
    }
}
