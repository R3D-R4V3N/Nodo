using Rise.Shared.Organizations;

namespace Rise.Server.Endpoints.Organizations;

public class List(IOrganizationService organizationService) : EndpointWithoutRequest<Result<OrganizationResponse.GetOrganizations>>
{
    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
    }

    public override Task<Result<OrganizationResponse.GetOrganizations>> ExecuteAsync(CancellationToken ct)
    {
        return organizationService.GetOrganizationsAsync(ct);
    }
}
