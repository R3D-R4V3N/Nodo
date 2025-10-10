using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Chats;

internal class ChatParticipantConfiguration : EntityConfiguration<ChatParticipant>
{
    public override void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        base.Configure(builder);

        builder.HasOne(cp => cp.Chat)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany(u => u.ChatParticipations)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cp => new { cp.ChatId, cp.UserId })
            .IsUnique();
    }
}
