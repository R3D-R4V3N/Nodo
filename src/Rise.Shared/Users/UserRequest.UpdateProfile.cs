namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateProfile
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        public List<string> Likes { get; init; } = [];
        public List<string> Dislikes { get; init; } = [];
        public List<string> Hobbies { get; init; } = [];
        public List<string> DefaultChatLines { get; init; } = [];

        public class Validator : AbstractValidator<UpdateProfile>
        {
            private const int MaxNameLength = 200;
            private const int MaxBiographyLength = 500;
            private const int MaxAvatarLength = 250;
            private const int MaxPreferenceCount = 5;
            private const int MaxHobbyCount = 3;
            private const int MaxChatLineCount = 5;

            public Validator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(MaxNameLength);

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress();

                RuleFor(x => x.Bio)
                    .NotEmpty()
                    .MaximumLength(MaxBiographyLength);

                RuleFor(x => x.AvatarUrl)
                    .NotEmpty()
                    .MaximumLength(MaxAvatarLength);

                RuleFor(x => x.Likes)
                    .Must(list => list.Count <= MaxPreferenceCount)
                    .WithMessage($"Je kan maximaal {MaxPreferenceCount} interesses opslaan.");

                RuleFor(x => x.Dislikes)
                    .Must(list => list.Count <= MaxPreferenceCount)
                    .WithMessage($"Je kan maximaal {MaxPreferenceCount} afknappers opslaan.");

                RuleFor(x => x.Hobbies)
                    .Must(list => list.Count <= MaxHobbyCount)
                    .WithMessage($"Je kan maximaal {MaxHobbyCount} hobby's opslaan.");

                RuleFor(x => x.DefaultChatLines)
                    .Must(list => list.Count <= MaxChatLineCount)
                    .WithMessage($"Je kan maximaal {MaxChatLineCount} standaardzinnen opslaan.");

                RuleForEach(x => x.Likes)
                    .NotEmpty();

                RuleForEach(x => x.Dislikes)
                    .NotEmpty();

                RuleForEach(x => x.Hobbies)
                    .NotEmpty();

                RuleForEach(x => x.DefaultChatLines)
                    .NotEmpty();
            }
        }
    }
}
