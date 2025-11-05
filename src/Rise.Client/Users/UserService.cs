using Rise.Shared.Users;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Rise.Client.Users;

public class UserService(HttpClient httpClient) : IUserService
{
    private readonly HttpClient _http = httpClient;
    public async Task<Result<UserResponse.CurrentUser>> UpdateUserAsync(string accountId, UserRequest.UpdateCurrentUser request, CancellationToken ctx = default)
    {
        var response = await _http.PutAsJsonAsync($"/api/users/{accountId}", request, ctx);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error(error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserResponse.CurrentUser>>(cancellationToken: ctx);

        if (result is null)
        {
            return Result.Error("Kon het serverantwoord niet verwerken.");
        }

        return result;
    }
    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var result = await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>(
            $"api/users/{accountId}", cancellationToken);

        // gebruik de correcte generieke Error-methode
        return result ?? Result<UserResponse.CurrentUser>.Error("Kon de gebruikersinformatie niet laden.");
    }
}