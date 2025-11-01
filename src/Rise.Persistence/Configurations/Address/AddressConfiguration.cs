using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Locations;
using Rise.Domain.Locations.Properties;

namespace Rise.Persistence.Configurations.Messages;

internal class AddressConfiguration : EntityConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        base.Configure(builder);
        builder.ToTable("Addresses");


        builder.HasKey(a => a.Id);

        builder
            .Property(a => a.Province)
            .HasConversion(new ValueObjectConverter<Name, string>())
            .IsRequired()
            .HasColumnName("Province")
            .HasMaxLength(Name.MAX_LENGTH);

        builder
            .OwnsOne(a => a.City, city =>
            {
                city.Property(c => c.Name)
                    .HasConversion(new ValueObjectConverter<Name, string>())
                    .IsRequired()
                    .HasColumnName("CityName")
                    .HasMaxLength(Name.MAX_LENGTH);

                city.Property(c => c.ZipCode)
                    .HasConversion(new ValueObjectConverter<ZipCode, int>())
                    .IsRequired()
                    .HasColumnName("ZipCode");

                city.Property(c => c.Street)
                    .HasConversion(new ValueObjectConverter<Name, string>())
                    .IsRequired()
                    .HasColumnName("Street")
                    .HasMaxLength(Name.MAX_LENGTH);
            });

    }
}