using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Chats;

internal class ChatConfiguration : EntityConfiguration<Chat>
{
    public override void Configure(EntityTypeBuilder<Chat> builder)
    {
        base.Configure(builder);

        // Een Chat heeft één of meerdere Messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)              // elke Message hoort bij exact 1 Chat
            .HasForeignKey(m => m.ChatId)      // FK in Message
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.IsSupervisorAlertActive)
            .HasDefaultValue(false);
    }
}