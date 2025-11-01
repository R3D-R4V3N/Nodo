using Rise.Domain.Locations.Properties;

namespace Rise.Domain.Locations;

public class Address : Entity
{
    private Name _province = default!; 
    public required Name Province 
    { 
        get => _province; 
        set => _province = Guard.Against.Null(value); 
    }

    private City _city = default!; 
    public required City City 
    { 
        get => _city; 
        set => _city = Guard.Against.Null(value); 
    }

    public override string ToString()
        => $"{Province}, {City}";
}
