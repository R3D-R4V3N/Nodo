using System.Collections.Generic;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(
        QueryRequest.SkipTake request, 
        CancellationToken ctx = default
    )
    {
        var url = BuildQueryUrl("/api/connections/friends", request);
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>(url, cancellationToken: ctx);

        return result ?? Result<UserConnectionResponse.GetFriends>.Error("Kon de vriendenlijst niet ophalen.");
    }

    public async Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var body = new { targetAccountId = targetAccountId };

        var response = await httpClient.PostAsJsonAsync("/api/connections/add", body, ctx);

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ctx);
        return result ?? Result<string>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<string>> AcceptFriendAsync(string requesterAccountId, CancellationToken ctx = default)
    {
        var body = new { requesterAccountId };

        var response = await httpClient.PostAsJsonAsync("/api/connections/accept", body, ctx);

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ctx);
        return result ?? Result<string>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var url = BuildQueryUrl("/api/connections/suggested", req);
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>(url, cancellationToken: ct);

        return result ?? Result<UserConnectionResponse.GetSuggestions>.Error("Kon de vriendensuggesties niet ophalen.");
    }

    public async Task<Result<string>> RejectFriendAsync(string reqRequesterAccountId, CancellationToken ct = default)
    {
        var body = new { RequesterAccountId = reqRequesterAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/reject", body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het afwijzen van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ct);
        return result ?? Result<string>.Error("Kon het serverantwoord niet verwerken.");

    }

    private static string BuildQueryUrl(string baseUrl, QueryRequest.SkipTake request)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["Skip"] = request.Skip.ToString(),
            ["Take"] = request.Take.ToString(),
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            queryParams["SearchTerm"] = request.SearchTerm;
        }

        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            queryParams["OrderBy"] = request.OrderBy;
        }

        if (request.OrderDescending)
        {
            queryParams["OrderDescending"] = bool.TrueString;
        }

        return QueryHelpers.AddQueryString(baseUrl, queryParams);
    }
}
