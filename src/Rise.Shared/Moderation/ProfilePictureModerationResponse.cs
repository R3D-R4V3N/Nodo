namespace Rise.Shared.Moderation;

public record ProfilePictureModerationResponse(bool IsApproved, string? Message)
{
    public static ProfilePictureModerationResponse Approved(string? message = null) => new(true, message);

    public static ProfilePictureModerationResponse Rejected(string? message) => new(false, message);
}
