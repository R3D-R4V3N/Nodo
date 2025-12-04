using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Domain.Users.Connections;

namespace Rise.Persistence.Configurations.Chats;

internal class ChatConfiguration : EntityConfiguration<Chat>
{
    public override void Configure(EntityTypeBuilder<Chat> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.ChatType)
            .HasConversion<string>()
            .IsRequired();

        // Een Chat heeft één of meerdere Messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)       // elke Message hoort bij exact 1 Chat
            .HasForeignKey("ChatId")    //shadow fk
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(c => c.Users)
            .WithMany(u => u.Chats);

        builder.HasMany(c => c.ReadHistory)
            .WithOne(h => h.Chat)
            .HasForeignKey("ChatId")
            .OnDelete(DeleteBehavior.Cascade);

        // chatid1 seems to be gone without this
        //builder.Navigation(c => c.ReadHistory)
        //    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.ChatType);
    }
}