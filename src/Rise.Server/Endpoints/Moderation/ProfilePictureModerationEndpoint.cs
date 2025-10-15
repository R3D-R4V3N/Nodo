using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Rise.Services.Moderation;
using Rise.Shared.Moderation;

namespace Rise.Server.Endpoints.Moderation;

public class ProfilePictureModerationEndpoint(IImageModerationService moderationService)
    : Endpoint<ProfilePictureModerationRequest, ProfilePictureModerationResponse>
{
    public override void Configure()
    {
        Post("/api/moderation/profile-picture");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task<ProfilePictureModerationResponse> ExecuteAsync(ProfilePictureModerationRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length == 0)
        {
            return ProfilePictureModerationResponse.Rejected("Gelieve een afbeelding te selecteren.");
        }

        await using var readStream = req.File.OpenReadStream();
        var moderationResult = await moderationService.ModerateAsync(readStream, req.File.FileName, ct);

        return moderationResult.IsApproved
            ? ProfilePictureModerationResponse.Approved("Profiel foto goedgekeurd.")
            : ProfilePictureModerationResponse.Rejected(moderationResult.FailureReason ?? "Profiel foto afgekeurd.");
    }
}
