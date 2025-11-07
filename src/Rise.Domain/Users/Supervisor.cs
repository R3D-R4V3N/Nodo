using Ardalis.GuardClauses;
using Rise.Domain.Organizations;
using System;

namespace Rise.Domain.Users;
public class Supervisor : BaseUser
{
    public int OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public void AssignToOrganization(Organization organization)
    {
        Organization = Guard.Against.Null(organization);
        if (OrganizationId != default && OrganizationId != organization.Id)
        {
            throw new InvalidOperationException("De organisatie van de begeleider kan niet worden gewijzigd.");
        }
        OrganizationId = organization.Id;
        organization.AddSupervisor(this);
    }
}
