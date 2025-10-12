using Ardalis.Result;
using Rise.Shared.Organizations;

namespace Rise.Server.Endpoints.Organizations;

public class Index(IOrganizationService organizationService) : EndpointWithoutRequest<Result<OrganizationResponse.List>>
{
    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
    }

    public override Task<Result<OrganizationResponse.List>> ExecuteAsync(CancellationToken ct)
        => organizationService.GetAllAsync(ct);
}
