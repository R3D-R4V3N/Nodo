namespace Rise.Shared.Validators;
public interface IValidatorService
{
    Task<ValidatorRules> GetRulesAsync(CancellationToken ctx = default);
}
