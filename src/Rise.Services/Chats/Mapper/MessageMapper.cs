using Rise.Domain.Messages;
using Rise.Services.Users.Mapper;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
internal static class MessageMapper
{
    public static MessageDto.Chat? ToChatDto(this Message message)
    {
        if (message is null)
            return null;

        return new MessageDto.Chat
        {
            ChatId = message.Chat.Id,
            Id = message.Id,
            Content = message.Text?.CleanedUpValue ?? string.Empty,
            Timestamp = message.CreatedAt,
            User = UserMapper.ToMessageDto(message.Sender),
            AudioDataBlob = AudioHelperMethods.BuildAudioDataUrl(message),
            AudioDuration = message.AudioDurationSeconds.HasValue
                ? TimeSpan.FromSeconds(message.AudioDurationSeconds.Value)
                : null,
        };
    }
}
