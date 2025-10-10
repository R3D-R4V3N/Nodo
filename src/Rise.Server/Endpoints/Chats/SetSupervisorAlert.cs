using System.Security.Claims;
using System.Threading;
using Ardalis.Result;
using Rise.Shared.Chats;

namespace Rise.Server.Endpoints.Chats;

public class SetSupervisorAlert(IChatService chatService) : Endpoint<ChatRequest.SetSupervisorAlert, Result<SupervisorAlertNotificationDto>>
{
    public override void Configure()
    {
        Post("/api/chats/{ChatId:int}/supervisor-alert");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<SupervisorAlertNotificationDto>> ExecuteAsync(ChatRequest.SetSupervisorAlert req, CancellationToken ct)
    {
        req.ChatId = Route<int>("ChatId");
        return chatService.SetSupervisorAlertAsync(req, ct);
    }
}
