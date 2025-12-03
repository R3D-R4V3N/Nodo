using Rise.Shared.Validators;

namespace Rise.Server.Endpoints.Validators;
public class GetValidatorRules(IValidatorService validatorService) : EndpointWithoutRequest<ValidatorRules>
{
    public override void Configure()
    {
        Get("/api/config/rules");
        AllowAnonymous();
    }

    public override Task<ValidatorRules> ExecuteAsync(CancellationToken ctx)
    {
        return validatorService.GetRulesAsync(ctx);
    }
}