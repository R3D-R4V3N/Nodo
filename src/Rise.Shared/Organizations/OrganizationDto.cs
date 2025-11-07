namespace Rise.Shared.Organizations;

public static class OrganizationDto
{
    public record ListItem
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
    }
}
