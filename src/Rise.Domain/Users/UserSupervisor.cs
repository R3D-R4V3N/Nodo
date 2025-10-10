using Ardalis.GuardClauses;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public class UserSupervisor : Entity
{
    public int ChatUserId { get; private set; }

    public ApplicationUser ChatUser { get; private set; } = null!;

    public int SupervisorId { get; private set; }

    public ApplicationUser Supervisor { get; private set; } = null!;

    private UserSupervisor()
    {
    }

    public UserSupervisor(ApplicationUser chatUser, ApplicationUser supervisor)
    {
        ChatUser = Guard.Against.Null(chatUser);
        Supervisor = Guard.Against.Null(supervisor);

        ChatUserId = chatUser.Id;
        SupervisorId = supervisor.Id;
    }
}
