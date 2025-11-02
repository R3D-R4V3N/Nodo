namespace Rise.Shared.Registrations;

public static class RegistrationRequest
{
    public record Approve(int SupervisorId);

    public record Reject(int SupervisorId);
}
