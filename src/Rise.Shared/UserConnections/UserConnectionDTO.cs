using Rise.Shared.Users;

namespace Rise.Shared.UserConnections;

/// <summary>
/// Contains data transfer objects (DTOs) used for friend-related operations.
/// </summary>
public static class UserConnectionDto
{
    public record Get 
    {
        public UserDto.Connection User { get; set; }
        public UserConnectionTypeDto State { get; set; }
    }

    public record getProfile
    {
        public UserDto.ConnectionProfile User { get; set; } = default!;
        public UserConnectionTypeDto State { get; set; } = default!;
    }
}