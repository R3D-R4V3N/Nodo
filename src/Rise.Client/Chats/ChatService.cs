using System.Net.Http.Json;
using Rise.Client.Offline;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient, OfflineQueueService offlineQueueService) : IChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
        return result ?? Result.Error("Kon de chats niet laden.");
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
        return result ?? Result.Error("Kon het gesprek niet laden.");
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueMessageAsync(request, cancellationToken);
            return Result<MessageDto.Chat>.Error("Geen netwerkverbinding: het bericht is opgeslagen en wordt verzonden zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            await QueueMessageAsync(request, cancellationToken);
            return Result<MessageDto.Chat>.Error("Kon geen verbinding maken: het bericht is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<MessageDto.Chat>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }

    private Task QueueMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken)
    {
        return _offlineQueueService.QueueOperationAsync(
            _httpClient.BaseAddress?.ToString() ?? string.Empty,
            $"/api/chats/{request.ChatId}/messages",
            HttpMethod.Post,
            request,
            cancellationToken: cancellationToken);
    }
}
