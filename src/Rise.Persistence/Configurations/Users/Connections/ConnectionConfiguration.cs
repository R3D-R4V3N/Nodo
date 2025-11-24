using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Users.Connections;

internal class ConnectionConfiguration : EntityConfiguration<UserConnection>
{
    public override void Configure(EntityTypeBuilder<UserConnection> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserConnections");
        builder.HasQueryFilter(uc => !uc.IsDeleted);

        builder.HasOne(c => c.From)
            .WithMany()
            .HasForeignKey("FromId")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(c => c.To)
            .WithMany()
            .HasForeignKey("ToId")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Property(c => c.ConnectionType)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex("FromId", "ToId", nameof(UserConnection.IsDeleted));
    }
}
