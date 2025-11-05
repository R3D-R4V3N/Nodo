using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Messages;

namespace Rise.Persistence.Configurations.Messages;

internal class MessageConfiguration : EntityConfiguration<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        base.Configure(builder);

        // Elke Message hoort bij exact 1 Chat
        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey("ChatId") // shadow fk
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey("SenderId") // shadow fk
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Extra properties configureren (optioneel)
        builder.Property(m => m.Text)
            .HasConversion(
                new PropertyConverter<Domain.Messages.Properties.Text, string>()
            )
            .HasMaxLength(Domain.Messages.Properties.Text.MAX_LENGTH);

        builder.Property(m => m.AudioContentType)
            .HasMaxLength(128);

        builder.Property(m => m.AudioDurationSeconds);
    }
}