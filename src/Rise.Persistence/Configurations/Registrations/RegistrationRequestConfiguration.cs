using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Registrations;
using Rise.Domain.Users.Properties;

namespace Rise.Persistence.Configurations.Registrations;

internal class RegistrationRequestConfiguration : EntityConfiguration<RegistrationRequest>
{
    public override void Configure(EntityTypeBuilder<RegistrationRequest> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(RegistrationRequestStatus.Pending)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasConversion(new ValueObjectConverter<FirstName, string>())
            .IsRequired()
            .HasMaxLength(FirstName.MAX_LENGTH);

        builder.Property(x => x.LastName)
            .HasConversion(new ValueObjectConverter<LastName, string>())
            .IsRequired()
            .HasMaxLength(LastName.MAX_LENGTH);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedSupervisor)
            .WithMany()
            .HasForeignKey(x => x.AssignedSupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Feedback)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasIndex(x => x.AccountId)
            .IsUnique();
    }
}
