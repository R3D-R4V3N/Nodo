using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats;

internal class EmergencyConfiguration : EntityConfiguration<Emergency>
{
    public override void Configure(EntityTypeBuilder<Emergency> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.SnapshotEndTime)
            .HasDefaultValue(DateTime.Now);

        builder.Property(x => x.HasBeenResolved)
            .HasDefaultValue(false);

        builder.HasMany(x => x.HandledBy)
               .WithMany();

        builder.Ignore(x => x.SnapshotStartTime);
    }
}
