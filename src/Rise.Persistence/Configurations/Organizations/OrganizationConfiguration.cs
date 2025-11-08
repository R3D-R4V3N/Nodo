using Microsoft.EntityFrameworkCore;
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

        var membersNavigation = builder.Metadata.FindNavigation(nameof(Organization.Members));
        if (membersNavigation is not null)
        {
            membersNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
            membersNavigation.SetField("_members");
        }
    }
}
