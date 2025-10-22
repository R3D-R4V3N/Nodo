namespace Rise.Shared.Chats;

public interface IChatService
{
    Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatDto.GetChats>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);
    Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default);
}