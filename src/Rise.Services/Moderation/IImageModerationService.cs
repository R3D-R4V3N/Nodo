using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Services.Moderation;

public record ImageModerationResult(bool IsApproved, string? FailureReason);

public interface IImageModerationService
{
    Task<ImageModerationResult> ModerateAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
}
