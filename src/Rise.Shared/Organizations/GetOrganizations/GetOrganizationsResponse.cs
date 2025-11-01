namespace Rise.Shared.Organizations;

public static partial class OrganizationResponse
{
    public class GetOrganizations
    {
        public IReadOnlyCollection<OrganizationDto.Summary> Organizations { get; set; } = Array.Empty<OrganizationDto.Summary>();
    }
}
