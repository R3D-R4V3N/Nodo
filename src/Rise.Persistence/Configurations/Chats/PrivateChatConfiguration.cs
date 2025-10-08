using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats;

internal class PrivateChatConfiguration : EntityConfiguration<PrivateChat>
{
    public override void Configure(EntityTypeBuilder<PrivateChat> builder)
    {
        base.Configure(builder);

        builder.HasMany(x => x.Users)
               .WithMany(x => x.Chats);

        builder.HasMany(x => x.Messages)
               .WithOne(x => x.Chat)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
