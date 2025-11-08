using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Registrations;

namespace Rise.Persistence.Configurations.Registrations;

internal class RegistrationRequestConfiguration : EntityConfiguration<RegistrationRequest>
{
    public override void Configure(EntityTypeBuilder<RegistrationRequest> builder)
    {
        base.Configure(builder);

        builder.ToTable("RegistrationRequests");

        builder.Property(r => r.Email)
            .IsRequired();

        builder.Property(r => r.NormalizedEmail)
            .IsRequired();

        builder.Property(r => r.FirstName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.LastName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.FullName)
            .IsRequired();

        builder.Property(r => r.BirthDate)
            .IsRequired();

        builder.Property(r => r.Gender)
            .IsRequired();

        builder.Property(r => r.AvatarUrl)
            .IsRequired()
            .HasColumnType("longtext");

        builder.Property(r => r.PasswordHash)
            .IsRequired();

        builder.HasIndex(r => r.NormalizedEmail)
            .IsUnique();

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedSupervisor)
            .WithMany()
            .HasForeignKey(r => r.AssignedSupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ApprovedBySupervisor)
            .WithMany()
            .HasForeignKey(r => r.ApprovedBySupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
