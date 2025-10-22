using System;
using System.Collections.Generic;

namespace Rise.Shared.Profile;

public static class ProfileResponse
{
    public record Profile
    {
        public int Id { get; init; }
        public string AccountId { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Biography { get; init; } = string.Empty;
        public string Gender { get; init; } = "x";
        public string AvatarUrl { get; init; } = string.Empty;
        public DateTime MemberSince { get; init; }
            = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public IReadOnlyList<string> Interests { get; init; } = new List<string>();
    }

    public record Envelope
    {
        public Profile Profile { get; init; } = new();
        public IReadOnlyList<ProfileInterestDto> AvailableInterests { get; init; } = Array.Empty<ProfileInterestDto>();
        public int MaxInterestCount { get; init; } = ProfileCatalog.MaxInterestCount;
    }
}
