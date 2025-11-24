using System.Net.Http.Json;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService, SessionCacheService sessionCacheService) : IChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private readonly SessionCacheService _sessionCacheService = sessionCacheService;
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(30);

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ChatResponse.GetChats? cached = null;
        try
        {
            var cachedChats = await _sessionCacheService.GetCachedChatsAsync(cancellationToken);
            cached = new ChatResponse.GetChats
            {
                Chats = cachedChats
            };
        }
        catch
        {
            // Ignore cache issues and fall back to network.
        }

        var isOnline = await _offlineQueueService.IsOnlineAsync();
        if (!isOnline)
        {
            return cached is not null && cached.Chats.Any()
                ? Result.Success(cached)
                : Result<ChatResponse.GetChats>.Error("Kon de chats niet laden: offline.");
        }

        var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
        if (result?.IsSuccess == true && result.Value is not null)
        {
            await _sessionCacheService.CacheChatsAsync(result.Value.Chats ?? [], CacheLifetime, cancellationToken);
            return result;
        }

        if (cached is not null && cached.Chats.Any())
        {
            return Result.Success(cached);
        }

        return result ?? Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
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
