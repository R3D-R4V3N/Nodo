using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Chats;
using Rise.Shared.Identity;

namespace Rise.Services.Chats;

public class ChatService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IChatService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var chatsFromDb = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .ToListAsync(cancellationToken);

        var chatDtos = chatsFromDb.Select(c => new ChatDto.Index
        {
            chatId = c.Id,
            messages = c.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MapToDto)
                .ToList()
        }).ToList();

        return Result.Success(new ChatResponse.Index
        {
            Chats = chatDtos
        });
    }

    public async Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (sender is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var chat = await _dbContext.Chats
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        var message = new Message
        {
            ChatId = chat.Id,
            SenderId = sender.Id,
            Inhoud = request.Content!.Trim()
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(message, sender));
    }

    private static MessageDto MapToDto(Message message)
    {
        var sender = message.Sender ?? throw new InvalidOperationException("Message sender must be loaded.");
        return MapToDto(message, sender);
    }

    private static MessageDto MapToDto(Message message, ApplicationUser sender)
    {
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Inhoud,
            Timestamp = message.CreatedAt,
            SenderId = message.SenderId,
            SenderName = $"{sender.FirstName} {sender.LastName}",
            SenderAccountId = sender.AccountId
        };
    }
}
