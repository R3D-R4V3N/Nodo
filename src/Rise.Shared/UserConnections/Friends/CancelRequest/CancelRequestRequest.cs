namespace Rise.Shared.UserConnections;

public static partial class UserConnectionRequest
{
    public class CancelFriendRequest
    {
        public string TargetAccountId { get; set; }
    }
}