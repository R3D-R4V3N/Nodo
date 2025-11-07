namespace Rise.Shared.Registrations;

public static class RegistrationDto
{
    public record Pending
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string OrganizationName { get; init; } = string.Empty;
        public string? AssignedSupervisorName { get; init; }
        public string? AssignedSupervisorAccountId { get; init; }
        public DateTime RequestedAt { get; init; }

        public string FullName => string.Join(' ', new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
