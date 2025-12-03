using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.JSInterop;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService, CacheService cacheService) : IChatService
{
    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var chats = result.Value;
                CacheObject<ChatResponse.GetChats> cacheObject = new CacheObject<ChatResponse.GetChats>()
                { 
                    Key = CacheKeys.ChatsCacheKey,
                    Payload = chats
                };
                await cacheService.CacheAsync(cacheObject, cancellationToken);
            }

            return result ?? Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await cacheService
                .TryGetCachedAsync<ChatResponse.GetChats>(CacheKeys.ChatsCacheKey, cancellationToken);
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
            var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var chat = result.Value;
                CacheObject<ChatResponse.GetChat> cacheObject = new CacheObject<ChatResponse.GetChat>()
                {
                    Key = CacheKeys.GetChatCacheKey(chat.Chat.ChatId),
                    Payload = chat
                };
                await cacheService.CacheAsync(cacheObject, cancellationToken);
            }

            return result ?? Result<ChatResponse.GetChat>.Error("Kon het gesprek niet laden.");
        }
        catch (HttpRequestException)
        {
            var cachedDetail = await cacheService
                .TryGetCachedAsync<ChatResponse.GetChat>(CacheKeys.GetChatCacheKey(chatId), cancellationToken);
            
            if (cachedDetail is not null)
            {
                return Result<ChatResponse.GetChat>.Success(cachedDetail, "Offline: gesprek geladen vanuit cache.");
            }

            var cached = await cacheService
                .TryGetCachedAsync<ChatResponse.GetChats>(CacheKeys.ChatsCacheKey, cancellationToken);
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

    public async Task<Result<ChatResponse.GetSupervisorChat>> GetSupervisorChatAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.GetSupervisorChat>>($"api/chats/supervisor", cancellationToken);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var supervisorChat = result.Value;
                CacheObject<ChatResponse.GetSupervisorChat> cacheObject = new CacheObject<ChatResponse.GetSupervisorChat>()
                {
                    Key = CacheKeys.SupervisorChatCacheKey,
                    Payload = supervisorChat
                };
                await cacheService.CacheAsync(cacheObject, cancellationToken);
            }

            return result ?? Result<ChatResponse.GetSupervisorChat>.Error("Kon het gesprek niet laden.");
        }
        catch (HttpRequestException)
        {
            var cachedDetail = await cacheService
                .TryGetCachedAsync<ChatResponse.GetSupervisorChat>(CacheKeys.SupervisorChatCacheKey, cancellationToken);
            
            if (cachedDetail is not null)
            {
                return Result<ChatResponse.GetSupervisorChat>.Success(cachedDetail, "Offline: gesprek geladen vanuit cache.");
            }

            return Result<ChatResponse.GetSupervisorChat>.Error("Offline: het gesprek kan niet vernieuwd worden zonder verbinding.");
        }
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
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
            var queuedId = await offlineQueueService.QueueOperationAsync(
                httpClient.BaseAddress?.ToString() ?? string.Empty,
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
