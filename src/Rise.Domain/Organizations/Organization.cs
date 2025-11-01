using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Organizations.Properties;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    private OrganizationName _name = default!;
    private OrganizationLocation _location = default!;
    private readonly List<BaseUser> _members = [];

    public required OrganizationName Name
    {
        get => _name;
        set => _name = Guard.Against.Null(value);
    }

    public required OrganizationLocation Location
    {
        get => _location;
        set => _location = Guard.Against.Null(value);
    }

    public IReadOnlyCollection<BaseUser> Members => _members.AsReadOnly();

    public IEnumerable<Supervisor> Supervisors => _members.OfType<Supervisor>();
}
