using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Chats;

internal class MessageConfiguration : EntityConfiguration<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        base.Configure(builder);

        // Elke Message hoort bij exact 1 Chat
        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
<<<<<<< HEAD
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
=======
            .IsRequired()                       // verplicht: elke Message heeft een Chat
            .OnDelete(DeleteBehavior.Cascade);   // verwijder messages als chat verwijderd wordt
>>>>>>> origin/main

        // Extra properties configureren (optioneel)
        builder.Property(m => m.Inhoud)
            .IsRequired()
            .HasMaxLength(2000);
    }
}