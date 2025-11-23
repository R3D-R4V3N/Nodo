using Microsoft.JSInterop;
using Rise.Shared.Chats;

namespace Rise.Client.Offline;

public sealed class OfflineChatCacheService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public OfflineChatCacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveChatsAsync(IEnumerable<ChatDto.GetChats> chats, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("saveChatList", cancellationToken, chats.ToArray());
    }

    public async Task<IReadOnlyList<ChatDto.GetChats>> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var cached = await _module!.InvokeAsync<ChatDto.GetChats[]>("getChatList", cancellationToken);
        return cached ?? Array.Empty<ChatDto.GetChats>();
    }

    public async Task SaveChatAsync(ChatDto.GetChat chat, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("saveChatDetail", cancellationToken, chat);
    }

    public async Task<ChatDto.GetChat?> GetChatAsync(int chatId, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<ChatDto.GetChat?>("getChatDetail", cancellationToken, chatId);
    }

    private async Task EnsureModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/offlineChatCache.js");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
