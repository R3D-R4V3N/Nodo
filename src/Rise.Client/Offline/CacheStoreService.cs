using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using Rise.Shared.Chats;
using Rise.Shared.Organizations;
using Rise.Shared.Users;

namespace Rise.Client.Offline;

public sealed class CacheStoreService : IAsyncDisposable
{
    private const string ChatsStore = "chats";
    private const string ContactsStore = "contacts";
    private const string CurrentUserStore = "contact-profiles";
    private const string OrganizationsStore = "organizations";
    private const string AuthSessionStore = "auth-sessions";
    private const string CurrentSessionId = "current-session";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public CacheStoreService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task UpsertChatsAsync(IEnumerable<ChatDto.GetChats> chats, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = chats
            .Select(chat => CreateEnvelope(chat.ChatId.ToString(), chat));

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, ChatsStore, payload);
    }

    public async Task<IReadOnlyList<ChatDto.GetChats>> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<ChatDto.GetChats>[]>("getAll", cancellationToken, ChatsStore);

        return cached?.Select(entry => entry.Value).ToList() ?? [];
    }

    public async Task<ChatDto.GetChats?> GetChatAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var chats = await GetChatsAsync(cancellationToken);
        return chats.FirstOrDefault(chat => chat.ChatId == chatId);
    }

    public async Task UpsertMessagesAsync(int chatId, IEnumerable<MessageDto.Chat> messages, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = messages
            .Select(message => CreateEnvelope(message.Id.ToString(), message));

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, BuildMessagesStoreName(chatId), payload);
    }

    public async Task<IReadOnlyList<MessageDto.Chat>> GetMessagesAsync(int chatId, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<MessageDto.Chat>[]>("getAll", cancellationToken, BuildMessagesStoreName(chatId));

        return cached?.Select(entry => entry.Value).ToList() ?? [];
    }

    public async Task UpsertContactsAsync(IEnumerable<UserDto.Chat> contacts, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = contacts
            .Select(contact => CreateEnvelope(contact.AccountId, contact));

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, ContactsStore, payload);
    }

    public async Task<UserDto.Chat?> GetContactAsync(string? accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return null;
        }

        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<UserDto.Chat>[]>("getAll", cancellationToken, ContactsStore);

        return cached?.FirstOrDefault(contact => contact.Id == accountId)?.Value;
    }

    public async Task UpsertCurrentUserAsync(string accountId, UserDto.CurrentUser user, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = CreateEnvelope(accountId, user);

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, CurrentUserStore, new[] { payload });
    }

    public async Task<UserDto.CurrentUser?> GetCurrentUserAsync(string? accountId, TimeSpan? maxAge = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return null;
        }

        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<UserDto.CurrentUser>[]>("getAll", cancellationToken, CurrentUserStore);

        var entry = cached?.FirstOrDefault(contact => contact.Id == accountId);

        if (entry is null)
        {
            return null;
        }

        if (maxAge is TimeSpan maxAgeValue && entry.UpdatedAt < DateTimeOffset.UtcNow.Subtract(maxAgeValue))
        {
            return null;
        }

        return entry.Value;
    }

    public async Task UpsertOrganizationsAsync(IEnumerable<OrganizationDto.Summary> organizations, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = organizations
            .Select(org => CreateEnvelope(org.Id.ToString(), org));

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, OrganizationsStore, payload);
    }

    public async Task<IReadOnlyList<OrganizationDto.Summary>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<OrganizationDto.Summary>[]>("getAll", cancellationToken, OrganizationsStore);

        return cached?.Select(entry => entry.Value).ToList() ?? [];
    }

    public async Task UpsertAuthSessionAsync(CachedSession session, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var payload = CreateEnvelope(CurrentSessionId, session);

        await _module!.InvokeVoidAsync("upsertMany", cancellationToken, AuthSessionStore, new[] { payload });
    }

    public async Task<CachedSession?> GetAuthSessionAsync(TimeSpan? maxAge = null, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var cached = await _module!
            .InvokeAsync<CacheEnvelope<CachedSession>[]>("getAll", cancellationToken, AuthSessionStore);

        var session = cached?.FirstOrDefault(entry => entry.Id == CurrentSessionId);

        if (session is null)
        {
            return null;
        }

        if (maxAge is TimeSpan maxAgeValue && session.UpdatedAt < DateTimeOffset.UtcNow.Subtract(maxAgeValue))
        {
            return null;
        }

        return session.Value;
    }

    private static string BuildMessagesStoreName(int chatId) => $"messages-{chatId}";

    private static CacheEnvelope<T> CreateEnvelope<T>(string id, T value) => new(
        id,
        DateTimeOffset.UtcNow,
        value);

    private async Task EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/cacheStore.js");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }

    private sealed record CacheEnvelope<T>(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("updatedAt")] DateTimeOffset UpdatedAt,
        [property: JsonPropertyName("value")] T Value);
}
