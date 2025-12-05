using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Notifications;

namespace Rise.Persistence.Configurations.Notifications;

internal class NotificationSubscriptionConfiguration : EntityConfiguration<NotificationSubscription>
{
    public override void Configure(EntityTypeBuilder<NotificationSubscription> builder)
    {
        base.Configure(builder);
        builder.ToTable("NotificationSubscriptions");

        builder.Property(sub => sub.Endpoint)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(sub => sub.P256dh)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sub => sub.Auth)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(sub => new { sub.UserId, sub.Endpoint })
            .IsUnique();

        builder.HasOne(sub => sub.User)
            .WithMany()
            .HasForeignKey(sub => sub.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
