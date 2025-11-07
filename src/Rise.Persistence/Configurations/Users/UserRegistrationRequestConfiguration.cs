using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users.Registrations;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Configurations.Users;

internal class UserRegistrationRequestConfiguration : EntityConfiguration<UserRegistrationRequest>
{
    public override void Configure(EntityTypeBuilder<UserRegistrationRequest> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRegistrationRequests");

        builder.Property(r => r.AccountId)
            .IsRequired()
            .HasMaxLength(36);

        builder.HasIndex(r => r.AccountId)
            .IsUnique();

        builder.Property(r => r.Email)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.DecisionNote)
            .HasMaxLength(UserRegistrationRequest.MaxNoteLength);

        builder.HasOne(r => r.Organization)
            .WithMany(o => o.RegistrationRequests)
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedSupervisor)
            .WithMany()
            .HasForeignKey(r => r.AssignedSupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ProcessedBySupervisor)
            .WithMany()
            .HasForeignKey(r => r.ProcessedBySupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(r => r.IsPending);
    }
}
