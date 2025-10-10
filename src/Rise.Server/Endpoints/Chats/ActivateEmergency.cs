using System.Security.Claims;
using Ardalis.Result;
using Rise.Services.Chats;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class ActivateEmergency(IChatService chatService) : EndpointWithoutRequest<Result<ChatEmergencyStatusDto>>
{
    public override void Configure()
    {
        Post("/api/chats/{ChatId:int}/emergency/activate");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ChatEmergencyStatusDto>> ExecuteAsync(CancellationToken ct)
    {
        var request = new ChatRequest.ToggleEmergency
        {
            ChatId = Route<int>("ChatId")
        };

        return chatService.ActivateEmergencyAsync(request, ct);
    }
}
