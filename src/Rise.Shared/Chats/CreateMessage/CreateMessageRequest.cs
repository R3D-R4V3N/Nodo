namespace Rise.Shared.Chats;

public static partial class ChatRequest
{
    public class CreateMessage
    {
        public int ChatId { get; set; }
        public string? Content { get; set; }
        public string? AudioDataUrl { get; set; }
        public double? AudioDurationSeconds { get; set; }

        public class Validator : AbstractValidator<CreateMessage>
        {
            public Validator()
            {
                RuleFor(x => x.ChatId).GreaterThan(0);
                RuleFor(x => x.Content)
                    .MaximumLength(2_000);

                RuleFor(x => x)
                    .Must(request =>
                        !string.IsNullOrWhiteSpace(request.Content) ||
                        !string.IsNullOrWhiteSpace(request.AudioDataUrl))
                    .WithMessage("Een bericht moet tekst of audio bevatten.");

                RuleFor(x => x.AudioDurationSeconds)
                    .GreaterThan(0)
                    .When(x => !string.IsNullOrWhiteSpace(x.AudioDataUrl));
            }
        }
    }
}
