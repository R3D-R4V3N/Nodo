using Ardalis.Result;
using Rise.Services.Organizations;
using Rise.Shared.Organizations;

namespace Rise.Server.Endpoints.Organizations;

public class List(IOrganizationService organizationService) : EndpointWithoutRequest<Result<IReadOnlyCollection<OrganizationDto.ListItem>>>
{
    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
    }

    public override async Task<Result<IReadOnlyCollection<OrganizationDto.ListItem>>> ExecuteAsync(CancellationToken ct)
    {
        return await organizationService.GetOrganizationsAsync(ct);
    }
}
