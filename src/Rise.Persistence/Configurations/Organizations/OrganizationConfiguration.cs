using Microsoft.EntityFrameworkCore;
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

        builder.OwnsOne(organization => organization.Location, location =>
        {
            location.Property(l => l.Name)
                .HasColumnName("LocationName")
                .IsRequired()
                .HasMaxLength(OrganizationLocation.NAME_MAX_LENGTH);

            location.Property(l => l.ZipCode)
                .HasColumnName("LocationZipCode")
                .IsRequired()
                .HasMaxLength(OrganizationLocation.ZIPCODE_MAX_LENGTH);

            location.Property(l => l.City)
                .HasColumnName("LocationCity")
                .HasMaxLength(OrganizationLocation.CITY_MAX_LENGTH);

            location.Property(l => l.Street)
                .HasColumnName("LocationStreet")
                .HasMaxLength(OrganizationLocation.STREET_MAX_LENGTH);
        });
    }
}
