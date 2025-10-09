using FluentValidation;

namespace Rise.Shared.Chats;

public static partial class ChatRequest
{
    public class CreateMessage
    {
        public int ChatId { get; set; }
        public string? Content { get; set; }

        public class Validator : AbstractValidator<CreateMessage>
        {
            public Validator()
            {
                RuleFor(x => x.ChatId).GreaterThan(0);
                RuleFor(x => x.Content)
                    .NotEmpty()
                    .Must(content => !string.IsNullOrWhiteSpace(content))
                    .WithMessage("Berichtinhoud mag niet leeg zijn.")
                    .MaximumLength(2_000);
            }
        }
    }
}
