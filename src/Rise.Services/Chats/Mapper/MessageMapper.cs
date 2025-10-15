using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
public static class MessageMapper
{
    public static MessageDto MapToDto(this Message message)
    {
        var sender = message.Sender ?? throw new InvalidOperationException("Message sender must be loaded.");
        return MapToDto(message, sender);
    }

    public static MessageDto MapToDto(this Message message, ApplicationUser sender) =>
        new MessageDto
        {
            ChatId = message.ChatId,
            Id = message.Id,
            Content = message.Inhoud ?? string.Empty,
            Timestamp = message.CreatedAt,
            SenderId = message.SenderId,
            SenderName = $"{sender.FirstName} {sender.LastName}",
            SenderAccountId = sender.AccountId,
            AudioDataUrl = AudioHelperMethods.BuildAudioDataUrl(message),
            AudioDurationSeconds = message.AudioDurationSeconds
        };
}
