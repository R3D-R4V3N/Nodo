using System.Collections.Generic;
using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Organizations.Properties;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    private OrganizationName _name = default!;
    private OrganizationLocation _location = default!;

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

    public List<User> Users { get; private set; } = [];

    public List<Supervisor> Supervisors { get; private set; } = [];
}
