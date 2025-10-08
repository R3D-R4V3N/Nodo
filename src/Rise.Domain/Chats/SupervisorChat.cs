using Rise.Domain.Supervisors;

namespace Rise.Domain.Chats;

public class SupervisorChat : Chat
{
    public required Supervisor Supervisor { get; set; }
}
