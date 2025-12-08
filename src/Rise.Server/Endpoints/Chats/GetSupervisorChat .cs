using System.Security.Claims;
using Rise.Shared.Chats;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Chats;

public class GetSupervisorChat(IChatService chatService) : EndpointWithoutRequest<Result<ChatResponse.GetSupervisorChat>>
{
    public override void Configure()
    {
        Get("/api/chats/supervisor");
        Roles(AppRoles.User);
    }

    public override Task<Result<ChatResponse.GetSupervisorChat>> ExecuteAsync(CancellationToken ct)
    {
        return chatService.GetSupervisorChatAsync(ct);
    }
}
