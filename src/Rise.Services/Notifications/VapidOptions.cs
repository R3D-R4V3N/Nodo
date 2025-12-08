namespace Rise.Services.Notifications;

public class VapidOptions
{
    public string Subject { get; set; } = default!;
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!;
}