using Microsoft.JSInterop;
using Rise.Shared.Chats;

namespace Rise.Client.Offline;

public sealed class OfflineCacheService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public OfflineCacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveChatListAsync(ChatResponse.GetChats chats, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("cacheChatList", cancellationToken, chats);
    }

    public async Task<ChatResponse.GetChats?> GetCachedChatListAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<ChatResponse.GetChats?>("getCachedChatList", cancellationToken);
    }

    public async Task SaveChatAsync(int chatId, ChatResponse.GetChat chat, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("cacheChat", cancellationToken, chatId, chat);
    }

    public async Task<ChatResponse.GetChat?> GetCachedChatAsync(int chatId, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<ChatResponse.GetChat?>("getCachedChat", cancellationToken, chatId);
    }

    private async Task EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/offlineData.js");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
