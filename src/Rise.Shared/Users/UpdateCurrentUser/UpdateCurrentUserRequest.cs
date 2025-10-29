namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateCurrentUser
    {
        public const int MaxHobbies = 3;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Gender { get; set; } = "x";
        public List<HobbyDto.EditProfile> Hobbies { get; set; } = [];
        public List<SentimentDto.EditProfile> Sentiments { get; set; } = [];
        public List<string> DefaultChatLines { get; set; } = [];
    }

    public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUser>
    {
        private const int MaxNameLength = 200;
        private const int MaxBiographyLength = 500;
        private const int MaxAvatarLength = 250;
        private static readonly string[] AllowedGenders = ["man", "vrouw", "x"];
        private const int MaxDefaultChatLines = 5;
        private const int MaxPreferences = 5;
        private const int MaxChatLineLength = 150;

        public UpdateCurrentUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(MaxNameLength)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Voornaam mag niet leeg zijn.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(MaxNameLength)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Achternaam mag niet leeg zijn.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Biography)
                .NotEmpty()
                .MaximumLength(MaxBiographyLength)
                .Must(bio => !string.IsNullOrWhiteSpace(bio))
                .WithMessage("Bio mag niet leeg zijn.");

            RuleFor(x => x.AvatarUrl)
                .NotEmpty()
                .MaximumLength(MaxAvatarLength)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                .WithMessage("Avatar mag niet leeg zijn.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .Must(value => AllowedGenders.Contains(value.Trim().ToLowerInvariant()))
                .WithMessage("Ongeldig geslacht. Kies uit 'man', 'vrouw' of 'x'.");

            RuleFor(x => x.Hobbies)
                .Must(list => (list?.Select(x => x.Hobby) ?? []).Distinct().Count() <= UpdateCurrentUser.MaxHobbies)
                .WithMessage($"Je mag maximaal {UpdateCurrentUser.MaxHobbies} hobby's selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Like)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= MaxPreferences)
                .WithMessage($"Je mag maximaal {MaxPreferences} interesses selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Dislike)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= MaxPreferences)
                .WithMessage($"Je mag maximaal {MaxPreferences} interesses selecteren.");

            RuleFor(x => x.DefaultChatLines)
                .Must(list => (list ?? []).Count <= MaxDefaultChatLines)
                .WithMessage($"Je mag maximaal {MaxDefaultChatLines} standaardzinnen selecteren.");

            RuleForEach(x => x.DefaultChatLines)
                .NotEmpty()
                .MaximumLength(MaxChatLineLength);
        }
    }
}
