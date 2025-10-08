using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Message;

namespace Rise.Persistence.Configurations.Messages;

internal class TextMessageConfiguration : EntityConfiguration<TextMessage>
{
    public override void Configure(EntityTypeBuilder<TextMessage> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Timestamp)
            .HasDefaultValue(DateTime.Now)
            .IsRequired();

        builder.Property(x => x.Text)
            .HasMaxLength(500)
            .IsRequired();
    }
}
