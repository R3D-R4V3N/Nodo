namespace Rise.Services.BlobStorage;

public interface IBlobStorageService
{
    Task<Result<string>> GetBlobLinkAsync(string name, string container, CancellationToken ctx = default);
    Task<Result<string>> CreateBlobAsync(string name, string base64Data, string container, CancellationToken ctx = default);
}
