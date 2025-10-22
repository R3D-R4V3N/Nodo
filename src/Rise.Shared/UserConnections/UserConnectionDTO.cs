using Rise.Shared.Users;

namespace Rise.Shared.UserConnections;

/// <summary>
/// Contains data transfer objects (DTOs) used for friend-related operations.
/// </summary>
public static class UserConnectionDto
{
    public record GetFriends 
    {
        public UserDto.Connection User { get; set; }
        public UserConnectionTypeDto State { get; set; } = UserConnectionTypeDto.Friend;
    }
}