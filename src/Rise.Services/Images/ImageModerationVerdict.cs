using System.Collections.Generic;

namespace Rise.Services.Images;

/// <summary>
/// Represents the moderation verdict for a single image.
/// </summary>
public sealed record ImageModerationVerdict(
    bool IsAllowed,
    ContentLikelihood Adult,
    ContentLikelihood Medical,
    ContentLikelihood Violence,
    ContentLikelihood Racy,
    ContentLikelihood Spoof,
    IReadOnlyCollection<string> BlockedCategories);
