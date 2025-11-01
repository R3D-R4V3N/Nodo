using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Locations;
using Rise.Domain.Organizations;

namespace Rise.Persistence.Configurations.Organizations;

internal sealed class OrganizationConfiguration : EntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);
        builder.HasKey(a => a.Id);

        builder.Property(organization => organization.Name)
            .HasConversion(
                new ValueObjectConverter<Domain.Organizations.Properties.Name, string>()
            ).IsRequired()
            .HasMaxLength(Domain.Organizations.Properties.Name.MAX_LENGTH);

        builder.HasOne(o => o.Address)
               .WithMany() 
               .HasForeignKey("AddressId") // Shadow property for FK
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // Users collection
        builder.HasMany(o => o.Users)
            .WithOne()
            .HasForeignKey("OrganizationId");

        // Supervisors collection
        builder.HasMany(o => o.Supervisors)
            .WithOne()
            .HasForeignKey("OrganizationId");
    }
}
