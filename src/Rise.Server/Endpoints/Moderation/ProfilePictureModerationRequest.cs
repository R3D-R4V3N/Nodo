using Microsoft.AspNetCore.Http;

namespace Rise.Server.Endpoints.Moderation;

public class ProfilePictureModerationRequest
{
    public IFormFile? File { get; set; }
}
