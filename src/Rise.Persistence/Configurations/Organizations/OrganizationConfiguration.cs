using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;
using Rise.Domain.Organizations.Properties;
using Rise.Persistence.Configurations;

namespace Rise.Persistence.Configurations.Organizations;

internal sealed class OrganizationConfiguration : EntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.Property(organization => organization.Name)
            .HasConversion(new ValueObjectConverter<OrganizationName, string>())
            .IsRequired()
            .HasMaxLength(OrganizationName.MAX_LENGTH);

        builder.Property(organization => organization.Location)
            .HasConversion(new ValueObjectConverter<OrganizationLocation, string>())
            .IsRequired()
            .HasMaxLength(OrganizationLocation.MAX_LENGTH);

        builder.HasMany(organization => organization.Members)
            .WithOne(user => user.Organization)
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(organization => organization.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(organization => organization.Supervisors);
    }
}
