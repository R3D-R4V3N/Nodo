<<<<<<< HEAD
using Ardalis.Result;
using Rise.Shared.Chats;
using Rise.Shared.Common;
using System.Security.Claims;
=======
using Rise.Shared.Common;
using Rise.Shared.Chats;
>>>>>>> origin/main

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
<<<<<<< HEAD
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ChatResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) =>
        chatService.GetAllAsync(ct);
=======
        AllowAnonymous();
    }
   
    public override async Task<Result<ChatResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var result = await chatService.GetAllAsync(); // geen req en ct meer

        if (result == null)
            return Result.Error("Geen chats gevonden");

        return Result.Success(result);
    }
>>>>>>> origin/main
}