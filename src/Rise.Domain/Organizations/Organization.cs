using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    private readonly List<BaseUser> _members = [];

    private Organization() { }

    public Organization(string name, string? description = null)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description?.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<BaseUser> Members => _members.AsReadOnly();

    public void UpdateDetails(string name, string? description)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description?.Trim();
    }

    internal void AddMember(BaseUser member)
    {
        if (_members.Contains(member))
        {
            return;
        }

        _members.Add(member);
    }
}
