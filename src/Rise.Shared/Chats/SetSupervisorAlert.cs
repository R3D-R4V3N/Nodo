using FluentValidation;

namespace Rise.Shared.Chats;

public static partial class ChatRequest
{
    public class SetSupervisorAlert
    {
        public int ChatId { get; set; }
        public bool Enable { get; set; }

        public class Validator : AbstractValidator<SetSupervisorAlert>
        {
            public Validator()
            {
                RuleFor(x => x.ChatId).GreaterThan(0);
            }
        }
    }
}
