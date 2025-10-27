namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateCurrentUser
    {
        public const int MaxHobbies = 3;

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public List<string> HobbyIds { get; set; } = [];
        public List<string> Likes { get; set; } = [];
        public List<string> Dislikes { get; set; } = [];
        public List<string> DefaultChatLines { get; set; } = [];
    }

    public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUser>
    {
        private const int MaxNameLength = 200;
        private const int MaxBiographyLength = 500;
        private const int MaxAvatarLength = 250;
        private const int MaxDefaultChatLines = 5;
        private const int MaxPreferences = 5;
        private const int MaxChatLineLength = 150;

        public UpdateCurrentUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(MaxNameLength)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Naam mag niet leeg zijn.");

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

            RuleFor(x => x.HobbyIds)
                .Must(list => (list ?? []).Distinct(StringComparer.OrdinalIgnoreCase).Count() <= UpdateCurrentUser.MaxHobbies)
                .WithMessage($"Je mag maximaal {UpdateCurrentUser.MaxHobbies} hobby's selecteren.");

            RuleFor(x => x.Likes)
                .Must(list => (list ?? []).Distinct(StringComparer.OrdinalIgnoreCase).Count() <= MaxPreferences)
                .WithMessage($"Je mag maximaal {MaxPreferences} interesses selecteren.");

            RuleFor(x => x.Dislikes)
                .Must(list => (list ?? []).Distinct(StringComparer.OrdinalIgnoreCase).Count() <= MaxPreferences)
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
