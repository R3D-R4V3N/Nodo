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
        builder.ToTable("Chats");

        builder.Property(x => x.ChatType)
            .HasConversion<string>()
            .IsRequired();

        // Een Chat heeft één of meerdere Messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)       // elke Message hoort bij exact 1 Chat
            .HasForeignKey("ChatId")    //shadow fk
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity(j => j.ToTable("BaseUser_Chat"));

        builder.HasMany(c => c.Emergencies)
            .WithOne(m => m.HappenedInChat)       
            .HasForeignKey("ChatId")    
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ChatType);
    }
}