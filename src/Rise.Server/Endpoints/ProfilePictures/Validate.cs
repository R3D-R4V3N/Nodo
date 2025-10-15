using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Rise.Services.Images;
using Rise.Shared.ProfilePictures;

namespace Rise.Server.Endpoints.ProfilePictures;

public class Validate(IImageModerationService moderationService) : EndpointWithoutRequest<Result<ProfilePictureValidationResponse>>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public override void Configure()
    {
        Post("/api/profile-picture/validate");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task<Result<ProfilePictureValidationResponse>> ExecuteAsync(CancellationToken ct)
    {
        var file = Files.GetFile("image") ?? Files.FirstOrDefault();
        if (file is null)
        {
            return Result.Invalid(new ValidationError("image", "Selecteer een afbeelding om te controleren."));
        }

        if (!IsImage(file))
        {
            return Result.Invalid(new ValidationError("image", "Alleen afbeeldingsbestanden worden ondersteund."));
        }

        if (file.Length > MaxFileSize)
        {
            return Result.Invalid(new ValidationError("image", "De afbeelding mag maximaal 5 MB zijn."));
        }

        if (file.Length == 0)
        {
            return Result.Invalid(new ValidationError("image", "De afbeelding bevat geen data."));
        }

        try
        {
            await using var stream = file.OpenReadStream(MaxFileSize);

            var verdict = await moderationService.ValidateAsync(stream, ct);

            var response = new ProfilePictureValidationResponse(
                verdict.IsAllowed,
                new SafeSearchVerdict(
                    verdict.Adult.ToString(),
                    verdict.Medical.ToString(),
                    verdict.Violence.ToString(),
                    verdict.Racy.ToString(),
                    verdict.Spoof.ToString()),
                verdict.BlockedCategories);

            return Result.Success(response);
        }
        catch (InvalidDataException invalidData)
        {
            Logger.LogWarning(invalidData, "Afbeelding kon niet worden gelezen.");
            return Result.Invalid(new ValidationError("image", invalidData.Message));
        }
        catch (InvalidOperationException processingError)
        {
            Logger.LogError(processingError, "Validatie van profielfoto mislukt.");
            return Result.Error("Er ging iets mis bij het controleren van de afbeelding.");
        }
    }

    private static bool IsImage(IFormFile file) =>
        file.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
}
