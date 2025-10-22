using System.Collections.Generic;

namespace Rise.Shared.Users;

public static class UserRequest
{
    public class UpdateCurrentUser
    {
        public string Name { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public List<string> Likes { get; set; } = [];
        public List<string> Dislikes { get; set; } = [];
        public List<string> HobbyIds { get; set; } = [];
    }
}
