namespace Rise.Shared.ProfilePictures;

public class ProfilePictureValidationRequest
{
    public required string ImageBase64 { get; set; }

    public string? ContentType { get; set; }
}
