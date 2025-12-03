using System;
using System.Linq;
using Ardalis.Result;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public sealed class ChatMessageDispatchResult
{
    public MessageDto.Chat? PendingMessage { get; init; }
    public Result<MessageDto.Chat>? ServerResult { get; init; }
    public string? Error { get; init; }

    public bool IsSuccess => PendingMessage is not null || IndicatesQueued(ServerResult) || ServerResult?.IsSuccess == true;

    private static bool IndicatesQueued(Result<MessageDto.Chat>? result)
    {
        if (result is null)
        {
            return false;
        }

        return result.Errors.Any(error =>
            error.Contains("opgeslagen", StringComparison.OrdinalIgnoreCase)
            && error.Contains("verbinding", StringComparison.OrdinalIgnoreCase));
    }
}
