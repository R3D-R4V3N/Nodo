using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;

namespace Rise.Persistence.Configurations.Chats
{
    internal class SupervisorChatConfiguration : EntityConfiguration<SupervisorChat>
    {
        public override void Configure(EntityTypeBuilder<SupervisorChat> builder)
        {
            base.Configure(builder);

            builder.Ignore(x => x.Users);
            builder.Ignore(x => x.Messages);
        }
    }
}
