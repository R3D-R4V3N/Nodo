
namespace Rise.Domain.Users.Connections;
public class UserConnection : Entity
{
    public required User From { get; set; }
    public required User To { get; set; }
    public required UserConnectionType ConnectionType { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is UserConnection connection &&
               IsDeleted == connection.IsDeleted &&
               EqualityComparer<User>.Default.Equals(From, connection.From) &&
               EqualityComparer<User>.Default.Equals(To, connection.To) &&
               ConnectionType == connection.ConnectionType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsDeleted, From, To, ConnectionType);
    }

    public override string ToString() => $"{From} -> {To}: {ConnectionType}";
}
