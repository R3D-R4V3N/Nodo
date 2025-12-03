using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users;
using Rise.Domain.Users.Settings;
using Rise.Shared.Validators;

namespace Rise.Services.Validators;
public class ValidatorService : IValidatorService
{
    public Task<ValidatorRules> GetRulesAsync(CancellationToken ctx = default)
    {
        var rules = new ValidatorRules
        {
            MAX_AVATAR_URL_LENGTH = AvatarUrl.MAX_LENGTH,
            MAX_BIOGRAPHY_LENGTH = Biography.MAX_LENGTH,
            MAX_DEFAULT_CHAT_LINE_LENGTH = DefaultSentence.MAX_LENGTH,
            MAX_DEFAULT_CHAT_LINES_COUNT = UserSetting.MAX_DEFAULT_CHAT_LINE_COUNT,
            MAX_EMAIL_LENGTH = Email.MAX_LENGTH,
            MAX_FIRSTNAME_LENGTH = FirstName.MAX_LENGTH,
            MAX_FONT_SIZE = FontSize.MAX_FONT_SIZE,
            MIN_FONT_SIZE = FontSize.MIN_FONT_SIZE,
            MAX_LASTNAME_LENGTH = LastName.MAX_LENGTH,
            MAX_REGISTRATION_NOTE_LENGTH = RegistrationNote.MAX_LENGTH,
            MAX_TEXT_MESSAGE_LENGTH = TextMessage.MAX_LENGTH,
            MAX_HOBBIES_COUNT = User.MAX_HOBBIES,
            MAX_SENTIMENTS_PER_TYPE = User.MAX_SENTIMENTS_PER_TYPE,
        };

        return Task.FromResult(rules);
    }
}
