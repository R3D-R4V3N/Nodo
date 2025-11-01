using Rise.Domain.Locations.Properties;

namespace Rise.Domain.Locations;
public class City
{
    private Name _name = default!;
    public required Name Name
    {
        get => _name;
        set => _name = Guard.Against.Null(value);
    }

    private ZipCode _zipCode = default!;
    public required ZipCode ZipCode
    {
        get => _zipCode;
        set => _zipCode = Guard.Against.Null(value);
    }

    private Name _street = default!;
    public required Name Street
    {
        get => _street;
        set => _street = Guard.Against.Null(value);
    }

    public override string ToString()
        => $"{Name}: {ZipCode}, {Street}";
}
