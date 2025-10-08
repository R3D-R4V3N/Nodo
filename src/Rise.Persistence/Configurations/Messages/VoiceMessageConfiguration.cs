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
    internal class VoiceMessageConfiguration : EntityConfiguration<VoiceMessage>
    {
        public override void Configure(EntityTypeBuilder<VoiceMessage> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Blob).IsRequired();
            builder.Property(x => x.Encoding).IsRequired();
            builder.Property(x => x.Length).IsRequired();
        }
    }
}
