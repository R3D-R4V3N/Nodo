namespace Rise.Shared.Friends;

public sealed class FriendDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string AvatarUrl { get; set; } = "";
    public FriendState State { get; set; } = FriendState.All;
}