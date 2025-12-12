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
                .MaximumLength(rules.MAX_FIRSTNAME_LENGTH)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Voornaam mag niet leeg zijn.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(rules.MAX_LASTNAME_LENGTH)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Achternaam mag niet leeg zijn.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(rules.MAX_EMAIL_LENGTH);

            RuleFor(x => x.Biography)
                .NotEmpty()
                .MaximumLength(rules.MAX_BIOGRAPHY_LENGTH)
                .Must(bio => !string.IsNullOrWhiteSpace(bio))
                .WithMessage("Bio mag niet leeg zijn.");

            RuleFor(x => x.AvatarBlob)
                .Must(blob => blob is null ||
                              (!string.IsNullOrWhiteSpace(blob.Name) && !string.IsNullOrWhiteSpace(blob.Base64Data)))
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
                .MaximumLength(rules.MAX_DEFAULT_CHAT_LINE_LENGTH);
        }
    }
}
