using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;

namespace Rise.Persistence.Configurations.Organizations;

internal class OrganizationConfiguration : EntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.ToTable("Organizations");

        builder.Property(o => o.Name)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasMaxLength(500);
    }
}
