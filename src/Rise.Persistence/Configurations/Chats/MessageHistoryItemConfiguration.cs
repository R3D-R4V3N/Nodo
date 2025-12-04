using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Domain.Users.Connections;

namespace Rise.Persistence.Configurations.Chats;

internal class MessageHistoryItemConfiguration : EntityConfiguration<MessageHistoryItem>
{
    public override void Configure(EntityTypeBuilder<MessageHistoryItem> builder)
    {
        builder.ToTable("MessageHistoryItems");

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey("UserId")
            .IsRequired();

        builder
            .HasOne(x => x.LastReadMessage)
            .WithMany()
            .HasForeignKey("LastReadMessageId")
            .IsRequired();

        builder
            .HasOne(x => x.Chat)
            .WithMany(c => c.ReadHistory)
            .HasForeignKey("ChatId")
            .IsRequired();
    }
}