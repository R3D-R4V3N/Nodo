namespace Rise.Client.Offline;

public static class CacheKeys
{
    public const string ChatsCacheKey = "offline-cache:chats";
    public const string SupervisorChatCacheKey = "offline-cache:chat:supervisor";
    private const string ChatDetailCacheKeyPrefix = "offline-cache:chat:";
    public static string GetChatCacheKey(int chatId) => $"{ChatDetailCacheKeyPrefix}{chatId}";
}
