namespace Rise.Services.Images;

/// <summary>
/// Options used when validating images with Google Vision.
/// </summary>
public class VisionModerationOptions
{
    /// <summary>
    /// SafeSearch likelihood at which an image should be blocked.
    /// </summary>
    public ContentLikelihood BlockWhenLikelihoodAtLeast { get; set; } = ContentLikelihood.Likely;
}
