using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats;

internal class ChatConfiguration : EntityConfiguration<Chat>
{
    public override void Configure(EntityTypeBuilder<Chat> builder)
    {
        base.Configure(builder);

        // Een Chat heeft één of meerdere Messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)       // elke Message hoort bij exact 1 Chat
            .HasForeignKey("ChatId")    //shadow fk
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(c => c.Users)
            .WithMany(u => u.Chats);
    }
}