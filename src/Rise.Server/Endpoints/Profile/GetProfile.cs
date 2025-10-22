using System.Security.Claims;
using Rise.Shared.Profile;

namespace Rise.Server.Endpoints.Profile;

public class GetProfile(IProfileService profileService) : EndpointWithoutRequest<Result<ProfileResponse.Envelope>>
{
    public override void Configure()
    {
        Get("/api/profile");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ProfileResponse.Envelope>> ExecuteAsync(CancellationToken ct)
    {
        return profileService.GetAsync(ct);
    }
}
