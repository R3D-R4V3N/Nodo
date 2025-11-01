using System.Net.Http.Json;
using Rise.Shared.Users;

namespace Rise.Client.Users;

public class UserService(HttpClient httpClient) : IUserService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<UserResponse.CurrentUser>>(
            $"api/users/{accountId}", cancellationToken);

        // gebruik de correcte generieke Error-methode
        return result ?? Result<UserResponse.CurrentUser>.Error("Kon de gebruikersinformatie niet laden.");
    }


    public Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}