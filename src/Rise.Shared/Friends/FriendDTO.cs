namespace Rise.Shared.Friends;

public sealed class FriendDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Biography { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public FriendState State { get; set; } = FriendState.All;
}
