namespace Rise.Shared.UserConnections;

/// <summary>
/// Represents the response structure for product-related operations.
/// </summary>
public static partial class UserConnectionResponse
{
    public class GetFriends
    {
        public IEnumerable<UserConnectionDto.GetFriends> Connections { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
