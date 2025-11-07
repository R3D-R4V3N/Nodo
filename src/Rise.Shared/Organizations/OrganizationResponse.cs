namespace Rise.Shared.Organizations;

public static class OrganizationResponse
{
    public record List
    {
        public IReadOnlyList<OrganizationDto.Summary> Organizations { get; init; } = [];
    }
}
