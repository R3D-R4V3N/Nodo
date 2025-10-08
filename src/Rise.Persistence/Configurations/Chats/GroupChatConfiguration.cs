using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Chats;
using Rise.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Chats
{
    internal class GroupChatConfiguration : EntityConfiguration<GroupChat>
    {
        public override void Configure(EntityTypeBuilder<GroupChat> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.GroupName).HasMaxLength(100);


            builder.HasMany(x => x.Users)
                   .WithMany();

            builder.HasMany(x => x.Messages)
               .WithOne()
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
