using FluentValidation;

namespace Rise.Shared.RegistrationRequests;

public static class RegistrationRequestRequest
{
    public class Approve
    {
        public int AssignedSupervisorId { get; set; }
    }

    public class ApproveValidator : AbstractValidator<Approve>
    {
        public ApproveValidator()
        {
            RuleFor(x => x.AssignedSupervisorId)
                .GreaterThan(0)
                .WithMessage("Selecteer een begeleider.");
        }
    }
}
