using Rise.Domain.Organizations;

namespace Rise.Domain.Users;

public class Supervisor : BaseUser
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    private readonly List<User> _assignedUsers = [];
    public IReadOnlyCollection<User> AssignedUsers => _assignedUsers;
}
