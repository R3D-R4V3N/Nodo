namespace Rise.Shared.UserConnections;

/// <summary>
/// Contains data transfer objects (DTOs) used for friend-related operations.
/// </summary>
public class UserConnectionDTO
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string AvatarUrl { get; set; } = "";
    public UserConnectionTypeDto State { get; set; } = UserConnectionTypeDto.Friend;
}