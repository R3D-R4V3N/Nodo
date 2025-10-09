namespace Rise.Shared.Friends;

public static class FriendResponse
{
    public class Index
    {
        public List<FriendDto> Friends { get; set; } = [];
        public List<FriendDto> Requests { get; set; } = [];
        public List<FriendDto> Suggestions { get; set; } = [];
    }
}
