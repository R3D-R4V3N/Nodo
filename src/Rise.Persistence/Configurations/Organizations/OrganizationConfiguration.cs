using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Configurations.Organizations;

internal sealed class OrganizationConfiguration : EntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.Property(organization => organization.Name)
            .IsRequired();

        builder.Property(organization => organization.Location)
            .IsRequired();

        builder.HasMany(organization => organization.Members)
            .WithOne(user => user.Organization)
            .HasForeignKey(user => user.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
