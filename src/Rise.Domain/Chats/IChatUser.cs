namespace Rise.Domain.Chats;

public interface IChatUser
{
    List<IChat> Chats { get; set; }
}
