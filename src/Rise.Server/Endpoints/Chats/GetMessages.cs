using System.Security.Claims;
using Rise.Shared.Common;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class GetMessages(IChatService chatService) : Endpoint<QueryRequest.SkipTake, Result<ChatResponse.GetMessages>>
{
    public override void Configure()
    {
        Get("/api/chats/{ChatId:int}/messages");
        Claims(ClaimTypes.NameIdentifier);
        Summary(s =>
        {
            s.Summary = "Get chat messages";
            s.Description = "Returns a paginated list of messages for the specified chat.";
        });
    }

    public override Task<Result<ChatResponse.GetMessages>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var chatId = Route<int>("ChatId");
        return chatService.GetMessagesAsync(chatId, req, ct);
    }
}
