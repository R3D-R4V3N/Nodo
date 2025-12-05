namespace Rise.Services.Notifications;

/// <summary>
/// Configuration used to sign outgoing Web Push messages.
/// </summary>
public class VapidSettings
{
    public string Subject { get; set; } = "mailto:admin@example.com";
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
