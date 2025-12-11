using Ardalis.Result;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Rise.Services.BlobStorage;


namespace Rise.Storage;

public class BlobStorageService(BlobServiceClient blobServiceClient) : IBlobStorageService
{
    public async Task<Result<string>> GetBlobLinkAsync(string name, string container, CancellationToken ctx = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Conflict("Blob naam mag niet leeg zijn.");

        if (string.IsNullOrWhiteSpace(container))
            return Result.Conflict("Container naam mag niet leeg zijn.");

        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobClient = containerClient.GetBlobClient(name);

        if (!await blobClient.ExistsAsync(ctx))
            return Result.NotFound();

        return blobClient.Uri.ToString();
    }
    public async Task<Result<string>> CreateBlobAsync(string name, string base64Data, string container, CancellationToken ctx = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Conflict("Blob naam mag niet leeg zijn.");

        if (string.IsNullOrWhiteSpace(base64Data))
            return Result.Conflict("base 64 data mag niet leeg zijn.");

        if (string.IsNullOrWhiteSpace(container))
            return Result.Conflict("Container naam mag niet leeg zijn.");


        var extension = Path.GetExtension(name);

        var containerClient = blobServiceClient.GetBlobContainerClient(container);
                
        string newName = $"{Guid.CreateVersion7():N}{extension}";
        var blobClient = containerClient.GetBlobClient(newName);

        string contentType = string.Empty;
        byte[] fileBytes;

        try
        {
            var commaIdx = base64Data.IndexOf(',');

            var metadata = base64Data.Substring(0, commaIdx);

            var ctStart = metadata.IndexOf(':') + 1;
            var ctEnd = metadata.IndexOf(';');

            contentType = metadata.Substring(ctStart, ctEnd - ctStart);
            base64Data = base64Data.Substring(commaIdx + 1);

            fileBytes = Convert.FromBase64String(base64Data);
        }
        catch
        {
            return Result.Conflict("Invalid base64 file data.");
        }

        var headers = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        using var stream = new MemoryStream(fileBytes);

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = headers },
            ctx
        );

        return blobClient.Uri.ToString();
    }
}
