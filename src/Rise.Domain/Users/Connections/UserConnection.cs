
namespace Rise.Domain.Users.Connections;
public class UserConnection : ValueObject
{
    public required ApplicationUser Connection { get; set; }
    public required UserConnectionType ConnectionType { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Connection;
        yield return ConnectionType;
    }
    public override string ToString()
        => $"{Connection}: {ConnectionType}";
}
