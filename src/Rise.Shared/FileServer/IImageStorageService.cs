namespace Rise.Services.FileServer;

public interface IImageStorageService
{
    Result<string> CreateImageAsync(string blob, CancellationToken ctx = default);
}
