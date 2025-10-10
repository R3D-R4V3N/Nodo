using FluentValidation;

namespace Rise.Shared.Chats;

public static partial class ChatRequest
{
    public class ToggleEmergency
    {
        public int ChatId { get; set; }

        public class Validator : AbstractValidator<ToggleEmergency>
        {
            public Validator()
            {
                RuleFor(x => x.ChatId).GreaterThan(0);
            }
        }
    }
}
