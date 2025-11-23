using System.Net.Http.Json;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService, OfflineChatCacheService offlineChatCacheService) : IChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private readonly OfflineChatCacheService _offlineChatCacheService = offlineChatCacheService;

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        Result<ChatResponse.GetChats>? result = null;

        try
        {
            result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);

            if (result?.IsSuccess is true && result.Value?.Chats is not null)
            {
                await _offlineChatCacheService.SaveChatsAsync(result.Value.Chats, cancellationToken);
                return result;
            }
        }
        catch (HttpRequestException)
        {
            // Intentionally ignored: we fall back to offline cache.
        }

        var cached = await _offlineChatCacheService.GetChatsAsync(cancellationToken);
        if (cached.Count > 0)
        {
            return Result.Success(new ChatResponse.GetChats
            {
                Chats = cached.ToList()
            });
        }

        return result ?? Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        Result<ChatResponse.GetChat>? result = null;

        try
        {
            result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);

            if (result?.IsSuccess is true && result.Value?.Chat is not null)
            {
                await _offlineChatCacheService.SaveChatAsync(result.Value.Chat, cancellationToken);
                return result;
            }
        }
        catch (HttpRequestException)
        {
            // Intentionally ignored: we fall back to offline cache.
        }

        var cached = await _offlineChatCacheService.GetChatAsync(chatId, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(new ChatResponse.GetChat
            {
                Chat = cached
            });
        }

        return result ?? Result<ChatResponse.GetChat>.Error("Kon het gesprek niet laden.");
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
}
