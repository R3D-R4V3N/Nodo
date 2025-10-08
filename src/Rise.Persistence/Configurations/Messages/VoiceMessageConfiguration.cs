using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Message;

namespace Rise.Persistence.Configurations.Messages;

internal class VoiceMessageConfiguration : EntityConfiguration<VoiceMessage>
{
    public override void Configure(EntityTypeBuilder<VoiceMessage> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Blob).IsRequired();
        builder.Property(x => x.Encoding).IsRequired();
        builder.Property(x => x.Length).IsRequired();
    }
}
