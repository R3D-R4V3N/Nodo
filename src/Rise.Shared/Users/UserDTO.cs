namespace Rise.Shared.Users;
public static class UserDto
{
    public record CurrentUser
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        
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
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        
        public List<string> DefaultChatLines { get; init; } = [];
    }
}
