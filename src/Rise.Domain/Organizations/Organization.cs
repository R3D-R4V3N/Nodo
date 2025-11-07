using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Users;
using Rise.Domain.Users.Registrations;
using System.Collections.Generic;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    public Organization() { }

    public Organization(string name, string? description = null)
    {
        UpdateName(name);
        UpdateDescription(description);
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ICollection<Supervisor> Supervisors { get; private set; } = new List<Supervisor>();

    public ICollection<User> Users { get; private set; } = new List<User>();

    public ICollection<UserRegistrationRequest> RegistrationRequests { get; private set; } = new List<UserRegistrationRequest>();

    public void UpdateName(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    internal void AddSupervisor(Supervisor supervisor)
    {
        if (!Supervisors.Contains(supervisor))
        {
            Supervisors.Add(supervisor);
        }
    }

    internal void AddUser(User user)
    {
        if (!Users.Contains(user))
        {
            Users.Add(user);
        }
    }

    internal void AddRegistrationRequest(UserRegistrationRequest request)
    {
        if (!RegistrationRequests.Contains(request))
        {
            RegistrationRequests.Add(request);
        }
    }
}
