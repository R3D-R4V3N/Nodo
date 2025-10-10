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

    public async Task<Result<ChatEmergencyStatusDto>> ActivateEmergencyAsync(ChatRequest.ToggleEmergency request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/chats/{request.ChatId}/emergency/activate", null, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result<ChatEmergencyStatusDto>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon de noodmelding niet activeren.");
    }

    public async Task<Result<ChatEmergencyStatusDto>> DeactivateEmergencyAsync(ChatRequest.ToggleEmergency request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/chats/{request.ChatId}/emergency/deactivate", null, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result<ChatEmergencyStatusDto>>(cancellationToken: cancellationToken);

        return result ?? Result.Error("Kon de noodmelding niet intrekken.");
    }
}
