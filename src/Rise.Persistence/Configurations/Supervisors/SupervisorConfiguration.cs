using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Message;
using Rise.Domain.Supervisors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Supervisors
{
    internal class SupervisorConfiguration : EntityConfiguration<Supervisor>
    {
        public override void Configure(EntityTypeBuilder<Supervisor> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
            builder.HasIndex(x => x.AccountId).IsUnique();

            builder
                .HasMany(x => x.Chats)
                .WithMany();
        }
    }
}
