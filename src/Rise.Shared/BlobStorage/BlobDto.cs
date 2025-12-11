namespace Rise.Shared.BlobStorage;
public static class BlobDto
{
    public class Create
    {
        public required string Name { get; set; }
        public required string Base64Data { get; set; }
    }
}
