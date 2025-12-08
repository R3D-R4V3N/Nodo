using Rise.Shared.Common;

namespace Rise.Shared.Chats;

public interface IChatService
{
    Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);
    Task<Result<ChatResponse.GetSupervisorChat>> GetSupervisorChatAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatResponse.GetMessages>> GetMessagesAsync(int chatId, QueryRequest.SkipTake request, CancellationToken cancellationToken = default);
    Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default);
    Task<Result<int>> QueueMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default);
}