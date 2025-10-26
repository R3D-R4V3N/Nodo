using System;
using System.Collections.Generic;

namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateCurrentUser
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string Gender { get; set; } = "x";
        public string AvatarUrl { get; set; } = string.Empty;
        public IReadOnlyCollection<string> HobbyIds { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> LikePreferenceIds { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> DislikePreferenceIds { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> DefaultChatLines { get; set; } = Array.Empty<string>();
    }
}
