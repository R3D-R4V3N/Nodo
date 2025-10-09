using Rise.Shared.Chats;
using Rise.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;

namespace Rise.Services.Chats;

public class ChatService(ApplicationDbContext dbContext): IChatService
{
    public async Task<ChatResponse.Index?> GetAllAsync()
    {
        var chatsFromDb = await dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .ToListAsync();

        // Map naar DTO
        var chatDtos = chatsFromDb.Select(c => new ChatDto.Index
        {
            chatId = c.Id,
            messages = c.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Inhoud,
                    Timestamp = m.CreatedAt,
                    SenderId = m.SenderId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}"
                })
                .ToList()
        }).ToList();

        return new ChatResponse.Index
        {
            Chats = chatDtos
        };
    }
}