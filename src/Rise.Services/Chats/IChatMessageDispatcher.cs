namespace Rise.Services.Chats;

using System.Threading;
using System.Threading.Tasks;
using Rise.Shared.Chats;

public interface IChatMessageDispatcher
{
    Task NotifyMessageCreatedAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default);
}
