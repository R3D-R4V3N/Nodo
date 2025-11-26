using Ardalis.Result;
using Rise.Services.FileServer;

namespace Rise.Storage.Images;

public class ImageStorageService : IImageStorageService
{
    public Result<string> CreateImageAsync(string blob, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }
}
