using Rise.Domain.Chats;
<<<<<<< HEAD
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
=======
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
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        };
}
