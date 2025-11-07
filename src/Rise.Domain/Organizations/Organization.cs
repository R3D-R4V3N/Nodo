using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class Organization : Entity
{
    public required string Name { get; set; }

    private readonly List<Supervisor> _supervisors = [];
    public IReadOnlyCollection<Supervisor> Supervisors => _supervisors;

    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users;
}
