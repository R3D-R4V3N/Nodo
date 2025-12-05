namespace Rise.Services.Notifications;

public class MagicBellOptions
{
    public string ApiUrl { get; init; } = "https://api.magicbell.com/";
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public string? VapidPublicKey { get; init; }
}
