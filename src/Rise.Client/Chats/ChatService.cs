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
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
            return result ?? Result<ChatResponse.GetChats>.Error("Kon de chats niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await _offlineQueueService.GetCachedResponseAsync(_httpClient.BaseAddress?.ToString() ?? string.Empty,
                "/api/chats", HttpMethod.Get, cancellationToken);

            if (cached is not null)
            {
                var cachedResult = await cached.Content.ReadFromJsonAsync<Result<ChatResponse.GetChats>>(cancellationToken: cancellationToken);
                if (cachedResult is not null)
                {
                    return cachedResult;
                }
            }

            return Result<ChatResponse.GetChats>.Error("De chats kunnen offline niet geladen worden omdat er geen cache beschikbaar is.");
        }
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
            return result ?? Result<ChatResponse.GetChat>.Error("Kon het gesprek niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await _offlineQueueService.GetCachedResponseAsync(_httpClient.BaseAddress?.ToString() ?? string.Empty,
                $"/api/chats/{chatId}", HttpMethod.Get, cancellationToken);

            if (cached is not null)
            {
                var cachedResult = await cached.Content.ReadFromJsonAsync<Result<ChatResponse.GetChat>>(cancellationToken: cancellationToken);
                if (cachedResult is not null)
                {
                    return cachedResult;
                }
            }

            return Result<ChatResponse.GetChat>.Error("Het gesprek is offline niet beschikbaar omdat er geen opgeslagen gegevens zijn.");
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
            return Result<MessageDto.Chat>.Error(
                "Kon geen verbinding maken: probeer het later opnieuw of werk verder in offline modus.");
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
}
