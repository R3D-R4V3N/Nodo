using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Users;
public class UserConnection : ValueObject
{
    public UserConnection()
    {
    }

    public required ApplicationUser Connection { get; set; }
    public required UserConnectionType ConnectionType { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Connection;
        yield return ConnectionType;
    }
}

public enum UserConnectionType
{ 
    Friend,
    RequestIncoming,
    RequestOutgoing,
    Blocked,
}
