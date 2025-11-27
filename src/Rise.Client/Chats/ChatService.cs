using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.JSInterop;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService, IJSRuntime jsRuntime) : IChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private const string ChatsCacheKey = "offline-cache:chats";
    private const string ChatDetailCacheKeyPrefix = "offline-cache:chat:";

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                await CacheChatsAsync(result.Value, cancellationToken);
            }

            return result ?? Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await TryGetCachedChatsAsync(cancellationToken);
            if (cached is not null)
            {
                return Result<ChatResponse.GetChats>.Success(cached, "Offline: eerder geladen gesprekken worden getoond.");
            }

            return Result<ChatResponse.GetChats>.Error("Offline: gesprekken zijn niet beschikbaar zonder eerder geladen data.");
        }
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                await CacheChatAsync(result.Value, cancellationToken);
            }

            return result ?? Result<ChatResponse.GetChat>.Error("Kon het gesprek niet laden.");
        }
        catch (HttpRequestException)
        {
            var cachedDetail = await TryGetCachedChatAsync(chatId, cancellationToken);
            if (cachedDetail is not null)
            {
                return Result<ChatResponse.GetChat>.Success(cachedDetail, "Offline: gesprek geladen vanuit cache.");
            }

            var cached = await TryGetCachedChatsAsync(cancellationToken);
            var chat = cached?.Chats.FirstOrDefault(c => c.ChatId == chatId);

            if (chat is not null)
            {
                return Result<ChatResponse.GetChat>.Success(new ChatResponse.GetChat
                {
                    Chat = new ChatDto.GetChat
                    {
                        ChatId = chat.ChatId,
                        Users = chat.Users,
                        Messages = []
                    }
                }, "Offline: beperkte gegevens worden uit cache getoond.");
            }

            return Result<ChatResponse.GetChat>.Error("Offline: het gesprek kan niet vernieuwd worden zonder verbinding.");
        }
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            var queued = await TryQueueMessageAsync(request, cancellationToken);
            var message = queued
                ? "Kon geen verbinding maken: het bericht is opgeslagen en wordt verzonden zodra de verbinding terug is."
                : "Kon geen verbinding maken: het bericht kon niet worden opgeslagen om later te verzenden.";

            return Result<MessageDto.Chat>.Error(message);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<MessageDto.Chat>>(cancellationToken: cancellationToken);

        return result ?? Result<MessageDto.Chat>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<int>> QueueMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        try
        {
            var queuedId = await _offlineQueueService.QueueOperationAsync(
                _httpClient.BaseAddress?.ToString() ?? string.Empty,
                $"/api/chats/{request.ChatId}/messages",
                HttpMethod.Post,
                request,
                cancellationToken: cancellationToken);

            return Result.Success(queuedId);
        }
        catch
        {
            return Result<int>.Error("Het bericht kon niet offline opgeslagen worden.");
        }
    }

    private async Task<bool> TryQueueMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken)
    {
        var queueResult = await QueueMessageAsync(request, cancellationToken);
        return queueResult.IsSuccess;
    }

    private async Task CacheChatsAsync(ChatResponse.GetChats chats, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(chats, _serializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, ChatsCacheKey, payload);
        }
        catch
        {
            // Failing to cache should not break the UX.
        }
    }

    private async Task CacheChatAsync(ChatResponse.GetChat chat, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(chat, _serializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, GetChatCacheKey(chat.Chat.ChatId), payload);
        }
        catch
        {
            // Failing to cache should not break the UX.
        }
    }

    private async Task<ChatResponse.GetChats?> TryGetCachedChatsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, ChatsCacheKey);
            if (string.IsNullOrWhiteSpace(cached))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ChatResponse.GetChats>(cached, _serializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ChatResponse.GetChat?> TryGetCachedChatAsync(int chatId, CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, GetChatCacheKey(chatId));
            if (string.IsNullOrWhiteSpace(cached))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ChatResponse.GetChat>(cached, _serializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string GetChatCacheKey(int chatId) => $"{ChatDetailCacheKeyPrefix}{chatId}";
}
