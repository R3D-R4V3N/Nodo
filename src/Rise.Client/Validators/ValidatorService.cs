using Rise.Shared.Validators;
using System.Net.Http.Json;

namespace Rise.Client.Validators;

public class ValidatorService(HttpClient httpClient) : IValidatorService
{
    public Task<ValidatorRules> GetRulesAsync(CancellationToken ctx = default)
        => httpClient.GetFromJsonAsync<ValidatorRules>("/api/config/rules", cancellationToken: ctx)!;
}
