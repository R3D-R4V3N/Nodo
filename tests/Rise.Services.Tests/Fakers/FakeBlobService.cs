using Ardalis.Result;
using Rise.Services.BlobStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Services.Tests.Fakers;
public class FakeBlobService : IBlobStorageService
{
    public Task<Result<string>> CreateBlobAsync(string name, string base64Data, string container, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> GetBlobLinkAsync(string name, string container, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }
}
