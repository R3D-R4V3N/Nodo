namespace Rise.Server.Notifications;

public class MagicBellOptions
{
    public const string SectionName = "MagicBell";
    public const string HttpClientName = "MagicBell";

    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? VapidPublicKey { get; set; }
    public string? ActionUrlTemplate { get; set; }
}
