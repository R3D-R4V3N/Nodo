using System.Net.Http.Json;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient) : IChatService
{
    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.GetChats>>("api/chats", cancellationToken);
        return result ?? Result.Error("Kon de chats niet laden.");
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.GetChat>>($"api/chats/{chatId}", cancellationToken);
        return result ?? Result.Error("Kon het gesprek niet laden.");
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result<MessageDto.Chat>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }
}
