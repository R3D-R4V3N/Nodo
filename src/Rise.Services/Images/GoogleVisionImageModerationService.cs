using System.Collections.Generic;
using System.IO;
using Google.Cloud.Vision.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Rise.Services.Images;

public class GoogleVisionImageModerationService(
    ImageAnnotatorClient imageAnnotatorClient,
    IOptions<VisionModerationOptions> options,
    ILogger<GoogleVisionImageModerationService> logger) : IImageModerationService
{
    private readonly ImageAnnotatorClient _client = imageAnnotatorClient;
    private readonly VisionModerationOptions _options = options.Value;
    private readonly ILogger<GoogleVisionImageModerationService> _logger = logger;

    public async Task<ImageModerationVerdict> ValidateAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length == 0)
        {
            throw new InvalidDataException("De aangeleverde afbeelding bevat geen data.");
        }

        buffer.Position = 0;

        try
        {
            var image = Image.FromStream(buffer);

            var annotation = await _client.DetectSafeSearchAsync(image, cancellationToken: cancellationToken);

            var adult = Convert(annotation.Adult);
            var medical = Convert(annotation.Medical);
            var violence = Convert(annotation.Violence);
            var racy = Convert(annotation.Racy);
            var spoof = Convert(annotation.Spoof);

            var blockedCategories = new List<string>();
            var threshold = _options.BlockWhenLikelihoodAtLeast;

            Evaluate("Volwassen", adult, threshold, blockedCategories);
            Evaluate("Medisch", medical, threshold, blockedCategories);
            Evaluate("Geweld", violence, threshold, blockedCategories);
            Evaluate("Sensueel", racy, threshold, blockedCategories);
            Evaluate("Nep", spoof, threshold, blockedCategories);

            var verdict = blockedCategories.Count == 0;

            return new ImageModerationVerdict(verdict, adult, medical, violence, racy, spoof, blockedCategories);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogError(rpcEx, "Google Vision SafeSearch analyse mislukte.");
            throw new InvalidOperationException("Kon de afbeelding niet analyseren met Google Vision.", rpcEx);
        }
    }

    private static ContentLikelihood Convert(Likelihood likelihood) => likelihood switch
    {
        Likelihood.VeryUnlikely => ContentLikelihood.VeryUnlikely,
        Likelihood.Unlikely => ContentLikelihood.Unlikely,
        Likelihood.Possible => ContentLikelihood.Possible,
        Likelihood.Likely => ContentLikelihood.Likely,
        Likelihood.VeryLikely => ContentLikelihood.VeryLikely,
        _ => ContentLikelihood.Unknown
    };

    private static void Evaluate(string category, ContentLikelihood likelihood, ContentLikelihood threshold, ICollection<string> blockedCategories)
    {
        if (likelihood >= threshold)
        {
            blockedCategories.Add(category);
        }
    }
}
