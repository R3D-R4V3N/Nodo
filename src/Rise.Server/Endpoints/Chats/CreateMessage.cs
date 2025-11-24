using System.Security.Claims;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class CreateMessage(IChatService chatService) : Endpoint<ChatRequest.CreateMessage, Result<MessageDto.Chat>>
{
    public override void Configure()
    {
        Post("/api/chats/{ChatId:int}/messages");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<MessageDto.Chat>> ExecuteAsync(ChatRequest.CreateMessage req, CancellationToken ct)
    {
        req.ChatId = Route<int>("ChatId");
        return chatService.CreateMessageAsync(req, ct);
    }
}
