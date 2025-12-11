using Rise.Shared.BlobStorage;

using Rise.Shared.Validators;

namespace Rise.Shared.Chats;

public static partial class ChatRequest
{
    public class CreateMessage
    {
        public int ChatId { get; set; }
        public string? Content { get; set; }
        public BlobDto.Create? AudioDataBlob { get; set; }
        public double? AudioDurationSeconds { get; set; }

        public class Validator : AbstractValidator<CreateMessage>
        {
            public Validator(ValidatorRules rules)
            {
                RuleFor(x => x.ChatId).GreaterThan(0);
                RuleFor(x => x.Content)
                    .MaximumLength(rules.MAX_TEXT_MESSAGE_LENGTH);

                RuleFor(x => x)
                    .Must(request =>
                        !string.IsNullOrWhiteSpace(request.Content) ||
                        request.AudioDataBlob is not null)
                    .WithMessage("Een bericht moet tekst of audio bevatten.");

                RuleFor(x => x.AudioDurationSeconds)
                    .GreaterThan(0)
                    .When(x => x.AudioDataBlob is not null);
            }
        }
    }
}
