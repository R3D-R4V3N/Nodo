using Ardalis.Result;
using Rise.Services.FileServer;

namespace Rise.Storage.Messages;

public class MessageStorageService : IMessageStorageService
{
    public Result<string> CreateVoiceMessageAsync(string blob, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }
}
