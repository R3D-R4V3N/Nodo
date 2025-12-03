using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Events;
using Rise.Domain.Users;

namespace Rise.Persistence.Configurations.Events;

internal class EventConfiguration : EntityConfiguration<Event>
{
    public override void Configure(EntityTypeBuilder<Event> builder)
    {
        base.Configure(builder);
        builder.ToTable("Events");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Location)
            .HasMaxLength(256);

        builder.Property(e => e.Description)
            .HasMaxLength(2048);

        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(1024);

        // Many-to-many: Event <-> User
        builder
            .HasMany(e => e.InterestedUsers)
            .WithMany(u => u.InterestedInEvents)
            .UsingEntity<Dictionary<string, object>>(
                "Event_User_InterestedUsers",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Event>()
                    .WithMany()
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("EventId", "UserId");
                    join.ToTable("EventInterestedUsers");
                }
            );
    }
}