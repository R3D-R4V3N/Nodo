namespace Rise.Shared.UserConnections;

public static partial class UserConnectionResponse
{
    public class GetFriends
    {
        public IEnumerable<UserConnectionDto.Get> Connections { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
