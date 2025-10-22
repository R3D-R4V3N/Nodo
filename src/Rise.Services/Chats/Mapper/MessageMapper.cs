using Rise.Domain.Chats;
using Rise.Services.Users.Mapper;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
internal static class MessageMapper
{
    public static MessageDto.Chat ToChatDto(this Message message) =>
        new MessageDto.Chat
        {
            ChatId = message.Chat.Id,
            Id = message.Id,
            Content = message.Text ?? string.Empty,
            Timestamp = message.CreatedAt,
            User = UserMapper.ToMessageDto(message.Sender),
            AudioDataUrl = AudioHelperMethods.BuildAudioDataUrl(message),
            AudioDuration = message.AudioDurationSeconds.HasValue
                ? TimeSpan.FromSeconds(message.AudioDurationSeconds.Value)
                : null,
        };
}
