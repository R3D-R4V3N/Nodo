namespace Rise.Shared.RegistrationRequests;

public static class RegistrationRequestResponse
{
    public record PendingList
    {
        public IReadOnlyList<RegistrationRequestDto.Pending> Requests { get; init; } = [];
    }

    public record Approve
    {
        public int RegistrationRequestId { get; init; }
    }
}
