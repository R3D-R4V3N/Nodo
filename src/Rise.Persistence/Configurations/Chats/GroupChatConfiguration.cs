using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats
{
    internal class GroupChatConfiguration : EntityConfiguration<GroupChat>
    {
        public override void Configure(EntityTypeBuilder<GroupChat> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.GroupName).HasMaxLength(100);
            builder.Ignore(x => x.Users);
            builder.Ignore(x => x.Messages);
        }
    }
}
