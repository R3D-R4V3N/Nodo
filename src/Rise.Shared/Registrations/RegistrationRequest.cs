namespace Rise.Shared.Registrations;

public static class RegistrationRequest
{
    public class Get
    {
        public int RegistrationId { get; set; }
    }

    public class Approve
    {
        public int RegistrationId { get; set; }
        public int SupervisorId { get; set; }
        public string? Feedback { get; set; }
    }

    public class Reject
    {
        public int RegistrationId { get; set; }
        public int SupervisorId { get; set; }
        public string? Feedback { get; set; }
    }
}
