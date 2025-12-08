namespace Rise.Client.Offline;

public static class CacheKeys
{
    public const string ChatsCacheKey = "offline-cache:chats";
    public const string SupervisorChatCacheKey = "offline-cache:chat:supervisor";
    public const string FriendsCacheKey = "offline-cache:connections:friends";
    public const string FriendRequestsCacheKey = "offline-cache:connections:requests";
    public const string FriendSuggestionsCacheKey = "offline-cache:connections:suggestions";
    public static string GetChatCacheKey(int chatId) => $"{ChatDetailCacheKeyPrefix}{chatId}";
    public static string GetChatMessageCacheKey(int chatId, int page) => $"{ChatDetailCacheKeyPrefix}{chatId}:{page}";
    private const string ChatDetailCacheKeyPrefix = "offline-cache:chat:";
}
