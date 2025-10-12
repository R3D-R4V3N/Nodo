namespace Rise.Shared.Organizations;

public static class OrganizationDto
{
    public record Index
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
