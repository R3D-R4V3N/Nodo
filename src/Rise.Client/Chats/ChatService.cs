using System.Net.Http.Json;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService, CacheStoreService cacheStoreService) : IChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private readonly CacheStoreService _cacheStoreService = cacheStoreService;

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);

            if (result is { IsSuccess: true, Value: { } chatsResponse })
            {
                await _cacheStoreService.UpsertChatsAsync(chatsResponse.Chats, cancellationToken);
            }

            return result ?? await GetCachedChatsAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return await GetCachedChatsAsync(cancellationToken);
        }
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);

            if (result is { IsSuccess: true, Value: { } chatResponse })
            {
                await _cacheStoreService.UpsertMessagesAsync(chatId, chatResponse.Chat.Messages, cancellationToken);
                await _cacheStoreService.UpsertChatsAsync([new ChatDto.GetChats
                {
                    ChatId = chatResponse.Chat.ChatId,
                    Users = chatResponse.Chat.Users,
                    LastMessage = chatResponse.Chat.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault()
                }], cancellationToken);

                await _cacheStoreService.UpsertContactsAsync(chatResponse.Chat.Users, cancellationToken);
            }

            return result ?? await GetCachedChatAsync(chatId, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return await GetCachedChatAsync(chatId, cancellationToken);
        }
    }

    private async Task<Result<ChatResponse.GetChats>> GetCachedChatsAsync(CancellationToken cancellationToken)
    {
        var cachedChats = await _cacheStoreService.GetChatsAsync(cancellationToken);
        if (cachedChats.Any())
        {
            return Result.Success(new ChatResponse.GetChats { Chats = cachedChats.ToList() }).MarkCached();
        }

        return Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
    }

    private async Task<Result<ChatResponse.GetChat>> GetCachedChatAsync(int chatId, CancellationToken cancellationToken)
    {
        var chat = await _cacheStoreService.GetChatAsync(chatId, cancellationToken);
        var messages = await _cacheStoreService.GetMessagesAsync(chatId, cancellationToken);

        if (chat is not null)
        {
            return Result.Success(new ChatResponse.GetChat
            {
                Chat = new ChatDto.GetChat
                {
                    ChatId = chat.ChatId,
                    Users = chat.Users,
                    Messages = messages.OrderBy(m => m.Timestamp).ToList(),
                }
            }).MarkCached();
        }

        return Result<ChatResponse.GetChat>.Error("Kon het gesprek niet laden.");
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        request.ClientMessageId ??= Guid.NewGuid();

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
        request.ClientMessageId ??= Guid.NewGuid();

        AttachmentMetadata? attachment = null;
        if (request.Attachment is not null)
        {
            attachment = new AttachmentMetadata
            {
                BlobKey = request.Attachment.BlobKey,
                ContentType = request.Attachment.ContentType,
                FileName = request.Attachment.FileName
            };
        }

        try
        {
            var queuedId = await _offlineQueueService.QueueOperationAsync(
                _httpClient.BaseAddress?.ToString() ?? string.Empty,
                $"/api/chats/{request.ChatId}/messages",
                HttpMethod.Post,
                request,
                clientMessageId: request.ClientMessageId,
                chatId: request.ChatId,
                attachment: attachment,
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
