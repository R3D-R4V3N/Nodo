using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats;

internal class ChatMessageHistoryConfiguration : IEntityTypeConfiguration<ChatMessageHistory>
{
    public void Configure(EntityTypeBuilder<ChatMessageHistory> builder)
    {
        builder.ToTable("ChatMessageHistories");

        builder
            .HasOne(history => history.Chat)
            .WithMany()
            .HasForeignKey(history => history.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(history => history.User)
            .WithMany()
            .HasForeignKey(history => history.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(history => new { history.ChatId, history.UserId })
            .IsUnique();
    }
}
