using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;

namespace Rise.Shared.Chats;

public interface IChatService
{
    Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatDto.Index>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);
    Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default);
}