<<<<<<< HEAD
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
=======
using Rise.Shared.Common;

>>>>>>> origin/main

namespace Rise.Shared.Chats;

public interface IChatService
{
<<<<<<< HEAD
    Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatDto.Index>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);
    Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default);
=======
    Task<ChatResponse.Index?> GetAllAsync();

>>>>>>> origin/main
}