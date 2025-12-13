using Rise.Shared.Validators;

namespace Rise.Server.Endpoints.Validators;
public class GetValidatorRules(IValidatorService validatorService) : EndpointWithoutRequest<ValidatorRules>
{
    public override void Configure()
    {
        Get("/api/config/rules");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get validator rules";
            s.Description = "Provides the validation rules and constraints used by the client.";
        });
    }

    public override Task<ValidatorRules> ExecuteAsync(CancellationToken ctx)
    {
        return validatorService.GetRulesAsync(ctx);
    }
}