using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Validators;

namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdatePreferences
    {
        public List<HobbyDto.EditProfile> Hobbies { get; set; } = [];
        public List<SentimentDto.EditProfile> Sentiments { get; set; } = [];
    }

    public class UpdatePreferencesValidator : AbstractValidator<UpdatePreferences>
    {
        public UpdatePreferencesValidator(ValidatorRules rules)
        {
            RuleFor(x => x.Hobbies)
                .Must(list => (list?.Select(h => h.Hobby) ?? []).Distinct().Count() <= rules.MAX_HOBBIES_COUNT)
                .WithMessage($"Je mag maximaal {rules.MAX_HOBBIES_COUNT} hobby's selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Like)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= rules.MAX_SENTIMENTS_PER_TYPE)
                .WithMessage($"Je mag maximaal {rules.MAX_SENTIMENTS_PER_TYPE} interesses selecteren.");

            RuleFor(x => x.Sentiments.Where(x => x.Type.Equals(SentimentTypeDto.Dislike)))
                .Must(list => (list?.Select(x => x.Category) ?? []).Distinct().Count() <= rules.MAX_SENTIMENTS_PER_TYPE)
                .WithMessage($"Je mag maximaal {rules.MAX_SENTIMENTS_PER_TYPE} interesses selecteren.");
        }
    }
}
