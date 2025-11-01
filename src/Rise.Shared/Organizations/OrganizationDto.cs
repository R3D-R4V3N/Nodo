namespace Rise.Shared.Organizations;

public static class OrganizationDto
{
    public class Summary
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
