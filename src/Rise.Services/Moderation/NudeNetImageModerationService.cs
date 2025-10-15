using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NudityDetector;

namespace Rise.Services.Moderation;

/// <summary>
/// Wraps the NudeNet classifier to evaluate whether an uploaded profile picture is acceptable.
/// </summary>
public class NudeNetImageModerationService : IImageModerationService, IDisposable
{
    private readonly ILogger<NudeNetImageModerationService> logger;
    private readonly Classifier classifier;

    public NudeNetImageModerationService(ILogger<NudeNetImageModerationService> logger)
    {
        this.logger = logger;
        classifier = new Classifier();
    }

    public async Task<ImageModerationResult> ModerateAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        var tempFilePath = CreateTempFilePath(fileName);

        try
        {
            await using (var fileStream = File.Create(tempFilePath))
            {
                imageStream.Seek(0, SeekOrigin.Begin);
                await imageStream.CopyToAsync(fileStream, cancellationToken);
            }

            var probabilities = classifier.Classify(tempFilePath);
            if (!probabilities.TryGetValue("unsafe", out var unsafeScore))
            {
                logger.LogWarning("NudeNet classifier did not return an unsafe score for file {FileName}", fileName);
                return new ImageModerationResult(true, null);
            }

            var isApproved = unsafeScore < 0.5f;
            var failureReason = isApproved
                ? null
                : $"Afgekeurd door NudeNet (ongepaste score: {unsafeScore:P0}).";

            return new ImageModerationResult(isApproved, failureReason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kon de profielfoto {FileName} niet modereren", fileName);
            return new ImageModerationResult(false, "Er trad een technische fout op tijdens het beoordelen van de foto.");
        }
        finally
        {
            DeleteTempFile(tempFilePath);
        }
    }

    private static string CreateTempFilePath(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var tempFileName = $"nudenet_{Guid.NewGuid():N}{extension}";
        return Path.Combine(Path.GetTempPath(), tempFileName);
    }

    private static void DeleteTempFile(string tempFilePath)
    {
        try
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
        catch
        {
            // Swallow cleanup exceptions.
        }
    }

    public void Dispose()
    {
        if (classifier is IDisposable disposable)
        {
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
