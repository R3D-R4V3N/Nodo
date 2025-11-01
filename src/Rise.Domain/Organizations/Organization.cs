using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    private string _name = default!;
    private string _location = default!;

    public required string Name
    {
        get => _name;
        set => _name = Guard.Against.NullOrWhiteSpace(value);
    }

    public required string Location
    {
        get => _location;
        set => _location = Guard.Against.NullOrWhiteSpace(value);
    }

    public ICollection<ApplicationUser> Members { get; private set; } = new List<ApplicationUser>();

    public IEnumerable<ApplicationUser> Supervisors => Members.Where(member => member.IsSupervisor());
}
