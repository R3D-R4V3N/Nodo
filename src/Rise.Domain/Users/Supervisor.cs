using System.Collections.Generic;

namespace Rise.Domain.Users;

public class Supervisor : BaseUser
{
    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    internal void AttachUser(User user)
    {
        if (!_users.Contains(user))
        {
            _users.Add(user);
        }
    }

    internal void DetachUser(User user)
    {
        _users.Remove(user);
    }
}
