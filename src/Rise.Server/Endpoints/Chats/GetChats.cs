using Rise.Shared.Chats;
using Rise.Shared.Common;
using System.Security.Claims;

namespace Rise.Server.Endpoints.Chats;

/// <summary>
/// List all chats.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="chatService"></param>
public class GetChats(IChatService chatService) : Endpoint<QueryRequest.SkipTake, Result<ChatResponse.GetChats>>
{
    public override void Configure()
    {
        Get("/api/chats");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ChatResponse.GetChats>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
        => chatService.GetAllAsync(ct);
}