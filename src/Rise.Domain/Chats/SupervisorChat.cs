using Rise.Domain.Message;
using Rise.Domain.Supervisors;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class SupervisorChat : Entity, IChat
{
    public required Supervisor Supervisor { get; set; }
    public List<IChatUser> Users { get; set; } = [];
    public List<IMessage> Messages { get; set; } = [];
}
