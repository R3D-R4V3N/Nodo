using Rise.Shared.Common;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

/// <summary>
/// List all chats.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="chatService"></param>
public class Index(IChatService chatService) : Endpoint<QueryRequest.SkipTake, Result<ChatResponse.Index>>
{
    public override void Configure()
    {
        Get("/api/chats");
        AllowAnonymous();
    }
   
    public override async Task<Result<ChatResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var result = await chatService.GetAllAsync(); // geen req en ct meer

        if (result == null)
            return Result.Error("Geen chats gevonden");

        return Result.Success(result);
    }
}