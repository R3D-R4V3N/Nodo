using Rise.Shared.Chats;
using Rise.Shared.Common;

namespace Rise.Services.Chats;

public class ChatService: IChatService
{
    public async Task<ChatResponse.Index?> GetAllAsync()
    {
        // Mockdata
        var chats = new List<ChatDto.Index>
        {
            new ChatDto.Index
            {
                chatId = 1,
                messages = new List<MessageDto>
                {
                    new MessageDto { Id = 1, Content = "Hallo!", Timestamp = DateTime.UtcNow },
                    new MessageDto { Id = 2, Content = "Hoe gaat het?", Timestamp = DateTime.UtcNow }
                }
            },
            new ChatDto.Index
            {
                chatId = 2,
                messages = new List<MessageDto>
                {
                    new MessageDto { Id = 1, Content = "Test bericht", Timestamp = DateTime.UtcNow }
                }
            }
        };

        var response = new ChatResponse.Index
        {
            Chats = chats
        };

        return response;
    }
}