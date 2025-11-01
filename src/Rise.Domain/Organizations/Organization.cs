using Rise.Domain.Locations;
using Rise.Domain.Organizations.Properties;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    public Organization() { }

    private Name _name = default!;
    public required Name Name
    {
        get => _name;
        set => _name = Guard.Against.Null(value);
    }

    private Address _address = default!;
    public required Address Address
    {
        get => _address;
        set => _address = Guard.Against.Null(value);
    }

    private List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users;

    private List<Supervisor> _supervisors = [];
    public IReadOnlyCollection<Supervisor> Supervisors => _supervisors;
}
