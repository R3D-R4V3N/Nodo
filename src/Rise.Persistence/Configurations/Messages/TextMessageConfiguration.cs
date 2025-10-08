using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Events;
using Rise.Domain.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Messages
{
    internal class TextMessageConfiguration : EntityConfiguration<TextMessage>
    {
        public override void Configure(EntityTypeBuilder<TextMessage> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Timestamp)
                .HasDefaultValue(DateTime.Now)
                .IsRequired();

            builder.Property(x => x.Text)
                .HasMaxLength(500)
                .IsRequired();
        }
    }
}
