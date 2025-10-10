using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Users;

internal class UserSupervisorConfiguration : EntityConfiguration<UserSupervisor>
{
    public override void Configure(EntityTypeBuilder<UserSupervisor> builder)
    {
        base.Configure(builder);

        builder.HasOne(us => us.ChatUser)
            .WithMany(u => u.SupervisorLinks)
            .HasForeignKey(us => us.ChatUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.Supervisor)
            .WithMany(u => u.SupervisedUsers)
            .HasForeignKey(us => us.SupervisorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(us => new { us.ChatUserId, us.SupervisorId })
            .IsUnique();
    }
}
