using System.Collections.Generic;

namespace Rise.Shared.Users;

public static class UserDto
{
    public record CurrentUser
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Name => $"{FirstName} {LastName}";
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Biography { get; init; } = string.Empty;
        public string Gender { get; init; } = "x";
        public DateOnly BirthDay { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<SentimentDto.Get> Interests { get; init; } = [];
        public List<HobbyDto.Get> Hobbies { get; init; } = [];
        public List<string> DefaultChatLines { get; init; } = [];
    }

    public record Connection
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public int Age { get; init; }
        public string AvatarUrl { get; init; } = string.Empty;
    }

    public record Chat
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
    }

    public record Message
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
    }
    public record ConnectionProfile
    {
        
      
    }
}
