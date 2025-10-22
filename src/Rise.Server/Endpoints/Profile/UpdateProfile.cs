using System.Security.Claims;
using Rise.Shared.Profile;

namespace Rise.Server.Endpoints.Profile;

public class UpdateProfile(IProfileService profileService) : Endpoint<ProfileRequest.UpdateProfile, Result<ProfileResponse.Envelope>>
{
    public override void Configure()
    {
        Put("/api/profile");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<ProfileResponse.Envelope>> ExecuteAsync(ProfileRequest.UpdateProfile req, CancellationToken ct)
    {
        return profileService.UpdateAsync(req, ct);
    }
}
