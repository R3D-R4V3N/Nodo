using System.Security.Claims;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class GetChat(IChatService chatService) : Endpoint<ChatRequest.GetChat, Result<ChatDto.GetChats>>
{
    public override void Configure()
    {
        Get("/api/chats/{ChatId:int}");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ChatDto.GetChats>> ExecuteAsync(ChatRequest.GetChat req, CancellationToken ct)
    {
        req.ChatId = Route<int>("ChatId");
        return chatService.GetByIdAsync(req.ChatId, ct);
    }
}
