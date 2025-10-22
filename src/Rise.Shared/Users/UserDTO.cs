namespace Rise.Shared.Users;
public static class UserDto
{
    public record CurrentUser
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Biography { get; init; } = string.Empty;
        public DateOnly BirthDay { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<Interest> Interests { get; init; } = [];
        public List<Hobby> Hobbies { get; init; } = [];
        public List<string> DefaultChatLines { get; init; } = [];
    }
    public record Interest
    {
        public string Type { get; init; } = string.Empty;
        public string? Like { get; init; }
        public string? Dislike { get; init; }
    }
    public record Hobby
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Emoji { get; init; } = string.Empty;
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
}
