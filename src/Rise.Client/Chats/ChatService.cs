using System.Net.Http.Json;
<<<<<<< HEAD
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

    public async Task<Result<ChatDto.Index>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<ChatDto.Index>>($"api/chats/{chatId}", cancellationToken);
        return result ?? Result.Error("Kon het gesprek niet laden.");
    }

    public async Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/chats/{request.ChatId}/messages", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result<MessageDto>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }
}
=======
using System.Text.Json;
using Rise.Shared.Chats;
using Rise.Shared.Common;

namespace Rise.Client.Chats;


public class ChatService(HttpClient httpClient) : IChatService
{
    public async Task<ChatResponse.Index?> GetAllAsync()
    {
        // Deserialiseer eerst naar Result<ChatResponse.Index>
        var result = await httpClient.GetFromJsonAsync<Result<ChatResponse.Index>>("api/chats");

        // Return de Value (de echte chats)
        return result?.Value;
    }
}
>>>>>>> origin/main
