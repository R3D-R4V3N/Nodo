using System.IO;

namespace Rise.Services.Images;

public interface IImageModerationService
{
    Task<ImageModerationVerdict> ValidateAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
