<<<<<<< HEAD
=======
using Rise.Shared.Users;

>>>>>>> codex/add-alert-message-for-supervisor-monitoring
namespace Rise.Shared.UserConnections;

/// <summary>
/// Contains data transfer objects (DTOs) used for friend-related operations.
/// </summary>
<<<<<<< HEAD
public class UserConnectionDTO
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string AvatarUrl { get; set; } = "";
    public UserConnectionTypeDto State { get; set; } = UserConnectionTypeDto.Friend;
=======
public static class UserConnectionDto
{
    public record GetFriends 
    {
        public UserDto.Connection User { get; set; }
        public UserConnectionTypeDto State { get; set; } = UserConnectionTypeDto.Friend;
    }
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
}