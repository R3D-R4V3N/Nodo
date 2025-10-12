using System.Collections.Generic;
using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    public string Name { get; private set; }

    private readonly HashSet<ApplicationUser> _members = [];
    public IReadOnlyCollection<ApplicationUser> Members => _members;

    public Organization()
    {
        Name = string.Empty;
    }

    public Organization(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
    }

    public void AddMember(ApplicationUser user)
    {
        Guard.Against.Null(user);
        _members.Add(user);
    }
}
