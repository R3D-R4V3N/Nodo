using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    // ef
    private Organization() { }

    public Organization(string name, string? description = null)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description?.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private readonly List<User> _members = [];
    public IReadOnlyCollection<User> Members => _members.AsReadOnly();
    private readonly List<Supervisor> _workers = [];
    public IReadOnlyCollection<Supervisor> Workers => _workers.AsReadOnly();

    public void UpdateDetails(string name, string? description)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description?.Trim();
    }

    internal void AddMember(User member)
    {
        if (_members.Contains(member))
        {
            return;
        }

        _members.Add(member);

        if (member.Organization != this)
        {
            member.Organization = this;
        }
    }
    internal void AddWorker(Supervisor worker)
    {
        if (_workers.Contains(worker))
        {
            return;
        }

        _workers.Add(worker);

        if (worker.Organization != this)
        {
            worker.Organization = this;
        }
    }
}
