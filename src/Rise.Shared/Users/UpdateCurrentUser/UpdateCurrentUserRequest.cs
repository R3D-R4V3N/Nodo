using Rise.Shared.BlobStorage;
using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Validators;

namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateCurrentUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public BlobDto.Create? AvatarBlob { get; set; }
        public GenderTypeDto Gender { get; set; }
        public List<HobbyDto.EditProfile> Hobbies { get; set; } = [];
        public List<SentimentDto.EditProfile> Sentiments { get; set; } = [];
        public List<string> DefaultChatLines { get; set; } = [];
    }

    public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUser>
    {
        public UpdateCurrentUserValidator(ValidatorRules rules)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("Voornaam mag niet leeg zijn.")
                .MaximumLength(rules.MAX_FIRSTNAME_LENGTH)
                .WithMessage($"Voornaam heeft max {rules.MAX_FIRSTNAME_LENGTH} karakters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Achternaam mag niet leeg zijn")
                .MaximumLength(rules.MAX_LASTNAME_LENGTH)
                .WithMessage($"Achternaam heeft max {rules.MAX_LASTNAME_LENGTH} karakters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email mag niet leeg zijn")
                .EmailAddress()
                .WithMessage("Email moet een geldig formaat zijn")
                .MaximumLength(rules.MAX_EMAIL_LENGTH)
                .WithMessage($"Email heeft max {rules.MAX_EMAIL_LENGTH} karakters");

            RuleFor(x => x.Biography)
                .NotEmpty()
                .WithMessage("Bio mag niet leeg zijn.")
                .MaximumLength(rules.MAX_BIOGRAPHY_LENGTH)
                .WithMessage($"Bio heeft max {rules.MAX_EMAIL_LENGTH} karakters");

            RuleFor(x => x.AvatarBlob)
                .NotNull()
                .WithMessage("Avatar mag niet leeg zijn.");

            RuleFor(x => x.Hobbies)
                .Must(list => (list?.Select(x => x.Hobby) ?? []).Distinct().Count() <= rules.MAX_HOBBIES_COUNT)
                .WithMessage($"Je mag maximaal {rules.MAX_HOBBIES_COUNT} hobby's selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Like)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= rules.MAX_SENTIMENTS_PER_TYPE)
                .WithMessage($"Je mag maximaal {rules.MAX_SENTIMENTS_PER_TYPE} interesses selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Dislike)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= rules.MAX_SENTIMENTS_PER_TYPE)
                .WithMessage($"Je mag maximaal {rules.MAX_SENTIMENTS_PER_TYPE} interesses selecteren.");

            RuleFor(x => x.DefaultChatLines)
                .Must(list => (list ?? []).Count <= rules.MAX_DEFAULT_CHAT_LINES_COUNT)
                .WithMessage($"Je mag maximaal {rules.MAX_DEFAULT_CHAT_LINES_COUNT} standaardzinnen selecteren.");

            RuleForEach(x => x.DefaultChatLines)
                .NotEmpty()
                .WithMessage($"Standaard zinnen mag niet leeg zijn.")
                .MaximumLength(rules.MAX_DEFAULT_CHAT_LINE_LENGTH)
                .WithMessage($"Standaard zin mag max {rules.MAX_DEFAULT_CHAT_LINE_LENGTH} karakters hebben.");
        }
    }
}
