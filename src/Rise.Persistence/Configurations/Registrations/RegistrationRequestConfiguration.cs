using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Registrations;

namespace Rise.Persistence.Configurations.Registrations;

internal class RegistrationRequestConfiguration : EntityConfiguration<RegistrationRequest>
{
    public override void Configure(EntityTypeBuilder<RegistrationRequest> builder)
    {
        base.Configure(builder);

        builder.ToTable("RegistrationRequests");

        builder.OwnsOne(m => m.Email, email =>
        {
            email.Property(t => t.Value)
                .HasColumnName("Email")
                .HasMaxLength(Email.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.FirstName, firstname =>
        {
            firstname.Property(t => t.Value)
                .HasColumnName("FirstName")
                .HasMaxLength(FirstName.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.LastName, lastName =>
        {
            lastName.Property(t => t.Value)
                .HasColumnName("LastName")
                .HasMaxLength(LastName.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.AvatarUrl, bio =>
        {
            bio.Property(t => t.Value)
                .HasColumnName("AvatarUrl")
                .HasColumnType("longtext")
                .HasMaxLength(BlobUrl.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.BirthDay, bd =>
        {
            bd.Property(t => t.Value)
                .HasColumnName("BirthDay");
        });

        builder.Property(r => r.Gender)
            .IsRequired();

        builder.Property(r => r.PasswordHash)
            .IsRequired();

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedSupervisor)
            .WithMany()
            .HasForeignKey("AssignedSupervisorId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(u => u.Status, status =>
        {
            status.WithOwner(s => s.Request)
                .HasForeignKey("RequestId");

            status.Property(s => s.StatusType)
                .HasDefaultValue(RegistrationStatusType.Pending)
                .HasColumnName("StatusType");

            status.HasOne(s => s.HandledBy)
                .WithMany()
                .HasForeignKey("HandledById");

            status.Property<int?>("HandledById")
                .HasColumnName("HandledById");

            status.Property(s => s.HandledDate)
                .HasDefaultValue(DateTime.UtcNow)
                .HasColumnName("HandledDate");

            status.OwnsOne(m => m.Note, note =>
            {
                note.Property(t => t.Value)
                    .HasMaxLength(RegistrationNote.MAX_LENGTH)
                    .HasColumnName("Note");
            });
        });
    }
}
