namespace Rise.Shared.Registrations;

public static class RegistrationRequest
{
    public class AssignSupervisor
    {
        public int RegistrationId { get; set; }

        public class Validator : AbstractValidator<AssignSupervisor>
        {
            public Validator()
            {
                RuleFor(x => x.RegistrationId).GreaterThan(0);
            }
        }
    }

    public class Approve
    {
        public int RegistrationId { get; set; }

        public class Validator : AbstractValidator<Approve>
        {
            public Validator()
            {
                RuleFor(x => x.RegistrationId).GreaterThan(0);
            }
        }
    }
}
