using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats;

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
<<<<<<< HEAD:src/Rise.Persistence/Chats/MessageConfiguration.cs
        builder.Property(m => m.Inhoud)
=======
        builder.Property(m => m.Text)
>>>>>>> codex/add-alert-message-for-supervisor-monitoring:src/Rise.Persistence/Configurations/Chats/MessageConfiguration.cs
            .HasMaxLength(2000);

        builder.Property(m => m.AudioContentType)
            .HasMaxLength(128);

        builder.Property(m => m.AudioDurationSeconds);
    }
}