using FluentValidation;
using Rise.Shared.Validators;

namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdateDefaultChatLines
    {
        public List<string> DefaultChatLines { get; set; } = [];
    }

    public class UpdateDefaultChatLinesValidator : AbstractValidator<UpdateDefaultChatLines>
    {
        public UpdateDefaultChatLinesValidator(ValidatorRules rules)
        {
            RuleFor(x => x.DefaultChatLines)
                .Must(list => (list ?? []).Count <= rules.MAX_DEFAULT_CHAT_LINES_COUNT)
                .WithMessage($"Je mag maximaal {rules.MAX_DEFAULT_CHAT_LINES_COUNT} standaardzinnen selecteren.");

            RuleForEach(x => x.DefaultChatLines)
                .NotEmpty()
                .MaximumLength(rules.MAX_DEFAULT_CHAT_LINE_LENGTH);
        }
    }
}
