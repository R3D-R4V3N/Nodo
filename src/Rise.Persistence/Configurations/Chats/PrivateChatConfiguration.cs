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
    internal class PrivateChatConfiguration : EntityConfiguration<PrivateChat>
    {
        public override void Configure(EntityTypeBuilder<PrivateChat> builder)
        {
            base.Configure(builder);

            builder.HasMany(x => x.Users)
                .WithMany();

            builder.HasMany(x => x.Messages)
               .WithOne()
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
