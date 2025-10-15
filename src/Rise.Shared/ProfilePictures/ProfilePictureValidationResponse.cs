namespace Rise.Shared.ProfilePictures;

public sealed record ProfilePictureValidationResponse(
    bool IsAllowed,
    SafeSearchVerdict Verdict,
    IReadOnlyCollection<string> BlockedCategories);

public sealed record SafeSearchVerdict(
    string Adult,
    string Medical,
    string Violence,
    string Racy,
    string Spoof);
