namespace Rise.Shared.UserConnections;

/// <summary>
/// Represents the response structure for product-related operations.
/// </summary>
public static partial class UserConnectionResponse
{
    public class Index
    {
        public IEnumerable<UserConnectionDTO> Connections { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
