using Rise.Shared.Common;


namespace Rise.Shared.Chats;

public interface IChatService
{
    Task<ChatResponse.Index?> GetAllAsync();

}