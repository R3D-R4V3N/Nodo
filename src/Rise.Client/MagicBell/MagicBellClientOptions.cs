namespace Rise.Client.MagicBell;

public class MagicBellClientOptions
{
    public string PublicKey { get; init; } = string.Empty;
    public string ServiceWorkerPath { get; init; } = "/service-worker.js";
}
