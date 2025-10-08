using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rise.Domain.Events;

namespace Rise.Persistence.Configurations.Events
{
    internal class EventConfiguration : EntityConfiguration<Event>
    {
        public override void Configure(EntityTypeBuilder<Event> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DateTime)
                .IsRequired();

            builder.Property(x => x.Location)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Likes)
                .HasDefaultValue(0)
                .IsRequired();

            builder.HasMany(x => x.LikedBy)
               .WithMany();
        }
    }
}
