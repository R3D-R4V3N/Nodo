using System.Net.Http.Json;
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