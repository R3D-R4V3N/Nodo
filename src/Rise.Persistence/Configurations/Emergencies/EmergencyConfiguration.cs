using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Emergencies;
using Rise.Domain.Events;
using Rise.Domain.Users;

namespace Rise.Persistence.Configurations.Emergencies;

internal class EmergencyConfiguration : EntityConfiguration<Emergency>
{
    public override void Configure(EntityTypeBuilder<Emergency> builder)
    {
        base.Configure(builder);
        builder.ToTable("Emergencies");

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.Type)
            .IsRequired();

        builder.OwnsOne(e => e.Range, range =>
        {
            range.Property(r => r.End)
                .HasColumnName("EndRangeValue")
                .IsRequired();

            range.HasIndex(r => r.End);
        });

        builder.HasOne(e => e.HappenedInChat)
            .WithMany(c => c.Emergencies)
            .HasForeignKey("ChatId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MadeByUser)
            .WithMany()
            .HasForeignKey("MadeByUserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(e => e.AllowedToResolve)
            .WithMany()
            .UsingEntity(j => j.ToTable("Emergcency_Supervisors_AllowedToResolve"));

        builder
            .HasMany(e => e.HasResolved)
            .WithMany()
            .UsingEntity(j => j.ToTable("Emergcency_Supervisors_HasResolved"));
    }
}