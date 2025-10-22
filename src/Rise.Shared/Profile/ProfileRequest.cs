using System.Collections.Generic;

namespace Rise.Shared.Profile;

public static class ProfileRequest
{
    public record UpdateProfile
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Biography { get; init; } = string.Empty;
        public string Gender { get; init; } = "x";
        public string AvatarUrl { get; init; } = string.Empty;
        public IReadOnlyList<string> Interests { get; init; } = new List<string>();
    }
}
