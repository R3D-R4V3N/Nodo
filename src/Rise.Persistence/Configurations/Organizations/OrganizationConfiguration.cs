using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;

namespace Rise.Persistence.Configurations.Organizations;

internal class OrganizationConfiguration : EntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder
            .HasMany(o => o.Members)
            .WithOne(u => u.Organization!)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
