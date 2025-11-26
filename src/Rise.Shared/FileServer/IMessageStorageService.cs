namespace Rise.Services.FileServer;

public interface IMessageStorageService
{
    Result<string> CreateVoiceMessageAsync(string blob, CancellationToken ctx = default);
}
