namespace Rise.Services.Images;

/// <summary>
/// Represents the likelihood that a specific type of content is present in an image.
/// Matches Google's SafeSearch likelihood scale so it can be mapped easily.
/// </summary>
public enum ContentLikelihood
{
    Unknown = 0,
    VeryUnlikely = 1,
    Unlikely = 2,
    Possible = 3,
    Likely = 4,
    VeryLikely = 5
}
