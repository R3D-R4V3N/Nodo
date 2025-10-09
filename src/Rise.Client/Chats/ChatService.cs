using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatService(HttpClient httpClient) : IChatService
{
    public async Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.Index>>("api/chats", cancellationToken);
        return result ?? Result.Error("Kon de chats niet laden.");
    }

    public async Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result<MessageDto>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }
}
