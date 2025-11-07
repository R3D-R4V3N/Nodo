using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;

namespace Rise.Persistence.Configurations.Organizations;

internal class UserRegistrationConfiguration : EntityConfiguration<UserRegistration>
{
    public override void Configure(EntityTypeBuilder<UserRegistration> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRegistrations");

        builder.Property(r => r.AccountId)
            .IsRequired()
            .HasMaxLength(36);

        builder.HasIndex(r => r.AccountId).IsUnique();

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Status)
            .HasConversion<int>();

        builder.Property(r => r.RequestedAt)
            .HasColumnType("datetime")
            .HasDefaultValueSql("current_timestamp()");

        builder.Property(r => r.ApprovedAt)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedSupervisor)
            .WithMany()
            .HasForeignKey(r => r.AssignedSupervisorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
