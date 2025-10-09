using System.Security.Claims;
using Ardalis.Result;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class Show(IChatService chatService) : Endpoint<ChatRequest.Show, Result<ChatDto.Index>>
{
    public override void Configure()
    {
        Get("/api/chats/{ChatId:int}");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ChatDto.Index>> ExecuteAsync(ChatRequest.Show req, CancellationToken ct)
    {
        var chatId = Route<int>("ChatId");
        req.ChatId = chatId;
        return chatService.GetByIdAsync(req.ChatId, ct);
    }
}
