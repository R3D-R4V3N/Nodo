namespace Rise.Shared.Registrations;

public static class RegistrationResponse
{
    public record SupervisorOption(int Id, string Name);

    public record ListItem(
        int Id,
        string FirstName,
        string LastName,
        string Email,
        int OrganizationId,
        string OrganizationName,
        string Status,
        int? AssignedSupervisorId,
        string? AssignedSupervisorName,
        IReadOnlyList<SupervisorOption> Supervisors);
}
